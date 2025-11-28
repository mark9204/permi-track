import apiClient from '../../../services/apiClient';
import type { User } from '../../../types/auth.types';
import type { PaginatedResponse } from '../../../types/api.types';

export interface UserQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  password?: string;
  firstName: string;
  lastName: string;
  roleIds: number[];
}

export interface UpdateUserRequest {
  username: string;
  email: string;
  password?: string;
  firstName: string;
  lastName: string;
  roleIds: number[];
}

const getUsers = (params: UserQueryParams): Promise<PaginatedResponse<User>> => {
  return apiClient.get('/users', { params }).then((res) => res.data);
};

const createUser = (data: CreateUserRequest): Promise<User> => {
  return apiClient.post('/users', data).then((res) => res.data);
};

const updateUser = (id: number, data: UpdateUserRequest): Promise<User> => {
  return apiClient.put(`/users/${id}`, data).then((res) => res.data);
};

const deleteUser = (id: number): Promise<void> => {
  return apiClient.delete(`/users/${id}`);
};

export const userService = {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
};
