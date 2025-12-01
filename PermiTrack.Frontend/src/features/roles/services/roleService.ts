import apiClient from '../../../services/apiClient';

export interface Role {
  id: number;
  name: string;
  description: string;
  userCount?: number;
}

export interface CreateRoleRequest {
  name: string;
  description: string;
}

const roleService = {
  async getAllRoles(): Promise<Role[]> {
    
    const res = await apiClient.get('/roles', { params: { limit: 100 } });

    
    if (res && Array.isArray(res.data)) {
      return res.data as Role[];
    }

    if (res && res.data && Array.isArray(res.data.data)) {
      return res.data.data as Role[];
    }

    // Fallback: return empty array
    return [];
  },

  async createRole(data: CreateRoleRequest): Promise<Role> {
    const res = await apiClient.post('/roles', data);
    return res.data;
  },

  async updateRole(id: number, data: CreateRoleRequest): Promise<Role> {
    const res = await apiClient.put(`/roles/${id}`, data);
    return res.data;
  },

  async deleteRole(id: number): Promise<void> {
    await apiClient.delete(`/roles/${id}`);
  },
};

export default roleService;
