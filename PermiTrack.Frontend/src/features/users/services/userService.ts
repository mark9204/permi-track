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

// --- DEMO STATE ---
let mockUsers: User[] = [
  { 
    id: 1, 
    username: 'admin', 
    email: 'admin@company.com', 
    firstName: 'Admin', 
    lastName: 'User', 
    isActive: true, 
    roles: ['Admin'], 
    department: 'IT',
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-01-01T00:00:00Z',
    lastLogin: new Date(Date.now() - 1000 * 60 * 15).toISOString() // 15 mins ago
  },
  { 
    id: 2, 
    username: 'jdoe', 
    email: 'jdoe@company.com', 
    firstName: 'John', 
    lastName: 'Doe', 
    isActive: true, 
    roles: ['User'], 
    department: 'HR',
    createdAt: '2025-01-02T00:00:00Z',
    updatedAt: '2025-01-02T00:00:00Z',
    lastLogin: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2).toISOString() // 2 days ago
  },
  { 
    id: 3, 
    username: 'asmith', 
    email: 'asmith@company.com', 
    firstName: 'Alice', 
    lastName: 'Smith', 
    isActive: false, 
    roles: ['Manager'], 
    department: 'Finance',
    createdAt: '2025-01-03T00:00:00Z',
    updatedAt: '2025-01-03T00:00:00Z',
    lastLogin: new Date(Date.now() - 1000 * 60 * 60 * 24 * 30).toISOString() // 30 days ago
  },
];

const getUsers = async (params: UserQueryParams): Promise<PaginatedResponse<User>> => {
  await new Promise(resolve => setTimeout(resolve, 400));
  
  let filtered = [...mockUsers];
  if (params.search) {
    const lower = params.search.toLowerCase();
    filtered = filtered.filter(u => 
      u.username.toLowerCase().includes(lower) || 
      u.email.toLowerCase().includes(lower) ||
      u.firstName.toLowerCase().includes(lower) ||
      u.lastName.toLowerCase().includes(lower)
    );
  }

  const page = params.page || 1;
  const pageSize = params.pageSize || 10;

  return {
    data: filtered,
    pagination: {
      page,
      pageSize,
      totalCount: filtered.length,
      totalPages: Math.ceil(filtered.length / pageSize)
    }
  };
};

const createUser = async (data: CreateUserRequest): Promise<User> => {
  await new Promise(resolve => setTimeout(resolve, 600));
  const newUser: User = {
    id: Math.floor(Math.random() * 10000),
    username: data.username,
    email: data.email,
    firstName: data.firstName,
    lastName: data.lastName,
    isActive: true,
    roles: ['User'], // Default role for demo
    department: 'IT', // Default department for demo
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  };
  mockUsers.push(newUser);
  return newUser;
};

const updateUser = async (id: number, data: UpdateUserRequest): Promise<User> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  const index = mockUsers.findIndex(u => u.id === id);
  if (index !== -1) {
    mockUsers[index] = { ...mockUsers[index], ...data };
    return mockUsers[index];
  }
  throw new Error('User not found');
};

const deleteUser = async (id: number): Promise<void> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  mockUsers = mockUsers.filter(u => u.id !== id);
};

export const userService = {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
};
