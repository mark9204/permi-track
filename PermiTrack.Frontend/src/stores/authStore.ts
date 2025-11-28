import { create } from 'zustand';
import type { User } from '../types/auth.types';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  setUser: (user: User | null) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => {
  // Initialize logic
  const storedUser = localStorage.getItem('user');
  const initialUser = storedUser ? JSON.parse(storedUser) : null;
  const initialIsAuthenticated = !!initialUser;

  return {
    user: initialUser,
    isAuthenticated: initialIsAuthenticated,
    setUser: (user) => set({ user, isAuthenticated: !!user }),
    logout: () => {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      set({ user: null, isAuthenticated: false });
      // Optional: Redirect to login if needed, but usually handled by router
      // window.location.href = '/login'; 
    },
  };
});
