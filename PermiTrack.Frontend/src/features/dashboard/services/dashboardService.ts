import apiClient from '../../../services/apiClient';

export interface TopRequestedRole {
  roleId: number;
  roleName: string;
  count: number;
}

export interface DashboardStats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  cancelled: number;
  averageProcessingTimeHours: number;
  topRequestedRoles: TopRequestedRole[];
}

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    const res = await apiClient.get('/access-requests/statistics');
    return (res && (res.data || res)) as DashboardStats;
  },
};

export default dashboardService;
