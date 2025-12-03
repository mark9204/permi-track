import type { PaginatedResponse } from '../../../types/api.types';

export interface AuditLog {
  id: number;
  userId: number | null;
  userName?: string | null;
  action: string;
  resourceType: string;
  resourceId?: string | number | null;
  oldValues?: string | null;
  newValues?: string | null;
  ipAddress?: string | null;
  location?: string;
  device?: string;
  riskScore?: 'Low' | 'Medium' | 'High' | 'Critical';
  behaviorTags?: string[];
  createdAt: string;
}

export interface AuditLogQueryParams {
  page?: number;
  pageSize?: number;
  userId?: number;
  action?: string;
  dateFrom?: string;
  dateTo?: string;
}

// --- DEMO STATE ---
const generateMockLogs = (count: number): AuditLog[] => {
  const actions = ['Login', 'Logout', 'Create Role', 'Update Permission', 'Access File', 'Download Report', 'Failed Login'];
  const resources = ['System', 'User', 'Role', 'Document', 'Report', 'Database'];
  const users = ['admin', 'jdoe', 'asmith', 'unknown'];
  const locations = ['New York, US', 'London, UK', 'Berlin, DE', 'Tokyo, JP', 'Unknown Proxy'];
  const devices = ['Chrome / Windows', 'Safari / macOS', 'Firefox / Linux', 'Edge / Windows', 'Mobile Safari / iOS'];
  const risks: ('Low' | 'Medium' | 'High' | 'Critical')[] = ['Low', 'Low', 'Low', 'Medium', 'High', 'Critical'];

  return Array.from({ length: count }).map((_, i) => {
    const isHighRisk = Math.random() > 0.8;
    const risk = isHighRisk ? risks[Math.floor(Math.random() * 3) + 3] : 'Low';
    
    return {
      id: 1000 + i,
      userId: Math.floor(Math.random() * 5) + 1,
      userName: users[Math.floor(Math.random() * users.length)],
      action: actions[Math.floor(Math.random() * actions.length)],
      resourceType: resources[Math.floor(Math.random() * resources.length)],
      resourceId: Math.floor(Math.random() * 100),
      ipAddress: `192.168.${Math.floor(Math.random() * 255)}.${Math.floor(Math.random() * 255)}`,
      location: locations[Math.floor(Math.random() * locations.length)],
      device: devices[Math.floor(Math.random() * devices.length)],
      riskScore: risk,
      behaviorTags: isHighRisk ? ['Unusual Location', 'After Hours'] : ['Normal'],
      createdAt: new Date(Date.now() - Math.floor(Math.random() * 1000000000)).toISOString(),
      oldValues: null,
      newValues: JSON.stringify({ status: 'Active', role: 'Admin' })
    };
  }).sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
};

const mockLogs = generateMockLogs(50);

export const auditLogService = {
  async getLogs(params: AuditLogQueryParams = {}): Promise<PaginatedResponse<AuditLog>> {
    await new Promise(resolve => setTimeout(resolve, 600));
    
    let filtered = [...mockLogs];
    
    // Simple filtering simulation
    if (params.action) {
      filtered = filtered.filter(l => l.action.toLowerCase().includes(params.action!.toLowerCase()));
    }

    const page = params.page || 1;
    const pageSize = params.pageSize || 10;
    const start = (page - 1) * pageSize;
    const end = start + pageSize;

    return {
      data: filtered.slice(start, end),
      pagination: {
        page,
        pageSize,
        totalCount: filtered.length,
        totalPages: Math.ceil(filtered.length / pageSize)
      }
    };
  },

  async getLogById(id: number): Promise<AuditLog> {
    await new Promise(resolve => setTimeout(resolve, 300));
    const log = mockLogs.find(l => l.id === id);
    if (log) return log;
    throw new Error('Log not found');
  },
};

export default auditLogService;
