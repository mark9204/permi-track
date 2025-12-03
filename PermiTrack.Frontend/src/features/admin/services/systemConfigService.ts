import apiClient from '../../../services/apiClient';

export interface SystemConfigOption {
  id: number;
  name: string;
  value: string;
}

// Mock data for now since we don't have a backend for this yet
let mockTargetSystems: SystemConfigOption[] = [
  { id: 1, name: 'ERP', value: 'ERP' },
  { id: 2, name: 'CRM', value: 'CRM' },
  { id: 3, name: 'HR', value: 'HR' },
  { id: 4, name: 'Finance', value: 'Finance' },
  { id: 5, name: 'IT', value: 'IT' },
];

let mockActions: SystemConfigOption[] = [
  { id: 1, name: 'Read', value: 'Read' },
  { id: 2, name: 'Write', value: 'Write' },
  { id: 3, name: 'Execute', value: 'Execute' },
  { id: 4, name: 'Admin', value: 'Admin' },
];

const systemConfigService = {
  async getTargetSystems(): Promise<SystemConfigOption[]> {
    // In a real app: return apiClient.get('/system-config/target-systems').then(res => res.data);
    return new Promise((resolve) => {
      setTimeout(() => resolve([...mockTargetSystems]), 500);
    });
  },

  async createTargetSystem(name: string): Promise<SystemConfigOption> {
    // In a real app: return apiClient.post('/system-config/target-systems', { name }).then(res => res.data);
    return new Promise((resolve) => {
      const newItem = { id: Date.now(), name, value: name };
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

  async getActions(): Promise<SystemConfigOption[]> {
    // In a real app: return apiClient.get('/system-config/actions').then(res => res.data);
    return new Promise((resolve) => {
      setTimeout(() => resolve([...mockActions]), 500);
    });
  },

  async createAction(name: string): Promise<SystemConfigOption> {
    // In a real app: return apiClient.post('/system-config/actions', { name }).then(res => res.data);
    return new Promise((resolve) => {
      const newItem = { id: Date.now(), name, value: name };
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
};

export default systemConfigService;
