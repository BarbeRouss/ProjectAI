"use client";

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { setAccessToken, getAccessToken } from '../api/client';
import apiClient from '../api/client';

interface User {
  id: string;
  email: string;
  name: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (token: string, user: User) => void;
  logout: () => Promise<void>;
  getToken: () => string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const AUTH_USER_KEY = 'houseflow_auth_user';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Load auth state from sessionStorage on mount
    // Note: We don't store tokens anymore - they're in cookies/memory
    try {
      const userJson = sessionStorage.getItem(AUTH_USER_KEY);

      if (userJson) {
        const savedUser = JSON.parse(userJson);
        setUser(savedUser);

        // Only try to refresh if we don't already have an access token
        // (for E2E tests, the token is set directly)
        if (!getAccessToken()) {
          // Try to refresh token to verify session is still valid
          // This will set the access token in memory if refresh succeeds
          apiClient.post('/v1/auth/refresh')
            .then((response) => {
              if (response.data.token) {
                setAccessToken(response.data.token);
              }
            })
            .catch(() => {
              // Refresh failed - clear user
              sessionStorage.removeItem(AUTH_USER_KEY);
              setUser(null);
            });
        }
      }
    } catch (error) {
      console.error('Failed to load auth state:', error);
      // Clear invalid data
      sessionStorage.removeItem(AUTH_USER_KEY);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const login = (token: string, userData: User) => {
    // Store access token in memory
    setAccessToken(token);

    // Store user data in sessionStorage (refresh tokens are in HttpOnly cookies)
    sessionStorage.setItem(AUTH_USER_KEY, JSON.stringify(userData));
    setUser(userData);
  };

  const logout = async () => {
    try {
      // Call logout endpoint to revoke refresh token
      await apiClient.post('/v1/auth/logout');
    } catch (error) {
      console.error('Logout API call failed:', error);
      // Continue with client-side cleanup even if API call fails
    } finally {
      // Clear access token from memory
      setAccessToken(null);

      // Clear user data from sessionStorage
      sessionStorage.removeItem(AUTH_USER_KEY);
      setUser(null);
    }
  };

  const getToken = (): string | null => {
    if (typeof window === 'undefined') return null;
    return getAccessToken();
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        getToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
