
export interface SystemConfigOption {
  id: number;
  name: string;
  value: string;
}

export interface TargetSystem extends SystemConfigOption {
  category: string;
  departmentId?: number;
}

export interface Action extends SystemConfigOption {
  targetSystemId?: number;
}

export interface Department extends SystemConfigOption {}

export const SYSTEM_CATEGORIES = [
  'Systems & Applications',
  'Data & Resources',
  'Physical Spaces',
  'Hardware & Assets',
  'Governance & Authority',
  'File System'
];

// Mock data for now since we don't have a backend for this yet
let mockTargetSystems: TargetSystem[] = [
  // Systems & Applications
  { id: 1, name: 'ERP', value: 'ERP', category: 'Systems & Applications', departmentId: 1 },
  { id: 2, name: 'CRM', value: 'CRM', category: 'Systems & Applications', departmentId: 4 },
  { id: 3, name: 'HR System', value: 'HR System', category: 'Systems & Applications', departmentId: 2 },
  
  // Data & Resources
  { id: 4, name: 'Finance Data', value: 'Finance Data', category: 'Data & Resources', departmentId: 3 },
  { id: 20, name: 'Customer Database', value: 'Customer Database', category: 'Data & Resources', departmentId: 4 },

  // Governance & Authority
  { id: 5, name: 'IT Governance', value: 'IT Governance', category: 'Governance & Authority', departmentId: 1 },
  { id: 21, name: 'Budget Committee', value: 'Budget Committee', category: 'Governance & Authority', departmentId: 3 },

  // Physical Spaces
  { id: 6, name: 'Server Room', value: 'Server Room', category: 'Physical Spaces', departmentId: 1 },
  { id: 7, name: 'Main Office', value: 'Main Office', category: 'Physical Spaces', departmentId: 4 },
  { id: 8, name: 'Warehouse', value: 'Warehouse', category: 'Physical Spaces', departmentId: 4 },
  { id: 9, name: 'Meeting Room A', value: 'Meeting Room A', category: 'Physical Spaces', departmentId: 2 },

  // Hardware & Assets
  { id: 22, name: 'Company Laptops', value: 'Company Laptops', category: 'Hardware & Assets', departmentId: 1 },
  { id: 23, name: 'Projectors', value: 'Projectors', category: 'Hardware & Assets', departmentId: 4 },
  { id: 24, name: 'Company Cars', value: 'Company Cars', category: 'Hardware & Assets', departmentId: 4 },

  // File System
  { id: 25, name: 'Shared Drive', value: 'Shared Drive', category: 'File System', departmentId: 1 },
  { id: 26, name: 'Confidential Docs', value: 'Confidential Docs', category: 'File System', departmentId: 2 },
  { id: 27, name: 'Project Blueprints', value: 'Project Blueprints', category: 'File System', departmentId: 4 },
];

let mockActions: Action[] = [
  // Systems & Applications Actions
  { id: 1, name: 'Read', value: 'Read', targetSystemId: 1 },
  { id: 2, name: 'Write', value: 'Write', targetSystemId: 1 },
  { id: 3, name: 'Execute', value: 'Execute', targetSystemId: 2 },
  { id: 15, name: 'View', value: 'View', targetSystemId: 3 },
  { id: 16, name: 'Edit', value: 'Edit', targetSystemId: 3 },

  // Governance & Authority Actions
  { id: 4, name: 'Admin', value: 'Admin', targetSystemId: 5 },
  { id: 30, name: 'Approve', value: 'Approve', targetSystemId: 21 },
  { id: 31, name: 'Reject', value: 'Reject', targetSystemId: 21 },
  { id: 32, name: 'Review', value: 'Review', targetSystemId: 5 },

  // Physical Spaces Actions
  { id: 10, name: 'Entry', value: 'Entry', targetSystemId: 6 },
  { id: 11, name: 'Entry', value: 'Entry', targetSystemId: 7 },
  { id: 12, name: 'Entry', value: 'Entry', targetSystemId: 8 },
  { id: 13, name: 'Entry', value: 'Entry', targetSystemId: 9 },
  { id: 14, name: 'Lock', value: 'Lock', targetSystemId: 6 },
  { id: 18, name: 'Unlock', value: 'Unlock', targetSystemId: 6 },

  // Data & Resources Actions
  { id: 17, name: 'Audit', value: 'Audit', targetSystemId: 4 },
  { id: 33, name: 'Export', value: 'Export', targetSystemId: 4 },
  { id: 34, name: 'Import', value: 'Import', targetSystemId: 20 },

  // Hardware & Assets Actions
  { id: 35, name: 'Assign', value: 'Assign', targetSystemId: 22 },
  { id: 36, name: 'Return', value: 'Return', targetSystemId: 22 },
  { id: 37, name: 'Repair', value: 'Repair', targetSystemId: 23 },
  { id: 38, name: 'Book', value: 'Book', targetSystemId: 24 },

  // File System Actions
  { id: 39, name: 'Read', value: 'Read', targetSystemId: 25 },
  { id: 40, name: 'Write', value: 'Write', targetSystemId: 25 },
  { id: 41, name: 'Delete', value: 'Delete', targetSystemId: 26 },
  { id: 42, name: 'Share', value: 'Share', targetSystemId: 27 },
  { id: 43, name: 'Archive', value: 'Archive', targetSystemId: 26 },
];

