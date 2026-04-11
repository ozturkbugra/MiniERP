import { create } from 'zustand';

interface User {
  id: number;
  userName: string;
  role: string;
  // Backend'den JWT ile gelen 'AppPermissions.StockTransactions.View' gibi string listesi
  permissions: string[]; 
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  setUser: (user: User | null) => void;
  // .NET'teki [Authorize(Policy = "...")] mantığının React hali aga
  hasPermission: (permission: string) => boolean;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  isAuthenticated: false,

  setUser: (user) => set({ user, isAuthenticated: !!user }),

  hasPermission: (permission) => {
    const user = get().user;
    if (!user) return false;
    // Admin ise her şeyi görür, değilse permissions dizisinde o string var mı diye bakar
    return user.role === 'Admin' || user.permissions.includes(permission);
  },

  logout: () => {
    set({ user: null, isAuthenticated: false });
    localStorage.removeItem('token');
  },
}));