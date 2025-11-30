import apiClient from '../../../services/apiClient';
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
  createdAt: string;
}

export interface AuditLogQueryParams {
  page?: number;
  pageSize?: number;
  userId?: number;
  action?: string;
  dateFrom?: string; // ISO date string
  dateTo?: string; // ISO date string
}

export const auditLogService = {
  async getLogs(params: AuditLogQueryParams = {}): Promise<PaginatedResponse<AuditLog>> {
    const res = await apiClient.get('/audit-logs', { params });
    return (res && (res.data || res)) as PaginatedResponse<AuditLog>;
  },

  async getLogById(id: number): Promise<AuditLog> {
    const res = await apiClient.get(`/audit-logs/${id}`);
    return (res && (res.data || res)) as AuditLog;
  },
};

export default auditLogService;
