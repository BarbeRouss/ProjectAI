import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

// Get API URL from environment (injected by Aspire)
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5203';

// In-memory token storage (more secure than localStorage)
let accessToken: string | null = null;
let isRefreshing = false;
let refreshSubscribers: ((token: string) => void)[] = [];

// Check for initial token from E2E tests (set via addInitScript)
if (typeof window !== 'undefined' && (window as any).__INITIAL_AUTH_TOKEN) {
  accessToken = (window as any).__INITIAL_AUTH_TOKEN;
  delete (window as any).__INITIAL_AUTH_TOKEN; // Clean up after reading
}

/**
 * Set the access token in memory
 */
export function setAccessToken(token: string | null) {
  accessToken = token;
}

/**
 * Get the current access token
 */
export function getAccessToken(): string | null {
  return accessToken;
}

// Expose setAccessToken globally for E2E tests
if (typeof window !== 'undefined') {
  (window as any).__setAccessToken = setAccessToken;
}

/**
 * Subscribe to token refresh events
 */
function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback);
}

/**
 * Notify all subscribers when token is refreshed
 */
function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach(callback => callback(token));
  refreshSubscribers = [];
}

/**
 * Refresh the access token using the refresh token (in HttpOnly cookie)
 */
async function refreshAccessToken(): Promise<string | null> {
  try {
    const response = await axios.post(
      `${API_URL}/v1/auth/refresh`,
      {},
      {
        withCredentials: true, // Important: send cookies
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );

    const newToken = response.data.token;
    setAccessToken(newToken);
    return newToken;
  } catch (error) {
    // Refresh failed - user needs to login again
    setAccessToken(null);
    if (typeof window !== 'undefined') {
      // Clear user data and redirect to login
      sessionStorage.removeItem('houseflow_auth_user');
      window.location.href = '/fr/login';
    }
    return null;
  }
}

/**
 * Axios client configured for HouseFlow API
 * Uses HttpOnly cookies for refresh tokens and in-memory storage for access tokens
 */
export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Important: send/receive cookies
});

/**
 * Request interceptor - Add JWT token to all requests
 */
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Add access token to Authorization header
    if (accessToken && config.headers) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Response interceptor - Handle token refresh and errors
 */
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Handle 401 Unauthorized - Try to refresh token
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      // Skip refresh for auth endpoints
      const isAuthEndpoint = originalRequest.url?.includes('/auth/login') ||
                             originalRequest.url?.includes('/auth/register') ||
                             originalRequest.url?.includes('/auth/refresh');

      if (isAuthEndpoint) {
        return Promise.reject(error);
      }

      // Mark this request as retried to avoid infinite loops
      originalRequest._retry = true;

      if (!isRefreshing) {
        isRefreshing = true;

        try {
          const newToken = await refreshAccessToken();

          if (newToken) {
            isRefreshing = false;
            onTokenRefreshed(newToken);

            // Retry the original request with new token
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${newToken}`;
            }
            return apiClient(originalRequest);
          }
        } catch (refreshError) {
          isRefreshing = false;
          return Promise.reject(refreshError);
        }
      } else {
        // Wait for the ongoing refresh to complete
        return new Promise((resolve) => {
          subscribeTokenRefresh((token: string) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            resolve(apiClient(originalRequest));
          });
        });
      }
    }

    // Handle 403 Forbidden
    if (error.response?.status === 403) {
      console.error('Access forbidden:', error.response.data);
    }

    // Handle 500 Server Error
    if (error.response?.status === 500) {
      console.error('Server error:', error.response.data);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
