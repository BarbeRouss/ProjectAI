import { useQuery, useMutation, useQueryClient, UseQueryOptions, UseMutationOptions } from '@tanstack/react-query';
import apiClient from '../client';

// DTOs matching backend
interface CreateHouseRequestDto {
  name: string;
  address?: string | null;
  zipCode?: string | null;
  city?: string | null;
}

interface HouseDto {
  id: string;
  name: string;
  address?: string | null;
  zipCode?: string | null;
  city?: string | null;
  createdAt?: string;
  role?: 'Owner' | 'Collaborator' | 'Tenant';
}

interface HouseMemberDto {
  userId: string;
  email: string;
  name: string;
  role: 'Owner' | 'Collaborator' | 'Tenant';
}

interface HouseDetailDto extends HouseDto {
  members: HouseMemberDto[];
}

interface InviteMemberRequestDto {
  email: string;
  role: 'Owner' | 'Collaborator' | 'Tenant';
}

/**
 * Hook to fetch all houses for current user
 */
export function useHouses(options?: UseQueryOptions<HouseDto[], Error>) {
  return useQuery({
    queryKey: ['houses'],
    queryFn: async () => {
      const response = await apiClient.get<HouseDto[]>('/v1/houses');
      return response.data;
    },
    ...options,
  });
}

/**
 * Hook to fetch a single house with details
 */
export function useHouse(houseId: string, options?: UseQueryOptions<HouseDetailDto, Error>) {
  return useQuery({
    queryKey: ['houses', houseId],
    queryFn: async () => {
      const response = await apiClient.get<HouseDetailDto>(`/v1/houses/${houseId}`);
      return response.data;
    },
    enabled: !!houseId,
    ...options,
  });
}

/**
 * Hook to create a new house
 */
export function useCreateHouse(
  options?: UseMutationOptions<HouseDto, Error, CreateHouseRequestDto>
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateHouseRequestDto) => {
      const response = await apiClient.post<HouseDto>('/v1/houses', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['houses'] });
    },
    ...options,
  });
}

/**
 * Hook to invite a member to a house
 */
export function useInviteMember(
  houseId: string,
  options?: UseMutationOptions<void, Error, InviteMemberRequestDto>
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: InviteMemberRequestDto) => {
      await apiClient.post(`/v1/houses/${houseId}/members`, data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['houses', houseId] });
    },
    ...options,
  });
}
