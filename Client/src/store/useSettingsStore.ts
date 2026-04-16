import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface SettingsState {
  defaultWarehouseId: string | null;
  setDefaultWarehouse: (id: string) => void;
}

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      defaultWarehouseId: null,
      setDefaultWarehouse: (id) => set({ defaultWarehouseId: id }),
    }),
    {
      name: 'mini-erp-settings', // LocalStorage anahtarı
    }
  )
);