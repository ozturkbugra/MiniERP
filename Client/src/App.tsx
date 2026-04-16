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
import RolePermissions from './pages/RolePermissions';
import UserDetails from './pages/UserDetails';
import Categories from './pages/Categories';
import Brands from './pages/Brands';
import Units from './pages/Units';
import Warehouses from "./pages/Warehouses";
import Banks from './pages/Banks';
import Cashes from './pages/Cashes';
import Customers from './pages/Customers';
import Products from './pages/Products';
import StockTransactions from './pages/StockTransactions';
import Finance from './pages/Finance'; // 🚀 YENİ

function App() {
  const { isAuthenticated } = useAuthStore();
  const hasToken = !!localStorage.getItem('token');
  const isLogged = isAuthenticated || hasToken;

  return (
    <Routes>
      {/* 🔓 BAĞIMSIZ SAYFALAR */}
      <Route
        path="/login"
        element={!isLogged ? <Login /> : <Navigate to="/" />}
      />
      <Route path="/unauthorized" element={<Unauthorized />} />

      {/* 🔒 KORUMALI ALAN (MainLayout) */}
      <Route
        path="/"
        element={isLogged ? <MainLayout /> : <Navigate to="/login" />}
      >
        {/* Dashboard herkese açık */}
        <Route index element={<Dashboard />} />

        {/* ⚙️ SİSTEM YÖNETİMİ */}
        <Route
          path="users"
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Users.View}>
              <Users />
            </PermissionGuard>
          }
        />
        <Route
          path="roles"
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Roles.View}>
              <Roles />
            </PermissionGuard>
          }
        />
        <Route path="roles/:roleId/permissions" element={<RolePermissions />} />
        <Route path="users/:userId/details" element={<UserDetails />} />

        {/* 🛠️ TANIMLAMALAR (Tümü Guard ile Korunuyor) */}
        <Route path="categories" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Categories.View}><Categories /></PermissionGuard>} />
        <Route path="brands" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Brands.View}><Brands /></PermissionGuard>} />
        <Route path="units" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Units.View}><Units /></PermissionGuard>} />
        <Route path="warehouses" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Warehouses.View}><Warehouses /></PermissionGuard>} />
        <Route path="banks" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Banks.View}><Banks /></PermissionGuard>} />
        <Route path="cashes" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Cashes.View}><Cashes /></PermissionGuard>} />

        {/* 🤝 TİCARET VE STOK */}
        <Route path="customers" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Customers.View}><Customers /></PermissionGuard>} />
        <Route path="products" element={<PermissionGuard requiredPermission={APP_PERMISSIONS.Products.View}><Products /></PermissionGuard>} />
        <Route 
            path="stocktransactions" 
            element={
                <PermissionGuard requiredPermission={APP_PERMISSIONS.StockTransactions.View}>
                    <StockTransactions />
                </PermissionGuard>
            } 
        />

        {/* 💸 FİNANS YÖNETİMİ */}
        <Route
          path="finance"
          element={
            <PermissionGuard requiredPermission={APP_PERMISSIONS.Finance.View}>
              <Finance />
            </PermissionGuard>
          }
        />

        {/* Muhasebe Raporları vb. için durabilir */}
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