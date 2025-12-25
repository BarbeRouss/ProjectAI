import { test, expect } from '../fixtures/auth';
import { DashboardPage } from '../pages/dashboard-page';
import { HousePage } from '../pages/house-page';

test.describe('User Flow 3: Maintenance Logging', () => {
  test('Quick log maintenance', async ({ authenticatedPage: page }) => {
    // Setup: Create house and device
    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();
    await dashboardPage.clickAddHouse();

    const housePage = new HousePage(page);
    await housePage.createHouse('Maintenance Test House', '789 Test Rd', '99999', 'Test City');

    // Add device
    await page.getByRole('link', { name: /add device|ajouter un appareil/i }).first().click();
    await page.getByPlaceholder(/chaudière/i).fill('Détecteur Fumée');
    await page.getByRole('combobox').selectOption('Détecteur de Fumée');
    await page.getByRole('button', { name: /save|enregistrer/i }).click();

    // Click on device to view details
    await page.getByText('Détecteur Fumée').click();
    await expect(page).toHaveURL(/.*devices\/[a-f0-9-]+$/);

    // Note: Maintenance types are auto-created by backend
    // If there are maintenance types, we can log maintenance
    const logButton = page.getByRole('button', { name: /log maintenance|enregistrer/i }).first();

    if (await logButton.isVisible()) {
      await logButton.click();

      // Quick log (default mode)
      const today = new Date().toISOString().split('T')[0];
      await page.locator('input[type="date"]').fill(today);
      await page.getByRole('button', { name: /save|enregistrer/i }).last().click();

      // Verify success - should close dialog and show in history
      await expect(page.getByText(/history|historique/i)).toBeVisible();
    }
  });

  test('Detailed log maintenance with cost and provider', async ({ authenticatedPage: page }) => {
    // Setup: Create house and device
    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();
    await dashboardPage.clickAddHouse();

    const housePage = new HousePage(page);
    await housePage.createHouse('Detailed Log House', '321 Test Blvd', '88888', 'Test City');

    // Add device
    await page.getByRole('link', { name: /add device|ajouter un appareil/i }).first().click();
    await page.getByPlaceholder(/chaudière/i).fill('Climatisation');
    await page.getByRole('combobox').selectOption('Climatisation');
    await page.getByRole('button', { name: /save|enregistrer/i }).click();

    // View device details
    await page.getByText('Climatisation').click();

    // Log maintenance with details
    const logButton = page.getByRole('button', { name: /log maintenance|enregistrer/i }).first();

    if (await logButton.isVisible()) {
      await logButton.click();

      // Switch to detailed mode
      await page.getByRole('button', { name: /detailed|détaillée/i }).click();

      // Fill in details
      const today = new Date().toISOString().split('T')[0];
      await page.locator('input[type="date"]').fill(today);
      await page.locator('input[type="number"]').fill('150.50');
      await page.getByPlaceholder(/company name|nom/i).fill('Clim Expert SARL');
      await page.getByPlaceholder(/additional notes|notes/i).fill('Remplacement filtre + vérification fluide frigorigène. RAS.');

      await page.getByRole('button', { name: /save|enregistrer/i }).last().click();

      // Verify maintenance is logged with details
      await expect(page.getByText(/clim expert/i)).toBeVisible();
      await expect(page.getByText(/150.50/i)).toBeVisible();
    }
  });
});
