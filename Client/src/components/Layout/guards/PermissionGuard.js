// src/components/guards/PermissionGuard.js
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const PermissionGuard = ({ children, requiredPermission }) => {
    const { hasPermission, loading } = useAuth();

    if (loading) return <div>Yükleniyor...</div>;

    if (!hasPermission(requiredPermission)) {
        return <Navigate to="/403" replace />; // Yetkisiz erişim sayfası
    }

    return children;
};

export default PermissionGuard;