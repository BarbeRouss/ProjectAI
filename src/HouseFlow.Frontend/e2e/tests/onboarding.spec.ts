import { test, expect } from '../fixtures/auth';
import { RegisterPage } from '../pages/register-page';
import { LoginPage } from '../pages/login-page';
import { DashboardPage } from '../pages/dashboard-page';
import { HousePage } from '../pages/house-page';

test.describe('User Flow 1: Onboarding (First Time Experience)', () => {
  test('Complete onboarding: Register → First House → Dashboard', async ({ page }) => {
    // ÉTAPE 1: Registration
    const registerPage = new RegisterPage(page);
    await registerPage.goto();

    const email = `newuser-${Date.now()}@houseflow.test`;
    const password = 'SecurePass123!';
    const name = 'Jean Dupont';

    await registerPage.register(name, email, password);
    await registerPage.expectRegisterSuccess();

    // ÉTAPE 2: User is on dashboard
    const dashboardPage = new DashboardPage(page);
    await expect(dashboardPage.welcomeHeading).toContainText(name);

    // ÉTAPE 3: Create first house
    await dashboardPage.clickAddHouse();

    const housePage = new HousePage(page);
    await housePage.createHouse(
      'Ma Maison Principale',
      '123 Rue de la Paix',
      '75001',
      'Paris'
    );
    await housePage.expectCreateSuccess();

    // ÉTAPE 4: Verify house appears in dashboard
    await dashboardPage.goto();
    await dashboardPage.expectHouseCount(1);
    await expect(page.getByText('Ma Maison Principale')).toBeVisible();
  });

  test('Login after registration should work', async ({ page, testUser }) => {
    // First register a user via fixture (already done in testUser)
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Clear any existing auth
    await page.evaluate(() => {
      localStorage.removeItem('houseflow_auth_token');
      localStorage.removeItem('houseflow_auth_user');
    });

    // Login with the test user
    await loginPage.login(testUser.email, testUser.password);
    await loginPage.expectLoginSuccess();

    const dashboardPage = new DashboardPage(page);
    await expect(dashboardPage.welcomeHeading).toBeVisible();
  });

  test('Invalid login credentials should fail', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    await loginPage.login('nonexistent@test.com', 'WrongPassword!');
    await loginPage.expectLoginError();
  });

  test('Duplicate email registration should fail', async ({ page, testUser }) => {
    const registerPage = new RegisterPage(page);
    await registerPage.goto();

    // Try to register with an email that already exists (from testUser fixture)
    await registerPage.register('Another User', testUser.email, 'DifferentPass123!');
    await registerPage.expectRegisterError();
  });
});
