import { Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from './components/Layout/MainLayout';
import Dashboard from './pages/Dashboard';
import Users from './pages/Users';
import Accounting from './pages/Accounting';
import Login from './pages/Login';
import { useAuthStore } from './store/useAuthStore'; // 🧠 Beyni bağladık

function App() {
  // Zustand store'u dinliyoruz. 
  // Login.tsx içinde setUser çağrıldığı an bu "App" bileşeni kendini yeniler.
  const { isAuthenticated } = useAuthStore();

  // Sayfa yenilendiğinde store boşalacağı için localStorage'ı da yedek kontrol olarak tutuyoruz.
  const hasToken = !!localStorage.getItem('token');
  const isLogged = isAuthenticated || hasToken;

  return (
    <Routes>
      {/* 🔓 AÇIK KAPI: Login Sayfası */}
      <Route 
        path="/login" 
        element={!isLogged ? <Login /> : <Navigate to="/" />}
      />

      {/* 🔒 KAPALI KAPI: Ana Uygulama (MainLayout) */}
      <Route 
        path="/" 
        element={isLogged ? <MainLayout /> : <Navigate to="/login" />}
      >
        <Route index element={<Dashboard />} />
        <Route path="users" element={<Users />} />
        <Route path="accounting" element={<Accounting />} />
      </Route>

      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  );
}

export default App;