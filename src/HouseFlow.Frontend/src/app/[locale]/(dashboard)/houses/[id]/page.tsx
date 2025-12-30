"use client";

import { use } from 'react';
import { useTranslations, useLocale } from 'next-intl';
import { useHouse, useDevices } from '@/lib/api/hooks';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import Link from 'next/link';

export default function HouseDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const locale = useLocale();
  const t = useTranslations('houses');
  const tDevices = useTranslations('devices');
  const tCommon = useTranslations('common');

  const { data: house, isLoading: houseLoading } = useHouse(id);
  const { data: devices, isLoading: devicesLoading } = useDevices(id);

  const getRoleLabel = (role: number) => {
    switch (role) {
      case 0:
        return t('roleOwner');
      case 1:
        return t('roleEditor');
      case 2:
        return t('roleViewer');
      default:
        return String(role);
    }
  };

  if (houseLoading) {
    return <div className="p-8">{tCommon('loading')}</div>;
  }

  if (!house) {
    return <div className="p-8">{t('notFound')}</div>;
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
      <div className="max-w-7xl mx-auto">
        {/* House Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            {house.name}
          </h1>
          {(house.address || house.city || house.zipCode) && (
            <p className="text-gray-600 dark:text-gray-400">
              {[house.address, house.city, house.zipCode].filter(Boolean).join(', ')}
            </p>
          )}
        </div>

        {/* Members Section */}
        <div className="mb-8">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>{t('members')}</CardTitle>
                  <CardDescription>
                    {house.members.length} {t('memberCount')}
                  </CardDescription>
                </div>
                <Button variant="outline">{t('inviteMember')}</Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {house.members.map((member) => (
                  <div key={member.userId} className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-800 rounded-md">
                    <div>
                      <p className="font-medium text-gray-900 dark:text-white">
                        {member.name}
                      </p>
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {member.email}
                      </p>
                    </div>
                    <span className="px-3 py-1 text-sm bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 rounded-full">
                      {getRoleLabel(member.role)}
                    </span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Devices Section */}
        <div>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-2xl font-semibold text-gray-900 dark:text-white">
              {tDevices('title')}
            </h2>
            <Link href={`/${locale}/houses/${id}/devices/new`}>
              <Button>{tDevices('addDevice')}</Button>
            </Link>
          </div>

          {devicesLoading ? (
            <p className="text-gray-600 dark:text-gray-400">{t('loadingDevices')}</p>
          ) : !devices || devices.length === 0 ? (
            <Card>
              <CardHeader>
                <CardTitle>{tDevices('noDevicesYet')}</CardTitle>
                <CardDescription>
                  {tDevices('noDevicesDescription')}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Link href={`/${locale}/houses/${id}/devices/new`}>
                  <Button>{tDevices('addDevice')}</Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {devices.map((device) => (
                <Card key={device.id} className="hover:shadow-lg transition-shadow">
                  <CardHeader>
                    <CardTitle className="text-lg">{device.name}</CardTitle>
                    <CardDescription>{device.type}</CardDescription>
                  </CardHeader>
                  <CardContent>
                    <Link href={`/${locale}/devices/${device.id}`}>
                      <Button variant="outline" className="w-full">
                        {tCommon('viewDetails')}
                      </Button>
                    </Link>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
