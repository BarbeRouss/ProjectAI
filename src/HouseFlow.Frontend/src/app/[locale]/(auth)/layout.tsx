"use client";

import { useEffect, useRef } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useLocale } from 'next-intl';
import { useAuth } from '@/lib/auth/context';

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const locale = useLocale();
  const pathname = usePathname();

  // Track whether auth transitioned from false→true (user just logged in/registered).
  // This distinguishes "user authenticated via form" from "user was already authenticated".
  const wasAuthenticated = useRef<boolean | null>(null);
  const authJustChanged = useRef(false);

  useEffect(() => {
    if (!isLoading) {
      if (wasAuthenticated.current === false && isAuthenticated) {
        // Auth transitioned false→true: form just authenticated the user.
        // The form handles its own redirect — don't interfere.
        authJustChanged.current = true;
      }
      wasAuthenticated.current = isAuthenticated;
    }
  }, [isAuthenticated, isLoading]);

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      if (authJustChanged.current) {
        // Form just authenticated — let it handle its own redirect
        // (e.g. to /houses/{id}/devices/new instead of /dashboard)
        authJustChanged.current = false;
        return;
      }
      // User is authenticated but didn't just authenticate here:
      // either they navigated to an auth page while logged in,
      // or pressed the back button to return to /register or /login.
      router.replace(`/${locale}/dashboard`);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pathname, isLoading, isAuthenticated, router, locale]);

  if (isLoading || isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
