import apiClient from './apiClient';
import type { LoginRequest, LoginResponse, User, RegisterRequest, UpdateProfileRequest } from '../types/auth.types';

// --- DEMO STATE ---
const mockUser: User = {
  id: 1,
  username: 'demo_user',
  email: 'demo@company.com',
  firstName: 'Demo',
  lastName: 'User',
  isActive: true,
  roles: ['User'],
  department: 'IT',
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-01T00:00:00Z'
};

const login = async (data: LoginRequest): Promise<LoginResponse> => {
  await new Promise(resolve => setTimeout(resolve, 800));
  
  const isAdmin = data.username.toLowerCase().includes('admin');
  const isManager = data.username.toLowerCase().includes('manager');
  
  let roles = ['User'];
  if (isAdmin) roles = ['SuperAdmin'];
  else if (isManager) roles = ['Manager'];

  // In a real app, this would return a temporary token or a "mfa_required" status
  // For this demo, we'll just return the final response but the UI will intercept it
  const response: LoginResponse = {
    accessToken: 'mock-access-token',
    refreshToken: 'mock-refresh-token',
    expiresAt: new Date(Date.now() + 3600 * 1000).toISOString(),
    user: { ...mockUser, username: data.username, roles },
    roles,
    permissions: []
  };

  // We don't set localStorage here anymore, the UI will do it after MFA
  
  return response;
};

const verify2FA = async (code: string): Promise<boolean> => {
  await new Promise(resolve => setTimeout(resolve, 600));
  return code === '123456'; // Mock validation
};

const refreshToken = async (): Promise<{ accessToken: string }> => {
  await new Promise(resolve => setTimeout(resolve, 200));
  return { accessToken: 'mock-refreshed-token' };
};

const register = async (data: RegisterRequest): Promise<void> => {
  await new Promise(resolve => setTimeout(resolve, 1000));
  console.log('Registered user:', data);
  return;
};

const getCurrentUser = async (): Promise<User> => {
  await new Promise(resolve => setTimeout(resolve, 300));
  return { ...mockUser };
};

const updateProfile = async (data: UpdateProfileRequest): Promise<User> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  return { ...mockUser, ...data };
};

const logout = () => {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  delete apiClient.defaults.headers.common['Authorization'];
};

export const authService = {
  login,
  verify2FA,
  logout,
  refreshToken,
  getCurrentUser,
  updateProfile,
  register,
};
