import { test, expect } from '../fixtures/auth';
import { DashboardPage } from '../pages/dashboard-page';
import { HousePage } from '../pages/house-page';

test.describe('User Flow 2: Device Management', () => {
  test('Add device with auto maintenance type configuration', async ({ authenticatedPage: page }) => {
    // ÉTAPE 1: Create a house first
    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();
    await dashboardPage.clickAddHouse();

    const housePage = new HousePage(page);
    await housePage.createHouse('Test House', '123 Test St', '12345', 'Test City');
    await housePage.expectCreateSuccess();

    // ÉTAPE 2: Add a device
    await page.getByRole('link', { name: /add device|ajouter un appareil/i }).first().click();
    await expect(page).toHaveURL(/.*devices\/new/);

    await page.getByPlaceholder(/chaudière/i).fill('Chaudière Sous-sol');
    await page.getByRole('combobox').selectOption('Chaudière Gaz');
    await page.getByRole('button', { name: /save|enregistrer/i }).click();

    // ÉTAPE 3: Verify device was created
    await expect(page).toHaveURL(/.*houses\/[a-f0-9-]+$/);
    await expect(page.getByText('Chaudière Sous-sol')).toBeVisible();
  });

  test('View all devices for a house', async ({ authenticatedPage: page }) => {
    // Create house
    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();
    await dashboardPage.clickAddHouse();

    const housePage = new HousePage(page);
    await housePage.createHouse('Multi-Device House', '456 Test Ave', '54321', 'Test City 2');

    // Add multiple devices
    const devices = [
      { name: 'Chaudière', type: 'Chaudière Gaz' },
      { name: 'Toiture', type: 'Toiture' },
      { name: 'Alarme', type: 'Alarme' },
    ];

    for (const device of devices) {
      await page.getByRole('link', { name: /add device|ajouter un appareil/i }).first().click();
      await page.getByPlaceholder(/chaudière/i).fill(device.name);
      await page.getByRole('combobox').selectOption(device.type);
      await page.getByRole('button', { name: /save|enregistrer/i }).click();
      await page.waitForURL(/.*houses\/[a-f0-9-]+$/);
    }

    // Verify all devices are visible
    for (const device of devices) {
      await expect(page.getByText(device.name)).toBeVisible();
    }
  });
});
