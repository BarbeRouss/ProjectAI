import { Page, Locator, expect } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly registerLink: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.getByPlaceholder('you@example.com');
    this.passwordInput = page.getByPlaceholder('••••••••');
    this.loginButton = page.getByRole('button', { name: /sign in|se connecter/i });
    this.registerLink = page.getByRole('link', { name: /sign up|s'inscrire/i });
    this.errorMessage = page.locator('.bg-red-50, .bg-red-900\\/20');
  }

  async goto() {
    await this.page.goto('/fr/login');
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }

  async expectLoginSuccess() {
    // NEW FLOW: Users with one house are auto-redirected to house details, not dashboard
    await expect(this.page).toHaveURL(/\/fr\/(dashboard|houses\/[a-f0-9-]+)$/);
  }

  async expectLoginError() {
    await expect(this.errorMessage).toBeVisible();
  }
}
