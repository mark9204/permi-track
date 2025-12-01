import accessRequestService from '../../workflow/services/accessRequestService';
import type { AccessRequest } from '../../workflow/types';

export interface TopRequestedRole {
  roleName: string;
  count: number;
}

export interface DashboardStats {
  total: number;
  pending: number;
  approved: number;
  rejected: number;
  cancelled: number;
  topRequestedRoles: TopRequestedRole[];
  viewMode: 'Global' | 'Personal';
}

export const dashboardService = {
  async getStats(isAdmin: boolean): Promise<DashboardStats> {
    let requests: AccessRequest[] = [];
    let viewMode: 'Global' | 'Personal' = 'Personal';

    if (isAdmin) {
      try {
        requests = await accessRequestService.getAllRequests();
        viewMode = 'Global';
      } catch (error) {
        console.warn('Failed to fetch all requests for admin dashboard, falling back to personal requests.', error);
        requests = await accessRequestService.getMyRequests();
        viewMode = 'Personal';
      }
    } else {
      requests = await accessRequestService.getMyRequests();
      viewMode = 'Personal';
    }

    // Calculate Stats
    const total = requests.length;
    const pending = requests.filter(r => r.status === 'Pending').length;
    const approved = requests.filter(r => r.status === 'Approved').length;
    const rejected = requests.filter(r => r.status === 'Rejected').length;
    const cancelled = requests.filter(r => r.status === 'Cancelled').length;

    // Calculate Top Roles
    const roleCounts: Record<string, number> = {};
    requests.forEach(r => {
      const name = r.requestedRoleName || 'Unknown Role';
      roleCounts[name] = (roleCounts[name] || 0) + 1;
    });

    const topRequestedRoles = Object.entries(roleCounts)
      .map(([roleName, count]) => ({ roleName, count }))
      .sort((a, b) => b.count - a.count)
      .slice(0, 5);

    return {
      total,
      pending,
      approved,
      rejected,
      cancelled,
      topRequestedRoles,
      viewMode
    };
  },
};

export default dashboardService;
