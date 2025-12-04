import apiClient from '../../../services/apiClient';

export interface Permission {
  id: number;
  name: string;
  description: string;
  category?: string;
  department?: string;
  targetSystem?: string;
  action?: string;
}

export interface CreatePermissionRequest {
  name: string;
  description: string;
  category?: string;
  department?: string;
  targetSystem?: string;
  action?: string;
}

// --- DEMO STATE ---
let mockPermissions: Permission[] = [
  { 
    id: 1, 
    name: 'ERP Read Access', 
    description: 'Allows reading data from ERP', 
    category: 'Systems & Applications', 
    department: 'IT', 
    targetSystem: 'ERP', 
    action: 'Read' 
  },
  { 
    id: 2, 
    name: 'Server Room Entry', 
    description: 'Physical access to server room', 
    category: 'Physical Spaces', 
    department: 'IT', 
    targetSystem: 'Server Room', 
    action: 'Entry' 
  },
  { 
    id: 3, 
    name: 'Finance Data Export', 
    description: 'Export financial reports', 
    category: 'Data & Resources', 
    department: 'Finance', 
    targetSystem: 'Finance Data', 
    action: 'Export' 
  },
  { 
    id: 4, 
    name: 'Laptop Assignment', 
    description: 'Assign laptops to employees', 
    category: 'Hardware & Assets', 
    department: 'IT', 
    targetSystem: 'Company Laptops', 
    action: 'Assign' 
  },
  { 
    id: 5, 
    name: 'Budget Approval', 
    description: 'Approve department budgets', 
    category: 'Governance & Authority', 
    department: 'Finance', 
    targetSystem: 'Budget Committee', 
    action: 'Approve' 
  },
  { 
    id: 6, 
    name: 'Confidential Docs Read', 
    description: 'Read access to confidential HR docs', 
    category: 'File System', 
    department: 'HR', 
    targetSystem: 'Confidential Docs', 
    action: 'Read' 
  },
  { 
    id: 7, 
    name: 'Shared Drive Write', 
    description: 'Write access to the main shared drive', 
    category: 'File System', 
    department: 'IT', 
    targetSystem: 'Shared Drive', 
    action: 'Write' 
  }
];

const permissionService = {
  async getAllPermissions(): Promise<Permission[]> {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 600));
    return [...mockPermissions];
  },

  async createPermission(data: CreatePermissionRequest): Promise<Permission> {
    await new Promise(resolve => setTimeout(resolve, 600));
    const newPermission: Permission = {
      id: Math.floor(Math.random() * 10000),
      ...data
    };
    mockPermissions.push(newPermission);
    return newPermission;
  },

  async updatePermission(id: number, data: CreatePermissionRequest): Promise<Permission> {
    await new Promise(resolve => setTimeout(resolve, 600));
    const index = mockPermissions.findIndex(p => p.id === id);
    if (index !== -1) {
      mockPermissions[index] = { ...mockPermissions[index], ...data };
      return mockPermissions[index];
    }
    throw new Error('Permission not found');
  },

  async deletePermission(id: number): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 600));
    mockPermissions = mockPermissions.filter(p => p.id !== id);
  },
};

export default permissionService;
