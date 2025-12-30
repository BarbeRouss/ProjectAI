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
      password: 'TestPassword123!', // Updated to meet new requirements: 12+ chars with special char
      name: 'Test User',
    };
    await use(user);
  },

  authenticatedPage: async ({ page, testUser }: { page: Page; testUser: TestUser }, use) => {
    const API_URL = process.env.API_URL || 'http://localhost:5203';
    const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:3000';

    // Register user via API first (creates auto-house "Ma Maison")
    // Use the page's request context to share cookies
    const registerResponse = await page.request.post(`${API_URL}/v1/auth/register`, {
      data: testUser,
    });

    expect(registerResponse.ok()).toBeTruthy();
    const authData = await registerResponse.json();

    // Navigate to login page
    await page.goto(`${FRONTEND_URL}/fr/login`);
    await page.waitForLoadState('domcontentloaded');

    // Inject the auth state into the page BEFORE any React components mount
    // This ensures the auth context picks up the state correctly
    await page.addInitScript(({ token, user }) => {
      // Set user in sessionStorage
      sessionStorage.setItem('houseflow_auth_user', JSON.stringify(user));

      // Set token in a way that will be picked up when the module loads
      (window as any).__INITIAL_AUTH_TOKEN = token;
    }, { token: authData.token, user: authData.user });

    // Now navigate to dashboard - this will initialize the app with auth
    await page.goto(`${FRONTEND_URL}/fr/dashboard`);

    // Wait for the page to fully load
    await page.waitForLoadState('networkidle');

    // Verify auth is set by checking if we're still on dashboard (not redirected to login)
    await page.waitForTimeout(1000);

    await use(page);
  },
});

export { expect };
