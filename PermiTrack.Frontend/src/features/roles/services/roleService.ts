import apiClient from '../../../services/apiClient';

export interface Role {
  id: string | number;
  name: string;
  description?: string | null;
}

const roleService = {
  async getAllRoles(): Promise<Role[]> {
    // Try to fetch up to 100 roles; adjust endpoint/query params as needed by backend
    const res = await apiClient.get('/roles', { params: { limit: 100 } });

    // If the backend returns a paginated response like { data: [...], meta: {...} }
    // try to extract the list; otherwise assume the array is returned directly.
    if (res && Array.isArray(res.data)) {
      return res.data as Role[];
    }

    if (res && res.data && Array.isArray(res.data.data)) {
      return res.data.data as Role[];
    }

    // Fallback: return empty array
    return [];
  },
};

export default roleService;
