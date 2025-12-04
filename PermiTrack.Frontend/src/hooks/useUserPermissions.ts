import { useAuthStore } from '../stores/authStore';

export const useUserPermissions = () => {
  const { user } = useAuthStore();

  const rawRoles = user?.roles ?? [];

  const isSuperAdmin = Array.isArray(rawRoles) && rawRoles.some((r: any) => {
    if (!r) return false;
    if (typeof r === 'object') {
      const level = r.Level ?? r.level;
      if (level !== undefined && level !== null) {
        const parsed = Number(level);
        if (!isNaN(parsed) && parsed >= 3) return true;
      }
      const name = r.name || r.roleName || r.role || '';
      return String(name).toLowerCase() === 'superadmin';
    }
    if (typeof r === 'string') {
      return r.toLowerCase() === 'superadmin';
    }
    return false;
  });

  const isAdmin = Array.isArray(rawRoles) && rawRoles.some((r: any) => {
    if (!r) return false;
    if (typeof r === 'object') {
      const level = r.Level ?? r.level;
      if (level !== undefined && level !== null) {
        const parsed = Number(level);
        if (!isNaN(parsed) && parsed >= 2) return true;
      }
      const name = r.name || r.roleName || r.role || '';
      const lower = String(name).toLowerCase();
      return lower === 'admin' || lower === 'superadmin';
    }
    if (typeof r === 'string') {
      const lower = r.toLowerCase();
      return lower === 'admin' || lower === 'superadmin';
    }
    return false;
  });

  const isManager = Array.isArray(rawRoles) && rawRoles.some((r: any) => {
    if (!r) return false;
    if (typeof r === 'object') {
      const name = r.name || r.roleName || r.role || '';
      const lower = String(name).toLowerCase();
      return lower === 'manager' || lower === 'admin' || lower === 'superadmin';
    }
    if (typeof r === 'string') {
      const lower = r.toLowerCase();
      return lower === 'manager' || lower === 'admin' || lower === 'superadmin';
    }
    return false;
  });

  return {
    isSuperAdmin,
    isAdmin, // This effectively means Level 2 or higher
    isManager,
    userDepartment: user?.department,
    user
  };
};
