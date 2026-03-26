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

  // Run intl middleware
  const response = intlMiddleware(request);

  // Set CSP header on response
  response.headers.set('Content-Security-Policy', cspHeader);

  // Propagate nonce to server components via Next.js request header forwarding.
  // Next.js reads x-middleware-request-* response headers and exposes them
  // as request headers to server components via headers().
  response.headers.set('x-middleware-request-x-nonce', nonce);

  return response;
}

export const config = {
  // Match only internationalized pathnames
  matcher: ['/', '/(fr|en)/:path*'],
};
