import { Page, Locator, expect } from '@playwright/test';

export class RegisterPage {
  readonly page: Page;
  readonly nameInput: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly registerButton: Locator;
  readonly loginLink: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.nameInput = page.getByPlaceholder(/jean dupont/i);
    this.emailInput = page.getByPlaceholder('you@example.com');
    this.passwordInput = page.getByPlaceholder('••••••••');
    this.registerButton = page.getByRole('button', { name: /sign up|s'inscrire/i });
    this.loginLink = page.getByRole('link', { name: /sign in|se connecter/i });
    this.errorMessage = page.locator('.bg-red-50, [class*="bg-red-900"]');
  }

  async goto() {
    await this.page.goto('/fr/register');
  }

  async register(name: string, email: string, password: string) {
    // Webkit requires special handling - type character by character to ensure React state updates
    await this.nameInput.click();
    await this.nameInput.pressSequentially(name, { delay: 50 });
    await expect(this.nameInput).toHaveValue(name);

    await this.emailInput.click();
    await this.emailInput.pressSequentially(email, { delay: 50 });
    await expect(this.emailInput).toHaveValue(email);

    await this.passwordInput.click();
    await this.passwordInput.pressSequentially(password, { delay: 50 });
    await expect(this.passwordInput).toHaveValue(password);

    await this.registerButton.click();
  }

  async expectRegisterSuccess() {
    // NEW FLOW: After registration, users are redirected to device creation for the auto-created house
    // Wait longer for webkit (it might be slower with cookie handling)
    await expect(this.page).toHaveURL(/\/fr\/houses\/[a-f0-9-]+\/devices\/new/, { timeout: 15000 });
  }

  async expectRegisterError() {
    await expect(this.errorMessage).toBeVisible();
  }
}
