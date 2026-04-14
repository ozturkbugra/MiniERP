import { Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from './components/Layout/MainLayout';
import Dashboard from './pages/Dashboard';
import Users from './pages/Users';
import Roles from './pages/Roles'
import Accounting from './pages/Accounting';
import Login from './pages/Login';
import Unauthorized from './pages/Unauthorized';
import { useAuthStore } from './store/useAuthStore';
import { APP_PERMISSIONS } from './constants/permissions';
import PermissionGuard from './components/Layout/guards/PermissionGuard';
import RolePermissions from './pages/RolePermissions'; // Yeni sayfayı import et
import UserDetails from './pages/UserDetails';
import Categories from './pages/Categories';
import Brands from './pages/Brands';

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

        {/* 🛡️ 2. ROL YÖNETİMİ: Burayı ekliyoruz */}
        <Route
          path="roles"
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Roles.View}>
              <Roles />
            </PermissionGuard>
          }
        />

        <Route path="/roles/:roleId/permissions" element={<RolePermissions />} />
        <Route path="/users/:userId/details" element={<UserDetails />} />


        <Route path="/categories" element={<Categories />} />
        <Route path="/brands" element={<Brands />} />
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