let mockDepartments: Department[] = [
  { id: 1, name: 'IT', value: 'IT' },
  { id: 2, name: 'HR', value: 'HR' },
  { id: 3, name: 'Finance', value: 'Finance' },
  { id: 4, name: 'Operations', value: 'Operations' },
];

const systemConfigService = {
  async getTargetSystems(): Promise<TargetSystem[]> {
    // In a real app: return apiClient.get('/system-config/target-systems').then(res => res.data);
    return new Promise((resolve) => {
      setTimeout(() => resolve([...mockTargetSystems]), 500);
    });
  },

  async createTargetSystem(name: string, category: string, departmentId?: number): Promise<TargetSystem> {
    // In a real app: return apiClient.post('/system-config/target-systems', { name, category, departmentId }).then(res => res.data);
    return new Promise((resolve) => {
      const newItem = { id: Date.now(), name, value: name, category, departmentId };
      mockTargetSystems.push(newItem);
      setTimeout(() => resolve(newItem), 500);
    });
  },

  async deleteTargetSystem(id: number): Promise<void> {
    // In a real app: return apiClient.delete(`/system-config/target-systems/${id}`);
    return new Promise((resolve) => {
      mockTargetSystems = mockTargetSystems.filter(item => item.id !== id);
      setTimeout(() => resolve(), 500);
    });
  },

  async getActions(): Promise<Action[]> {
    // In a real app: return apiClient.get('/system-config/actions').then(res => res.data);
    return new Promise((resolve) => {
      setTimeout(() => resolve([...mockActions]), 500);
    });
  },

  async createAction(name: string, targetSystemId: number): Promise<Action> {
    // In a real app: return apiClient.post('/system-config/actions', { name, targetSystemId }).then(res => res.data);
    return new Promise((resolve) => {
      const newItem = { id: Date.now(), name, value: name, targetSystemId };
      mockActions.push(newItem);
      setTimeout(() => resolve(newItem), 500);
    });
  },

  async deleteAction(id: number): Promise<void> {
    // In a real app: return apiClient.delete(`/system-config/actions/${id}`);
    return new Promise((resolve) => {
      mockActions = mockActions.filter(item => item.id !== id);
      setTimeout(() => resolve(), 500);
    });
  },

  async getDepartments(): Promise<Department[]> {
    return new Promise((resolve) => {
      setTimeout(() => resolve([...mockDepartments]), 500);
    });
  },

  async createDepartment(name: string): Promise<Department> {
    return new Promise((resolve) => {
      const newItem = { id: Date.now(), name, value: name };
      mockDepartments.push(newItem);
      setTimeout(() => resolve(newItem), 500);
    });
  },

  async deleteDepartment(id: number): Promise<void> {
    return new Promise((resolve) => {
      mockDepartments = mockDepartments.filter(item => item.id !== id);
      setTimeout(() => resolve(), 500);
    });
  },
};

export default systemConfigService;
