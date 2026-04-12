import { Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from './components/Layout/MainLayout';
import Dashboard from './pages/Dashboard';
import Users from './pages/Users';
import Accounting from './pages/Accounting';
import Login from './pages/Login';
import Unauthorized from './pages/Unauthorized'; 
import { useAuthStore } from './store/useAuthStore';
import { APP_PERMISSIONS } from './constants/permissions'; 
import PermissionGuard from './components/Layout/guards/PermissionGuard';

function App() {
  const { isAuthenticated } = useAuthStore();
  const hasToken = !!localStorage.getItem('token');
  const isLogged = isAuthenticated || hasToken;

  return (
    <Routes>
      {/* 🔓 BAĞIMSIZ SAYFALAR (Layout Dışı) */}
      <Route 
        path="/login" 
        element={!isLogged ? <Login /> : <Navigate to="/" />}
      />

      {/* 🛡️ TAM EKRAN HATA SAYFASI (Artık Sidebar Yok) */}
      <Route path="/unauthorized" element={<Unauthorized />} />

      {/* 🔒 KORUMALI ALAN (MainLayout İçindeki Sayfalar) */}
      <Route 
        path="/" 
        element={isLogged ? <MainLayout /> : <Navigate to="/login" />}
      >
        {/* Dashboard herkese açık */}
        <Route index element={<Dashboard />} />

        {/* Yetki Gerektiren Sayfalar */}
        <Route 
          path="users" 
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Users.View}>
              <Users />
            </PermissionGuard>
          } 
        />

        <Route 
          path="accounting" 
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Finance.View}>
              <Accounting />
            </PermissionGuard>
          } 
        />
      </Route>

      {/* Tanımsız tüm yolları ana sayfaya yönlendir */}
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
}

export default App;