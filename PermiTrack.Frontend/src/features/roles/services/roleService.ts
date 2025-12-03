export interface Role {
  id: number;
  name: string;
  description: string;
  userCount?: number;
  department?: string;
  permissions?: any[];
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  department?: string;
}

// --- DEMO STATE ---
let mockRoles: Role[] = [
  { id: 1, name: 'IT Manager', description: 'Manages IT resources', department: 'IT', permissions: [] },
  { id: 2, name: 'HR Specialist', description: 'Handles employee data', department: 'HR', permissions: [] },
  { id: 3, name: 'Finance Auditor', description: 'Read-only access to finance', department: 'Finance', permissions: [] },
];

const roleService = {
  async getAllRoles(): Promise<Role[]> {
    await new Promise(resolve => setTimeout(resolve, 300));
    return [...mockRoles];
  },

  async createRole(data: CreateRoleRequest): Promise<Role> {
    await new Promise(resolve => setTimeout(resolve, 600));
    const newRole: Role = {
      id: Math.floor(Math.random() * 10000),
      ...data,
      permissions: []
    };
    mockRoles.push(newRole);
    return newRole;
  },

  async updateRole(id: number, data: CreateRoleRequest): Promise<Role> {
    await new Promise(resolve => setTimeout(resolve, 500));
    const index = mockRoles.findIndex(r => r.id === id);
    if (index !== -1) {
      mockRoles[index] = { ...mockRoles[index], ...data };
      return mockRoles[index];
    }
    throw new Error('Role not found');
  },

  async deleteRole(id: number): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 500));
    mockRoles = mockRoles.filter(r => r.id !== id);
  },

  async addPermissionToRole(roleId: number, permissionId: number): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 400));
    // In a real app, we'd link them. For demo, we just simulate success.
    console.log(`Added permission ${permissionId} to role ${roleId}`);
  }
};

export default roleService;
