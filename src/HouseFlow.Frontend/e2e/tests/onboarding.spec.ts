import { test, expect } from '../fixtures/auth';
import { RegisterPage } from '../pages/register-page';
import { LoginPage } from '../pages/login-page';
import { DashboardPage } from '../pages/dashboard-page';
import { HousePage } from '../pages/house-page';

test.describe('User Flow 1: Onboarding (First Time Experience)', () => {
  test('Complete onboarding: Register → Auto-created House → Add Device', async ({ page }) => {
    // ÉTAPE 1: Registration
    const registerPage = new RegisterPage(page);
    await registerPage.goto();

    const email = `newuser-${Date.now()}@houseflow.test`;
    const password = 'SecurePass123!';
    const name = 'Jean Dupont';

    await registerPage.register(name, email, password);
    await registerPage.expectRegisterSuccess();

    // ÉTAPE 2: After registration, user is redirected to device creation
    // for the auto-created house "Ma Maison"
    await expect(page).toHaveURL(/\/fr\/houses\/[^/]+\/devices\/new/);

    // ÉTAPE 3: Verify we're on the add device page
    await expect(page.getByRole('heading', { name: /ajouter un appareil/i })).toBeVisible();

    // ÉTAPE 4: Add first device
    await page.getByLabel(/nom de l'appareil/i).fill('Chaudière Principale');
    await page.getByLabel(/type d'appareil/i).selectOption('Chaudière Gaz');
    await page.getByRole('button', { name: /ajouter/i }).click();

    // ÉTAPE 5: Verify redirect to house page
    await expect(page).toHaveURL(/\/fr\/houses\/[^/]+$/);

    // ÉTAPE 6: Verify the auto-created house "Ma Maison" exists
    await expect(page.getByRole('heading', { name: /ma maison/i })).toBeVisible();
    await expect(page.getByText('Chaudière Principale')).toBeVisible();
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
