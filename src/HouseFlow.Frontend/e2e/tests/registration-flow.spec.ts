import { test, expect } from '@playwright/test';

test.describe('Complete Registration Flow', () => {
  test('User can register and is redirected to dashboard', async ({ page }) => {
    // Navigate to register page
    await page.goto('http://localhost:3000/fr/register');

    // Wait for page to load
    await page.waitForLoadState('networkidle');

    // Generate unique email
    const timestamp = Date.now();
    const email = `testuser${timestamp}@houseflow.test`;
    const password = 'TestPassword123';
    const name = 'Test User';

    // Fill registration form
    await page.getByPlaceholder('Jean Dupont').fill(name);
    await page.getByPlaceholder('you@example.com').fill(email);
    await page.getByPlaceholder('••••••••').fill(password);

    // Submit form
    await page.getByRole('button', { name: /s'inscrire|sign up/i }).click();

    // Wait for redirect to dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 });

    // Verify we're on dashboard
    expect(page.url()).toContain('/dashboard');

    // Verify welcome message is displayed
    await expect(page.getByRole('heading', { name: /bienvenue|welcome/i })).toBeVisible({ timeout: 10000 });

    // Verify user name appears somewhere on the page
    await expect(page.getByText(name)).toBeVisible();

    // Verify dashboard content is present
    await expect(page.getByText(/mes maisons|my houses/i)).toBeVisible();
  });

  test('User can login after registration', async ({ page }) => {
    // First, register a user
    const timestamp = Date.now();
    const email = `testuser${timestamp}@houseflow.test`;
    const password = 'TestPassword123';
    const name = 'Test User';

    await page.goto('http://localhost:3000/fr/register');
    await page.getByPlaceholder('Jean Dupont').fill(name);
    await page.getByPlaceholder('you@example.com').fill(email);
    await page.getByPlaceholder('••••••••').fill(password);
    await page.getByRole('button', { name: /s'inscrire/i }).click();

    // Wait for dashboard
    await page.waitForURL('**/dashboard');

    // Logout (simulate by clearing local storage and going to login)
    await page.evaluate(() => {
      localStorage.clear();
    });

    // Navigate to login page
    await page.goto('http://localhost:3000/fr/login');

    // Login with same credentials
    await page.getByPlaceholder('you@example.com').fill(email);
    await page.getByPlaceholder('••••••••').fill(password);
    await page.getByRole('button', { name: /se connecter|login/i }).click();

    // Verify redirect to dashboard
    await page.waitForURL('**/dashboard', { timeout: 10000 });
    expect(page.url()).toContain('/dashboard');

    // Verify user is logged in - check for welcome heading and user name
    await expect(page.getByRole('heading', { name: /bienvenue|welcome/i })).toBeVisible({ timeout: 10000 });
    await expect(page.getByText(name)).toBeVisible();
  });
});
