export type RequestStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';

export interface AccessRequest {
  id: number;
  userId: number;
  requestedRoleId: number;
  requestedRoleName: string;
  status: RequestStatus;
  reason: string;
  requestedAt: string;
  approvedAt?: string | null;
  rejectedAt?: string | null;
  reviewerComment?: string | null;
}

export interface SubmitRequestPayload {
  roleId: number;
  reason: string;
  durationHours?: number;
  requestType?: string;
  targetSystem?: string;
  action?: string;
}
