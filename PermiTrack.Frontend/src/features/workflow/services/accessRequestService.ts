import type { AccessRequest, SubmitRequestPayload } from '../types';

// --- DEMO STATE ---
let mockRequests: AccessRequest[] = [
  { 
    id: 101, 
    userId: 2, 
    requestedRoleId: 1, 
    requestedRoleName: 'IT Manager',
    status: 'Pending', 
    requestedAt: '2025-12-01T10:00:00Z', 
    reason: 'Project Alpha Access', 
    uncPath: '',
    reviewerComment: ''
  },
  { 
    id: 104, 
    userId: 4, 
    requestedRoleId: 10, 
    requestedRoleName: 'Server Room Entry',
    status: 'Pending', 
    requestedAt: '2025-12-04T08:15:00Z', 
    reason: 'Need physical access for server maintenance', 
    uncPath: '',
    reviewerComment: ''
  },
  { 
    id: 105, 
    userId: 5, 
    requestedRoleId: 11, 
    requestedRoleName: 'Finance Data Export',
    status: 'Pending', 
    requestedAt: '2025-12-04T09:30:00Z', 
    reason: 'Q4 Audit Preparation', 
    uncPath: '',
    reviewerComment: ''
  },
  { 
    id: 106, 
    userId: 6, 
    requestedRoleId: 12, 
    requestedRoleName: 'VPN Access',
    status: 'Pending', 
    requestedAt: '2025-12-03T16:45:00Z', 
    reason: 'Remote work request for next week', 
    uncPath: '',
    reviewerComment: ''
  },
  { 
    id: 107, 
    userId: 2, 
    requestedRoleId: 13, 
    requestedRoleName: 'Project Blueprints',
    status: 'Pending', 
    requestedAt: '2025-12-04T11:00:00Z', 
    reason: 'Architecture review', 
    uncPath: '\\\\server\\blueprints\\project-x',
    reviewerComment: ''
  },
  { 
    id: 102, 
    userId: 3, 
    requestedRoleId: 2, 
    requestedRoleName: 'HR Specialist',
    status: 'Approved', 
    requestedAt: '2025-11-28T14:30:00Z', 
    approvedAt: '2025-11-29T09:00:00Z',
    reason: 'HR Onboarding', 
    uncPath: '',
    reviewerComment: 'Approved by Admin'
  },
  { 
    id: 103, 
    userId: 1, 
    requestedRoleId: 3, 
    requestedRoleName: 'Finance Auditor',
    status: 'Rejected', 
    requestedAt: '2025-11-20T11:15:00Z', 
    rejectedAt: '2025-11-21T16:45:00Z',
    reason: 'Wrong department', 
    uncPath: '',
    reviewerComment: 'Please request for your own department'
  },
];

const accessRequestService = {
  async getMyRequests(): Promise<AccessRequest[]> {
    await new Promise(resolve => setTimeout(resolve, 400)); // Simulate network latency
    return [...mockRequests];
  },

  async getAllRequests(): Promise<AccessRequest[]> {
    await new Promise(resolve => setTimeout(resolve, 400));
    return [...mockRequests];
  },

  async submitRequest(payload: SubmitRequestPayload): Promise<AccessRequest> {
    await new Promise(resolve => setTimeout(resolve, 800));
    
    const newReq: AccessRequest = {
      id: Math.floor(Math.random() * 10000),
      userId: 1, // Simulating current user
      requestedRoleId: payload.roleId,
      requestedRoleName: 'Requested Role', // In a real app this would be fetched
      status: 'Pending',
      requestedAt: new Date().toISOString(),
      reason: payload.reason,
      uncPath: payload.uncPath,
      reviewerComment: ''
    };
    
    mockRequests.unshift(newReq); // Add to top of list
    return newReq;
  },

  async cancelRequest(id: number): Promise<AccessRequest> {
    await new Promise(resolve => setTimeout(resolve, 500));
    const req = mockRequests.find(r => r.id === id);
    if (req) {
      req.status = 'Cancelled';
      return { ...req };
    }
    throw new Error('Request not found');
  },

  async getPendingRequests(): Promise<AccessRequest[]> {
    await new Promise(resolve => setTimeout(resolve, 400));
    return mockRequests.filter(r => r.status === 'Pending');
  },

  async approveRequest(id: number): Promise<AccessRequest> {
    await new Promise(resolve => setTimeout(resolve, 500));
    const req = mockRequests.find(r => r.id === id);
    if (req) {
      req.status = 'Approved';
      req.approvedAt = new Date().toISOString();
      return { ...req };
    }
    throw new Error('Request not found');
  },

  async rejectRequest(id: number, comment: string): Promise<AccessRequest> {
    await new Promise(resolve => setTimeout(resolve, 500));
    const req = mockRequests.find(r => r.id === id);
    if (req) {
      req.status = 'Rejected';
      req.rejectedAt = new Date().toISOString();
      req.reviewerComment = comment;
      return { ...req };
    }
    throw new Error('Request not found');
  },
};

export default accessRequestService;
