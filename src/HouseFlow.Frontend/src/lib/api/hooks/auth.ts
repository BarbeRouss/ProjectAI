import { useMutation, UseMutationOptions } from '@tanstack/react-query';
import apiClient from '../client';
import { useAuth } from '@/lib/auth/context';

// DTOs matching backend
interface RegisterRequestDto {
  email: string;
  password: string;
  name: string;
}

interface LoginRequestDto {
  email: string;
  password: string;
}

interface AuthResponseDto {
  token: string;
  user: {
    id: string;
    email: string;
    name: string;
  };
  firstHouseId?: string;
}

/**
 * Hook for user registration
 */
export function useRegister(
  options?: UseMutationOptions<AuthResponseDto, Error, RegisterRequestDto>
) {
  const { login } = useAuth();

  return useMutation({
    mutationFn: async (data: RegisterRequestDto) => {
      const response = await apiClient.post<AuthResponseDto>('/v1/auth/register', data);
      return response.data;
    },
    ...options,
    onSuccess: (data, variables, context) => {
      // Always call login first
      login(data.token, data.user);
      // Then call the user's onSuccess if provided
      options?.onSuccess?.(data, variables, context);
    },
  });
}

/**
 * Hook for user login
 */
export function useLogin(
  options?: UseMutationOptions<AuthResponseDto, Error, LoginRequestDto>
) {
  const { login } = useAuth();

  return useMutation({
    mutationFn: async (data: LoginRequestDto) => {
      const response = await apiClient.post<AuthResponseDto>('/v1/auth/login', data);
      return response.data;
    },
    ...options,
    onSuccess: (data, variables, context) => {
      // Always call login first
      login(data.token, data.user);
      // Then call the user's onSuccess if provided
      options?.onSuccess?.(data, variables, context);
    },
  });
}
