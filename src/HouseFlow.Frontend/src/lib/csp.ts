/**
 * CSP (Content Security Policy) utilities for nonce-based script/style authorization.
 */

const API_URL = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || '';

export function buildCspHeader(nonce: string, isDev: boolean): string {
  const scriptSrc = isDev
    ? `'self' 'nonce-${nonce}' 'strict-dynamic' 'unsafe-eval'`
    : `'self' 'nonce-${nonce}' 'strict-dynamic'`;

  const connectSrc = isDev
    ? `'self' http://localhost:* ws://localhost:*`
    : `'self' ${API_URL} https://api.houseflow.rouss.be`;

  const directives = [
    `default-src 'self'`,
    `script-src ${scriptSrc}`,
    `style-src 'self' 'nonce-${nonce}'`,
    `img-src 'self' data: https:`,
    `font-src 'self'`,
    `connect-src ${connectSrc}`,
    `frame-ancestors 'none'`,
  ];

  return directives.join('; ');
}
