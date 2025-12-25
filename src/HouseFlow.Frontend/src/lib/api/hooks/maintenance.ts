import { useQuery, useMutation, useQueryClient, UseQueryOptions, UseMutationOptions } from '@tanstack/react-query';
import apiClient from '../client';

// Enums matching backend
type Periodicity = 'Daily' | 'Weekly' | 'Monthly' | 'Quarterly' | 'Biannual' | 'Annual' | 'Custom';
type MaintenanceStatus = 'Pending' | 'Completed' | 'Overdue' | 'Skipped';

// DTOs matching backend
interface CreateMaintenanceTypeRequestDto {
  name: string;
  periodicity: Periodicity;
  customDays?: number | null;
  reminderEnabled: boolean;
  reminderDaysBefore: number;
}

interface MaintenanceTypeDto {
  id: string;
  deviceId: string;
  name: string;
  periodicity: Periodicity;
  customDays?: number | null;
  reminderEnabled: boolean;
  reminderDaysBefore: number;
}

interface LogMaintenanceRequestDto {
  date: string;
  status: MaintenanceStatus;
  cost?: number | null;
  provider?: string | null;
  notes?: string | null;
}

interface MaintenanceInstanceDto {
  id: string;
  maintenanceTypeId: string;
  date: string;
  status: MaintenanceStatus;
  cost?: number | null;
  provider?: string | null;
  notes?: string | null;
}

/**
 * Hook to fetch all maintenance types for a device
 */
export function useMaintenanceTypes(
  deviceId: string,
  options?: UseQueryOptions<MaintenanceTypeDto[], Error>
) {
  return useQuery({
    queryKey: ['devices', deviceId, 'maintenance-types'],
    queryFn: async () => {
      const response = await apiClient.get<MaintenanceTypeDto[]>(`/v1/devices/${deviceId}/maintenance-types`);
      return response.data;
    },
    enabled: !!deviceId,
    ...options,
  });
}

/**
 * Hook to create a maintenance type
 */
export function useCreateMaintenanceType(
  deviceId: string,
  options?: UseMutationOptions<MaintenanceTypeDto, Error, CreateMaintenanceTypeRequestDto>
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateMaintenanceTypeRequestDto) => {
      const response = await apiClient.post<MaintenanceTypeDto>(
        `/v1/devices/${deviceId}/maintenance-types`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices', deviceId, 'maintenance-types'] });
    },
    ...options,
  });
}

/**
 * Hook to fetch maintenance instances for a device
 */
export function useMaintenanceInstances(
  deviceId: string,
  options?: UseQueryOptions<MaintenanceInstanceDto[], Error>
) {
  return useQuery({
    queryKey: ['devices', deviceId, 'maintenance-instances'],
    queryFn: async () => {
      const response = await apiClient.get<MaintenanceInstanceDto[]>(
        `/v1/devices/${deviceId}/maintenance-instances`
      );
      return response.data;
    },
    enabled: !!deviceId,
    ...options,
  });
}

/**
 * Hook to log a maintenance instance
 */
export function useLogMaintenance(
  maintenanceTypeId: string,
  options?: UseMutationOptions<MaintenanceInstanceDto, Error, LogMaintenanceRequestDto>
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: LogMaintenanceRequestDto) => {
      const response = await apiClient.post<MaintenanceInstanceDto>(
        `/v1/maintenance-types/${maintenanceTypeId}/instances`,
        data
      );
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] });
      queryClient.invalidateQueries({ queryKey: ['maintenance-instances'] });
    },
    ...options,
  });
}
