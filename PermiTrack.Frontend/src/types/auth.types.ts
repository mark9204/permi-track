export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface Role {
  id?: number;
  name: string;
  level?: number;
  Level?: number;
}

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  department?: string;
  isActive: boolean;
  roles?: (string | Role)[];
  createdAt: string;
  updatedAt: string;
  lastLogin?: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
  department?: string;
  password?: string;
  currentPassword?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  roles: (string | Role)[];
  permissions: string[];
}

export interface RefreshRequest {
  refreshToken: string;
}
