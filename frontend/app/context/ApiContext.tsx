import React, { createContext, useContext, ReactNode, useMemo } from 'react';

export interface IApiClient {
  get<T>(endpoint: string, options?: RequestInit): Promise<T>;
  post<T>(endpoint: string, body: any, options?: RequestInit): Promise<T>;
  put<T>(endpoint: string, body: any, options?: RequestInit): Promise<T>;
  delete<T>(endpoint: string, options?: RequestInit): Promise<T>;
}

// 2. Base API Client class implementing the REST helpers
class ApiClient implements IApiClient {
  private baseUrl: string;
  private apiKey: string;

  constructor(baseUrl: string, apiKey: string) {
    this.baseUrl = baseUrl;
    this.apiKey = apiKey;
  }

  // Common request handler to avoid repeating try/catch and header logic
  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;
    
    // Default headers (e.g., Content-Type, Auth tokens)
    const defaultHeaders = {
      'Content-Type': 'application/json',
      'X-Api-Key': this.apiKey
      // 'Authorization': `Bearer ${your_token_here}`
    };

    const config = {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, config);
      
      if (!response.ok) {
        // Handle basic HTTP errors
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP Error: ${response.status}`);
      }

      // Handle 204 No Content responses gracefully
      if (response.status === 204) return {} as T;

      return await response.json() as T;
    } catch (error) {
      console.error(`API Error on ${options.method || 'GET'} ${endpoint}:`, error);
      throw error;
    }
  }

  // GET
  async get<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  // POST
  async post<T>(endpoint: string, body: any, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: JSON.stringify(body),
    });
  }

  // PUT
  async put<T>(endpoint: string, body: any, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: JSON.stringify(body),
    });
  }

  // DELETE
  async delete<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }
}

// 3. Setup React Context
const ApiContext = createContext<ApiClient | null>(null);

interface ApiProviderProps {
  children: ReactNode;
  baseUrl: string;
  apiKey: string
}

export const ApiProvider = ({ children, baseUrl, apiKey }: ApiProviderProps) => {
  // Instantiate the client once
  const apiClient = useMemo(() => new ApiClient(baseUrl, apiKey), [baseUrl])

  return (
    <ApiContext.Provider value={apiClient}>
      {children}
    </ApiContext.Provider>
  );
};

// 4. Custom hook for easy consumption in components
export const useApi = () => {
  const context = useContext(ApiContext);
  if (!context) {
    throw new Error('useApi must be used within an ApiProvider');
  }
  return context;
};