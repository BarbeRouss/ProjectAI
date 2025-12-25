import { useQuery, useMutation, useQueryClient, UseQueryOptions, UseMutationOptions } from '@tanstack/react-query';
import apiClient from '../client';

// DTOs matching backend
interface CreateDeviceRequestDto {
  name: string;
  type: string;
  metadata?: string | null;
  installDate?: string | null;
}

interface DeviceDto {
  id: string;
  houseId: string;
  name: string;
  type: string;
  metadata?: string | null;
  installDate?: string | null;
  createdAt: string;
}

/**
 * Hook to fetch all devices for a house
 */
export function useDevices(
  houseId: string,
  options?: UseQueryOptions<DeviceDto[], Error>
) {
  return useQuery({
    queryKey: ['houses', houseId, 'devices'],
    queryFn: async () => {
      const response = await apiClient.get<DeviceDto[]>(`/v1/houses/${houseId}/devices`);
      return response.data;
    },
    enabled: !!houseId,
    ...options,
  });
}

/**
 * Hook to fetch a single device
 */
export function useDevice(
  deviceId: string,
  options?: UseQueryOptions<DeviceDto, Error>
) {
  return useQuery({
    queryKey: ['devices', deviceId],
    queryFn: async () => {
      const response = await apiClient.get<DeviceDto>(`/v1/devices/${deviceId}`);
      return response.data;
    },
    enabled: !!deviceId,
    ...options,
  });
}

/**
 * Hook to create a new device
 */
export function useCreateDevice(
  houseId: string,
  options?: UseMutationOptions<DeviceDto, Error, CreateDeviceRequestDto>
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateDeviceRequestDto) => {
      const response = await apiClient.post<DeviceDto>(`/v1/houses/${houseId}/devices`, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['houses', houseId, 'devices'] });
    },
    ...options,
  });
}
