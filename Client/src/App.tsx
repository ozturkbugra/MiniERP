import { Routes, Route } from 'react-router-dom';
import MainLayout from './components/Layout/MainLayout';
import Dashboard from './pages/Dashboard';
import Users from './pages/Users';
import Accounting from './pages/Accounting';

function App() {
  return (
    <Routes>
      {/* MainLayout burada kapsayıcı oluyor */}
      <Route path="/" element={<MainLayout />}>
        {/* '/' adresinde Dashboard açılır */}
        <Route index element={<Dashboard />} />
        {/* '/users' adresinde Users açılır */}
        <Route path="users" element={<Users />} />
        {/* '/accounting' adresinde Accounting açılır */}
        <Route path="accounting" element={<Accounting />} />
      </Route>
      
      {/* Layout dışında kalan sayfalar (Login vb.) buraya gelir */}
      <Route path="/login" element={<div>Giriş Sayfası</div>} />
    </Routes>
  );
}

export default App;