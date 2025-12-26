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
    this.errorMessage = page.locator('.bg-red-50, .bg-red-900\\/20');
  }

  async goto() {
    await this.page.goto('/fr/register');
  }

  async register(name: string, email: string, password: string) {
    await this.nameInput.fill(name);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.registerButton.click();
  }

  async expectRegisterSuccess() {
    // NEW FLOW: After registration, users are redirected to device creation for the auto-created house
    await expect(this.page).toHaveURL(/\/fr\/houses\/[a-f0-9-]+\/devices\/new/);
  }

  async expectRegisterError() {
    await expect(this.errorMessage).toBeVisible();
  }
}
