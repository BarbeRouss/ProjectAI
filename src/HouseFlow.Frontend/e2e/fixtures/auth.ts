import { test as base, expect, Page } from '@playwright/test';

/**
 * Generate a unique email for test isolation
 */
export function generateTestEmail(): string {
  return `test-${Date.now()}-${Math.random().toString(36).substring(7)}@houseflow.test`;
}

type TestUser = { email: string; password: string; name: string };

/**
 * Extended test fixture with authenticated user
 */
export const test = base.extend<{
  authenticatedPage: Page;
  testUser: TestUser;
}>({
  testUser: async ({}, use) => {
    const user: TestUser = {
      email: generateTestEmail(),
      password: 'TestPassword123',
      name: 'Test User',
    };
    await use(user);
  },

  authenticatedPage: async ({ page, testUser }: { page: Page; testUser: TestUser }, use) => {
    const API_URL = process.env.API_URL || 'http://localhost:5203';

    // Register user via API (creates auto-house "Ma Maison")
    const registerResponse = await page.request.post(`${API_URL}/v1/auth/register`, {
      data: testUser,
    });

    expect(registerResponse.ok()).toBeTruthy();
    const authData = await registerResponse.json();

    // Store token and user in localStorage
    await page.goto('http://localhost:3000/fr/login');
    await page.evaluate((token) => {
      localStorage.setItem('houseflow_auth_token', token);
    }, authData.token);

    await page.evaluate((user) => {
      localStorage.setItem('houseflow_auth_user', JSON.stringify(user));
    }, authData.user);

    // Navigate to dashboard
    await page.goto('http://localhost:3000/fr/dashboard');

    // Wait for page to load
    await page.waitForLoadState('domcontentloaded');

    // Users with one house are auto-redirected to /houses/{id}
    // Wait a bit for the redirect to happen
    await page.waitForTimeout(1000);

    await use(page);
  },
});

export { expect };
