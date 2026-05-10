// components/Layout/guards/PermissionGuard.tsx
import React, { useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../../../store/useAuthStore';

interface PermissionGuardProps {
  children: React.ReactNode;
  requiredPermission: string;
}

const PermissionGuard: React.FC<PermissionGuardProps> = ({ children, requiredPermission }) => {
  const { hasPermission, isAuthenticated, user, fetchPermissions, isInitialLoading } = useAuthStore();

  useEffect(() => {
    if (isAuthenticated && user && isInitialLoading) {
      fetchPermissions();
    }
  }, [isAuthenticated, user, fetchPermissions, isInitialLoading]);

  // API'den yetki cevabı gelene kadar hiçbir yere yönlendirme yapma
  if (isInitialLoading) {
    return null; // Buraya küçük bir Spinner bileşeni de koyabilirsin
  }

  // 1. Giriş kontrolü
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // 2. Yetki kontrolü
  if (!hasPermission(requiredPermission)) {
    // App.tsx içindeki <Route path="unauthorized" ... /> ile tam uyumlu
    return <Navigate to="/unauthorized" replace />;
  }

  // 3. Geçiş serbest
  return <>{children}</>;
};

export default PermissionGuard;