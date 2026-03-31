import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createWrapper, createTestQueryClient } from '@/__tests__/test-utils';
import { useHouses, useHouse, useCreateHouse, useUpdateHouse, useDeleteHouse } from '../houses';

function createWrapperWith(queryClient: QueryClient) {
  return function Wrapper({ children }: { children: React.ReactNode }) {
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

// Mock the API client
vi.mock('../../client', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

import apiClient from '../../client';

const mockedClient = vi.mocked(apiClient);

describe('useHouses', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches houses list successfully', async () => {
    const mockData = {
      houses: [
        { id: '1', name: 'Maison', score: 85, devicesCount: 3, pendingCount: 1, overdueCount: 0 },
      ],
      globalScore: 85,
    };
    mockedClient.get.mockResolvedValueOnce({ data: mockData });

    const { result } = renderHook(() => useHouses(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockData);
    expect(mockedClient.get).toHaveBeenCalledWith('/api/v1/houses');
  });

  it('handles fetch error', async () => {
    mockedClient.get.mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useHouses(), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error?.message).toBe('Network error');
  });
});

describe('useHouse', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches a single house by id', async () => {
    const mockHouse = {
      id: '1',
      name: 'Maison',
      score: 90,
      devicesCount: 2,
      pendingCount: 0,
      overdueCount: 0,
      devices: [],
    };
    mockedClient.get.mockResolvedValueOnce({ data: mockHouse });

    const { result } = renderHook(() => useHouse('1'), { wrapper: createWrapper() });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockHouse);
    expect(mockedClient.get).toHaveBeenCalledWith('/api/v1/houses/1');
  });

  it('does not fetch when houseId is empty', () => {
    renderHook(() => useHouse(''), { wrapper: createWrapper() });
    expect(mockedClient.get).not.toHaveBeenCalled();
  });
});

describe('useCreateHouse', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('creates a house and returns data', async () => {
    const newHouse = { id: '2', name: 'Appartement' };
    mockedClient.post.mockResolvedValueOnce({ data: newHouse });

    const { result } = renderHook(() => useCreateHouse(), { wrapper: createWrapper() });

    result.current.mutate({ name: 'Appartement' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(newHouse);
    expect(mockedClient.post).toHaveBeenCalledWith('/api/v1/houses', { name: 'Appartement' });
  });

  it('invalidates cache and calls onSuccess when both are provided', async () => {
    const newHouse = { id: '2', name: 'Appartement' };
    mockedClient.post.mockResolvedValueOnce({ data: newHouse });
    const onSuccess = vi.fn();

    const queryClient = createTestQueryClient();
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');
    const wrapper = createWrapperWith(queryClient);

    const { result } = renderHook(() => useCreateHouse({ onSuccess }), { wrapper });

    result.current.mutate({ name: 'Appartement' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['houses'], refetchType: 'active' });
    expect(onSuccess).toHaveBeenCalled();
  });
});

describe('useUpdateHouse', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('updates a house', async () => {
    const updated = { id: '1', name: 'Maison Bleue' };
    mockedClient.put.mockResolvedValueOnce({ data: updated });

    const { result } = renderHook(() => useUpdateHouse('1'), { wrapper: createWrapper() });

    result.current.mutate({ name: 'Maison Bleue' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(updated);
    expect(mockedClient.put).toHaveBeenCalledWith('/api/v1/houses/1', { name: 'Maison Bleue' });
  });

  it('invalidates cache and calls onSuccess when both are provided', async () => {
    const updated = { id: '1', name: 'Maison Bleue' };
    mockedClient.put.mockResolvedValueOnce({ data: updated });
    const onSuccess = vi.fn();

    const queryClient = createTestQueryClient();
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');
    const wrapper = createWrapperWith(queryClient);

    const { result } = renderHook(() => useUpdateHouse('1', { onSuccess }), { wrapper });

    result.current.mutate({ name: 'Maison Bleue' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['houses'], refetchType: 'active' });
    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['houses', '1'], refetchType: 'active' });
    expect(onSuccess).toHaveBeenCalled();
  });
});

describe('useDeleteHouse', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deletes a house', async () => {
    mockedClient.delete.mockResolvedValueOnce({});

    const { result } = renderHook(() => useDeleteHouse(), { wrapper: createWrapper() });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockedClient.delete).toHaveBeenCalledWith('/api/v1/houses/1');
  });

  it('invalidates cache and calls onSuccess when both are provided', async () => {
    mockedClient.delete.mockResolvedValueOnce({});
    const onSuccess = vi.fn();

    const queryClient = createTestQueryClient();
    const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries');
    const wrapper = createWrapperWith(queryClient);

    const { result } = renderHook(() => useDeleteHouse({ onSuccess }), { wrapper });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['houses'], refetchType: 'active' });
    expect(onSuccess).toHaveBeenCalled();
  });
});
