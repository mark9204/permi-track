import apiClient from './apiClient';
import type { LoginRequest, LoginResponse, User } from '../types/auth.types';

const login = (data: LoginRequest): Promise<LoginResponse> => {
  return apiClient.post('/auth/login', data).then(res => {
    if (res.data.accessToken) {
      localStorage.setItem('accessToken', res.data.accessToken);
      localStorage.setItem('refreshToken', res.data.refreshToken);
      apiClient.defaults.headers.common['Authorization'] = `Bearer ${res.data.accessToken}`;
    }
    return res.data;
  });
};

const refreshToken = (): Promise<{ accessToken: string }> => {
  const refreshToken = localStorage.getItem('refreshToken');
  return apiClient.post('/auth/refresh', { refreshToken }).then(res => res.data);
};

const getCurrentUser = (): Promise<User> => {
  return apiClient.get('/account/profile').then(res => res.data);
};

const logout = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  delete apiClient.defaults.headers.common['Authorization'];
};

export const authService = {
  login,
  logout,
  refreshToken,
  getCurrentUser,
};
