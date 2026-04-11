import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode'; // Token çözücü
import api from '../api/axiosInstance';

interface User {
  id: number;
  userName: string;
  role: string;
  permissions: string[];
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  setUser: (token: string | null) => void; // Artık direkt token alıyor
  hasPermission: (permission: string) => boolean;
  logout: () => void;
}

// Token'ı deşifre eden yardımcı fonksiyon
const getUserFromToken = (token: string): User | null => {
  try {
    const decoded: any = jwtDecode(token);
    // .NET tarafındaki claim isimlerine göre burayı eşleştiriyoruz
    return {
      id: decoded.id || 0,
      userName: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || "Kullanıcı",
      role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || "User",
      permissions: decoded["permissions"] || [] // Backend'den gelen policy listesi
    };
  } catch {
    return null;
  }
};

export const useAuthStore = create<AuthState>((set, get) => ({
  // Sayfa açıldığında localStorage'da token varsa direkt kullanıcıyı yükle (Persistence)
  user: localStorage.getItem('token') ? getUserFromToken(localStorage.getItem('token')!) : null,
  isAuthenticated: !!localStorage.getItem('token'),

  // Giriş yapınca token'ı al, hem sakla hem de parçalayıp user'ı set et
  setUser: (token) => {
    if (token) {
      const user = getUserFromToken(token);
      set({ user, isAuthenticated: true });
    } else {
      set({ user: null, isAuthenticated: false });
    }
  },

  logout: async () => {
    try {
      await api.post('/Auths/logout');
    } catch (error) {
      console.error("Logout hatası:", error);
    } finally {
      localStorage.removeItem('token');
      set({ user: null, isAuthenticated: false });
      window.location.href = '/login';
    }
  },

  hasPermission: (permission) => {
    const user = get().user;
    if (!user) return false;
    // Admin ise her kapı açılır, değilse listeye bakılır
    return user.role === 'Admin' || (user.permissions && user.permissions.includes(permission));
  },
}));