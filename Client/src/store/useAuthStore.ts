import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import api from '../api/axiosInstance';

interface User {
  id: number;
  userName: string;
  role: string;
  permissions: string[]; // Token'dan gelenler (yedek)
}

interface AuthState {
  user: User | null;
  serverPermissions: string[]; // API'den gelen taze yetkiler
  isAuthenticated: boolean;
  setUser: (token: string | null) => void;
  fetchPermissions: () => Promise<void>; // Taze yetkileri çekme fonksiyonu
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
  serverPermissions: [], // Başlangıçta boş
  isAuthenticated: !!localStorage.getItem('token'),

  setUser: (token) => {
    if (token) {
      const user = getUserFromToken(token);
      set({ user, isAuthenticated: true });
      // Giriş yapınca hemen taze yetkileri çekelim
      get().fetchPermissions();
    } else {
      set({ user: null, isAuthenticated: false, serverPermissions: [] });
    }
  },

  fetchPermissions: async () => {
    try {
      console.log("🔍 API'den taze yetkiler isteniyor...");
      const response = await api.get("/Auths/GetMyPermissions");
      
      if (response.data.isSuccess) {
        //console.log("✅ API'den Gelen Yetkiler:", response.data.data);
        set({ serverPermissions: response.data.data });
      }
    } catch (error) {
      console.error("❌ Yetki çekme hatası:", error);
    }
  },

  hasPermission: (permission) => {
    const { user, serverPermissions } = get();
    
    // 1. Admin ise her zaman true
    //if (user?.role === 'Admin') return true;

    // 2. Önce API'den gelen taze listeye bak (ServerPermissions)
    if (serverPermissions.length > 0) {
        const hasIt = serverPermissions.includes(permission);
        console.log(`🔐 [API Kontrol] ${permission} : ${hasIt}`);
        return hasIt;
    }

    // 3. API henüz dönmediyse yedek olarak Token'a bak
    const hasItToken = user?.permissions?.includes(permission) || false;
    console.log(`🎫 [Token Kontrol] ${permission} : ${hasItToken}`);
    return hasItToken;
  },

  logout: async () => {
    localStorage.removeItem('token');
    set({ user: null, isAuthenticated: false, serverPermissions: [] });
    window.location.href = '/login';
  },
}));