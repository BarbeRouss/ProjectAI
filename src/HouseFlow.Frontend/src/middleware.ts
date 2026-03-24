import { NextRequest } from 'next/server';
import createIntlMiddleware from 'next-intl/middleware';
import { locales, defaultLocale } from './lib/i18n/config';
import { buildCspHeader } from './lib/csp';

const intlMiddleware = createIntlMiddleware({
  // A list of all locales that are supported
  locales,

  // Used when no locale matches
  defaultLocale,

  // Always use locale prefix (e.g., /fr/dashboard instead of /dashboard)
  localePrefix: 'always',
});

export default function middleware(request: NextRequest) {
  // Generate a cryptographic nonce for this request
  const nonce = Buffer.from(crypto.randomUUID()).toString('base64');

  // Build CSP header with the nonce
  const isDev = process.env.NODE_ENV === 'development';
  const cspHeader = buildCspHeader(nonce, isDev);

  // Add nonce to request headers so server components can read it via headers()
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-nonce', nonce);

  // Run intl middleware with the modified request
  const response = intlMiddleware(request);

  // Set CSP and nonce headers on the response
  response.headers.set('Content-Security-Policy', cspHeader);
  response.headers.set('x-nonce', nonce);

  return response;
}

export const config = {
  // Match only internationalized pathnames
  matcher: ['/', '/(fr|en)/:path*'],
};
