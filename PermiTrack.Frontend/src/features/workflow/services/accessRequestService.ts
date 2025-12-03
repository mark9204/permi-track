import apiClient from '../../../services/apiClient';
import type { AccessRequest, SubmitRequestPayload } from '../types';

const extractArray = (res: any): any[] => {
  if (!res) return [];
  // 1. Ha a válasz { requests: [...] }
  if (res.data && Array.isArray(res.data.requests)) return res.data.requests;
  // 2. Ha a válasz { data: [...] }
  if (Array.isArray(res.data)) return res.data;
  // 3. Ha a válasz { data: { data: [...] } }
  if (res.data && Array.isArray(res.data.data)) return res.data.data;
  
  return [];
};

const accessRequestService = {
  async getMyRequests(): Promise<AccessRequest[]> {
    const res = await apiClient.get('/access-requests/my-requests');
    return extractArray(res) as AccessRequest[];
  },

  async getAllRequests(): Promise<AccessRequest[]> {
    // Calls the endpoint that returns all requests (filtering can be added later)
    const res = await apiClient.get('/access-requests');
    return extractArray(res) as AccessRequest[];
  },

  async submitRequest(payload: SubmitRequestPayload): Promise<AccessRequest> {
    const body = {
      requestedRoleId: payload.roleId,
      reason: payload.reason,
      requestedDurationHours: payload.durationHours,
      requestType: payload.requestType,
      targetSystem: payload.targetSystem,
      action: payload.action,
    };

    const res = await apiClient.post('/access-requests/submit', body);
    // prefer res.data if available
    return (res && (res.data || res)) as AccessRequest;
  },

  async cancelRequest(id: number): Promise<AccessRequest> {
    const res = await apiClient.put(`/access-requests/${id}/cancel`);
    return (res && (res.data || res)) as AccessRequest;
  },

  async getPendingRequests(): Promise<AccessRequest[]> {
    const res = await apiClient.get('/access-requests/pending');
    return extractArray(res) as AccessRequest[];
  },

  async approveRequest(id: number): Promise<AccessRequest> {
    const res = await apiClient.put(`/access-requests/${id}/approve`);
    return (res && (res.data || res)) as AccessRequest;
  },

  async rejectRequest(id: number, comment: string): Promise<AccessRequest> {
    const res = await apiClient.put(`/access-requests/${id}/reject`, { reviewerComment: comment });
    return (res && (res.data || res)) as AccessRequest;
  },
};

export default accessRequestService;
