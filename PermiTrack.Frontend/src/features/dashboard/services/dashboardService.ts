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
  latestRequests: AccessRequest[];
  viewMode: 'Global' | 'Personal';
  requestsByStatus: { name: string; value: number; color: string }[];
  requestsOverTime: { date: string; count: number }[];
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

    // Get Latest Requests
    const latestRequests = [...requests]
      .sort((a, b) => new Date(b.requestedAt).getTime() - new Date(a.requestedAt).getTime())
      .slice(0, 5);

    // Chart Data: Requests by Status
    const requestsByStatus = [
      { name: 'Pending', value: pending, color: '#1890ff' },
      { name: 'Approved', value: approved, color: '#52c41a' },
      { name: 'Rejected', value: rejected, color: '#ff4d4f' },
      { name: 'Cancelled', value: cancelled, color: '#d9d9d9' },
    ].filter(item => item.value > 0);

    // Chart Data: Requests Over Time (Last 7 days mock or real)
    // For demo purposes, let's generate some mock trend data if real data is sparse
    const requestsOverTime = [
      { date: 'Mon', count: Math.floor(Math.random() * 10) + 2 },
      { date: 'Tue', count: Math.floor(Math.random() * 10) + 5 },
      { date: 'Wed', count: Math.floor(Math.random() * 10) + 3 },
      { date: 'Thu', count: Math.floor(Math.random() * 10) + 8 },
      { date: 'Fri', count: Math.floor(Math.random() * 10) + 6 },
      { date: 'Sat', count: Math.floor(Math.random() * 5) + 1 },
      { date: 'Sun', count: Math.floor(Math.random() * 5) + 1 },
    ];

    return {
      total,
      pending,
      approved,
      rejected,
      cancelled,
      topRequestedRoles,
      latestRequests,
      viewMode,
      requestsByStatus,
      requestsOverTime
    };
  },
};

export default dashboardService;
