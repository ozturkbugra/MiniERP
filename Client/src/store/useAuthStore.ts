// store/useAuthStore.ts
import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import api from '../api/axiosInstance';

interface User {
  id: number;
  userName: string;
  role: string;
  permissions: string[];
}

interface AuthState {
  user: User | null;
  serverPermissions: string[];
  isAuthenticated: boolean;
  isInitialLoading: boolean; // API hızıyla yarışmamak için eklendi
  setUser: (token: string | null) => void;
  fetchPermissions: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  logout: () => void;
}

const getUserFromToken = (token: string): User | null => {
  try {
    const decoded: any = jwtDecode(token);
    return {
      id: decoded.id || 0,
      userName: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı",
      role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "User",
      permissions: decoded["permissions"] || [] 
    };
  } catch {
    return null;
  }
};

export const useAuthStore = create<AuthState>((set, get) => ({
  user: localStorage.getItem('token') ? getUserFromToken(localStorage.getItem('token')!) : null,
  serverPermissions: [],
  isAuthenticated: !!localStorage.getItem('token'),
  isInitialLoading: true, 

  setUser: (token) => {
    if (token) {
      const user = getUserFromToken(token);
      set({ user, isAuthenticated: true });
      get().fetchPermissions();
    } else {
      set({ user: null, isAuthenticated: false, serverPermissions: [], isInitialLoading: false });
    }
  },

  fetchPermissions: async () => {
    try {
      set({ isInitialLoading: true });
      const response = await api.get("/Auths/GetMyPermissions");
      if (response.data.isSuccess) {
        set({ serverPermissions: response.data.data });
      }
    } catch {
      set({ serverPermissions: [] });
    } finally {
      set({ isInitialLoading: false });
    }
  },

  hasPermission: (permission) => {
    const { user, serverPermissions } = get();
    // Admin yetki kontrolünü istersen burada aktif edebilirsin
    // if (user?.role === 'Admin') return true;

    if (serverPermissions.length > 0) {
        return serverPermissions.includes(permission);
    }
    return user?.permissions?.includes(permission) || false;
  },

  logout: async () => {
    localStorage.removeItem('token');
    set({ user: null, isAuthenticated: false, serverPermissions: [], isInitialLoading: false });
    window.location.href = '/login';
  },
}));