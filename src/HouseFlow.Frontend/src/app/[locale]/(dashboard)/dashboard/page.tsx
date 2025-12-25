"use client";

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations, useLocale } from 'next-intl';
import { useHouses } from '@/lib/api/hooks';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { useAuth } from '@/lib/auth/context';

export default function DashboardPage() {
  const router = useRouter();
  const locale = useLocale();
  const t = useTranslations('dashboard');
  const tHouses = useTranslations('houses');
  const tCommon = useTranslations('common');
  const { user } = useAuth();

  const { data: houses, isLoading } = useHouses();

  // Auto-redirect to single house if user has exactly one
  useEffect(() => {
    if (!isLoading && houses && houses.length === 1) {
      router.push(`/${locale}/houses/${houses[0].id}`);
    }
  }, [houses, isLoading, router, locale]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
        <div className="max-w-7xl mx-auto">
          <p className="text-gray-700 dark:text-gray-300">{tCommon('loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            {t('welcome')}, {user?.name}!
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            {t('title')}
          </p>
        </div>

        {/* Houses Section */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-semibold text-gray-900 dark:text-white">
              {t('myHouses')}
            </h2>
            <Link href={`/${locale}/houses/new`}>
              <Button>{tHouses('addHouse')}</Button>
            </Link>
          </div>

          {!houses || houses.length === 0 ? (
            <Card>
              <CardHeader>
                <CardTitle>{t('noHousesYet')}</CardTitle>
                <CardDescription>
                  {t('getStarted')}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Link href={`/${locale}/houses/new`}>
                  <Button>{tHouses('addHouse')}</Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {houses.map((house) => (
                <Link key={house.id} href={`/${locale}/houses/${house.id}`}>
                  <Card className="hover:shadow-lg transition-shadow cursor-pointer">
                    <CardHeader>
                      <CardTitle className="text-xl">{house.name}</CardTitle>
                      <CardDescription>
                        {house.address && house.city && house.zipCode
                          ? `${house.address}, ${house.city} ${house.zipCode}`
                          : house.name}
                      </CardDescription>
                    </CardHeader>
                    <CardContent>
                      <Button variant="outline" className="w-full">
                        {tCommon('viewDetails')}
                      </Button>
                    </CardContent>
                  </Card>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
