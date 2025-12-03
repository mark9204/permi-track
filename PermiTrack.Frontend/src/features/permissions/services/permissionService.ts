import apiClient from '../../../services/apiClient';

export interface Permission {
  id: number;
  name: string;
  description: string;
  category?: string;
  department?: string;
}

export interface CreatePermissionRequest {
  name: string;
  description: string;
  category?: string;
  department?: string;
}

const permissionService = {
  async getAllPermissions(): Promise<Permission[]> {
    const res = await apiClient.get('/permissions', { params: { limit: 100 } });

    if (res && Array.isArray(res.data)) {
      return res.data as Permission[];
    }

    if (res && res.data && Array.isArray(res.data.data)) {
      return res.data.data as Permission[];
    }

    return [];
  },

  async createPermission(data: CreatePermissionRequest): Promise<Permission> {
    const res = await apiClient.post('/permissions', data);
    return res.data;
  },

  async updatePermission(id: number, data: CreatePermissionRequest): Promise<Permission> {
    const res = await apiClient.put(`/permissions/${id}`, data);
    return res.data;
  },

  async deletePermission(id: number): Promise<void> {
    await apiClient.delete(`/permissions/${id}`);
  },
};

export default permissionService;
