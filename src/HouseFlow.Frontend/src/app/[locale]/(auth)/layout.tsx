"use client";

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useLocale } from 'next-intl';
import { useAuth } from '@/lib/auth/context';
import { consumeFormRedirecting } from '@/lib/auth/redirect-guard';

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const locale = useLocale();

  // Handle the case where the user is already authenticated when navigating
  // to an auth page (e.g. typing /login in the URL bar).
  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      if (consumeFormRedirecting()) {
        return;
      }
      router.replace(`/${locale}/dashboard`);
    }
  }, [isAuthenticated, isLoading, router, locale]);

  // Handle back-button navigation: popstate fires when the user presses
  // back/forward. Read auth state from storage to avoid stale closures.
  useEffect(() => {
    const handlePopState = () => {
      const hasToken = !!localStorage.getItem('houseflow_access_token');
      const hasUser = !!sessionStorage.getItem('houseflow_auth_user');
      if (hasToken && hasUser) {
        // Use window.location.replace for guaranteed history replacement
        // and navigation, bypassing any Next.js router caching issues.
        window.location.replace(`/${locale}/dashboard`);
      }
    };

    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, [locale]);

  if (isLoading || isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
