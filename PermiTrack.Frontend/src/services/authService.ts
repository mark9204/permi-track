import apiClient from './apiClient';
import type { LoginRequest, LoginResponse, User, RegisterRequest } from '../types/auth.types';

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

const register = (data: RegisterRequest): Promise<void> => {
  return apiClient.post('/auth/register', data)
    .then(() => {
      return;
    })
    .catch((err: any) => {
      console.error('authService.register failed', {
        url: '/auth/register',
        payload: data,
        status: err?.response?.status,
        responseData: err?.response?.data,
        message: err?.message,
      });

      const serverMsg = err?.response?.data?.message || err?.response?.data || err?.message;
      throw new Error(`Registration failed: ${serverMsg} (status: ${err?.response?.status})`);
    });
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
  register,
};
