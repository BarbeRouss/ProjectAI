"use client";

import { use, useState } from 'react';
import { useTranslations } from 'next-intl';
import { useDevice, useMaintenanceTypes, useMaintenanceInstances } from '@/lib/api/hooks';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { format } from 'date-fns';
import { LogMaintenanceDialog } from '@/components/maintenance/log-maintenance-dialog';

export default function DeviceDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const t = useTranslations('devices');
  const tMaintenance = useTranslations('maintenance');

  const { data: device, isLoading: deviceLoading } = useDevice(id);
  const { data: maintenanceTypes, isLoading: typesLoading } = useMaintenanceTypes(id);
  const { data: maintenanceInstances, isLoading: instancesLoading } = useMaintenanceInstances(id);

  const [showLogDialog, setShowLogDialog] = useState(false);
  const [selectedMaintenanceType, setSelectedMaintenanceType] = useState<string | null>(null);

  if (deviceLoading) {
    return <div className="p-8">Loading...</div>;
  }

  if (!device) {
    return <div className="p-8">Device not found</div>;
  }

  const handleLogMaintenance = (maintenanceTypeId: string) => {
    setSelectedMaintenanceType(maintenanceTypeId);
    setShowLogDialog(true);
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Device Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            {device.name}
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            {device.type}
            {device.installDate && (
              <> • Installed {format(new Date(device.installDate), 'MMM d, yyyy')}</>
            )}
          </p>
        </div>

        {/* Maintenance Types Section */}
        <div className="mb-8">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>{tMaintenance('title')} Types</CardTitle>
                  <CardDescription>
                    {maintenanceTypes?.length || 0} maintenance type(s)
                  </CardDescription>
                </div>
                <Button variant="outline">Add Maintenance Type</Button>
              </div>
            </CardHeader>
            <CardContent>
              {typesLoading ? (
                <p>Loading...</p>
              ) : !maintenanceTypes || maintenanceTypes.length === 0 ? (
                <p className="text-gray-600 dark:text-gray-400">
                  No maintenance types configured yet
                </p>
              ) : (
                <div className="space-y-3">
                  {maintenanceTypes.map((type) => (
                    <div key={type.id} className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-800 rounded-md">
                      <div>
                        <p className="font-medium text-gray-900 dark:text-white">
                          {type.name}
                        </p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                          {type.periodicity}
                          {type.reminderEnabled && ` • Reminder ${type.reminderDaysBefore} days before`}
                        </p>
                      </div>
                      <Button
                        size="sm"
                        onClick={() => handleLogMaintenance(type.id)}
                      >
                        {tMaintenance('logMaintenance')}
                      </Button>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Maintenance History Section */}
        <div>
          <Card>
            <CardHeader>
              <CardTitle>{tMaintenance('history')}</CardTitle>
              <CardDescription>
                {maintenanceInstances?.length || 0} maintenance record(s)
              </CardDescription>
            </CardHeader>
            <CardContent>
              {instancesLoading ? (
                <p>Loading...</p>
              ) : !maintenanceInstances || maintenanceInstances.length === 0 ? (
                <p className="text-gray-600 dark:text-gray-400">
                  No maintenance history yet
                </p>
              ) : (
                <div className="space-y-3">
                  {maintenanceInstances
                    .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
                    .map((instance) => (
                      <div key={instance.id} className="p-4 bg-gray-50 dark:bg-gray-800 rounded-md">
                        <div className="flex items-start justify-between mb-2">
                          <p className="font-medium text-gray-900 dark:text-white">
                            {format(new Date(instance.date), 'MMMM d, yyyy')}
                          </p>
                          <span className={`px-2 py-1 text-xs rounded-full ${
                            instance.status === 'Completed'
                              ? 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200'
                              : 'bg-yellow-100 dark:bg-yellow-900 text-yellow-800 dark:text-yellow-200'
                          }`}>
                            {instance.status}
                          </span>
                        </div>
                        {instance.provider && (
                          <p className="text-sm text-gray-600 dark:text-gray-400">
                            Provider: {instance.provider}
                          </p>
                        )}
                        {instance.cost && (
                          <p className="text-sm text-gray-600 dark:text-gray-400">
                            Cost: €{instance.cost}
                          </p>
                        )}
                        {instance.notes && (
                          <p className="text-sm text-gray-600 dark:text-gray-400 mt-2">
                            {instance.notes}
                          </p>
                        )}
                      </div>
                    ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Log Maintenance Dialog */}
      {selectedMaintenanceType && (
        <LogMaintenanceDialog
          maintenanceTypeId={selectedMaintenanceType}
          open={showLogDialog}
          onClose={() => {
            setShowLogDialog(false);
            setSelectedMaintenanceType(null);
          }}
        />
      )}
    </div>
  );
}
