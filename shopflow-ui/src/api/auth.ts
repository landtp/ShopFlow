// src/api/auth.ts
import { apiClient } from './client'
import type  { AuthResponse } from '../types'

export const authApi = {
  register: (data: {
    email: string
    password: string
    firstName: string
    lastName: string
  }) => apiClient.post<AuthResponse>('/api/v1/auth/register', data),

  login: (data: { email: string; password: string }) =>
    apiClient.post<AuthResponse>('/api/v1/auth/login', data),

  logout: () =>
    apiClient.post('/api/v1/auth/logout'),

  refresh: (data: { userId: string; refreshToken: string }) =>
    apiClient.post<AuthResponse>('/api/v1/auth/refresh', data)
}