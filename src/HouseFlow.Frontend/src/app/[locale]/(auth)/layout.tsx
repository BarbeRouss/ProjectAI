"use client";

import { useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useLocale } from 'next-intl';
import { useAuth } from '@/lib/auth/context';

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();
  const locale = useLocale();
  // Track whether the user was already authenticated when this layout mounted.
  // This prevents the layout from hijacking redirects set by login/register forms
  // when they authenticate the user and navigate to a specific destination.
  const wasAuthenticatedOnMount = useRef<boolean | null>(null);

  useEffect(() => {
    if (!isLoading && wasAuthenticatedOnMount.current === null) {
      wasAuthenticatedOnMount.current = isAuthenticated;
    }
  }, [isLoading, isAuthenticated]);

  useEffect(() => {
    // Only redirect if the user was already authenticated when they navigated
    // to this auth page (e.g. manually visiting /login while logged in).
    // Do NOT redirect if they just authenticated via the form — the form
    // handles its own redirect (e.g. to /houses/{id}/devices/new).
    if (!isLoading && isAuthenticated && wasAuthenticatedOnMount.current) {
      router.replace(`/${locale}/dashboard`);
    }
  }, [isAuthenticated, isLoading, router, locale]);

  if (isLoading || (isAuthenticated && wasAuthenticatedOnMount.current)) {
    return null;
  }

  return <>{children}</>;
}
