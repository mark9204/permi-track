import apiClient from '../../../services/apiClient';

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
  { id: 1, name: 'ERP', value: 'ERP', category: 'Systems & Applications', departmentId: 1 },
  { id: 2, name: 'CRM', value: 'CRM', category: 'Systems & Applications', departmentId: 4 },
  { id: 3, name: 'HR', value: 'HR', category: 'Systems & Applications', departmentId: 2 },
  { id: 4, name: 'Finance', value: 'Finance', category: 'Data & Resources', departmentId: 3 },
  { id: 5, name: 'IT', value: 'IT', category: 'Governance & Authority', departmentId: 1 },
];

let mockActions: Action[] = [
  { id: 1, name: 'Read', value: 'Read', targetSystemId: 1 },
  { id: 2, name: 'Write', value: 'Write', targetSystemId: 1 },
  { id: 3, name: 'Execute', value: 'Execute', targetSystemId: 2 },
  { id: 4, name: 'Admin', value: 'Admin', targetSystemId: 5 },
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
