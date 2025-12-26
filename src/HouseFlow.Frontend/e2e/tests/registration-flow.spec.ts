import { test, expect } from '@playwright/test';

test.describe('Complete Registration Flow', () => {
  test('User can register and is redirected to device creation', async ({ page }) => {
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

    // NEW FLOW: Wait for redirect to device creation page for auto-created house
    await page.waitForURL(/\/fr\/houses\/[^/]+\/devices\/new/, { timeout: 10000 });

    // Verify we're on the add device page
    await expect(page.getByRole('heading', { name: /ajouter un appareil/i })).toBeVisible({ timeout: 10000 });

    // Verify the page contains device type selection
    await expect(page.getByLabel(/type d'appareil/i)).toBeVisible();
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

    // Wait for device creation page after registration
    await page.waitForURL(/\/fr\/houses\/[^/]+\/devices\/new/);

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

    // NEW FLOW: User with one house is auto-redirected to house details
    await page.waitForURL(/\/fr\/houses\/[^/]+$/, { timeout: 10000 });

    // Verify we're on the house page
    expect(page.url()).toMatch(/\/fr\/houses\/[^/]+$/);

    // Verify house page is displayed with "Ma Maison" (auto-created house)
    await expect(page.getByRole('heading', { name: /ma maison/i })).toBeVisible({ timeout: 10000 });
  });
});
