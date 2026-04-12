// src/context/AuthContext.js
import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../services/api'; // Senin axios instance'ın

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [permissions, setPermissions] = useState([]);
    const [loading, setLoading] = useState(true);

    const fetchPermissions = async () => {
        try {
            const response = await api.get("/Identity/GetMyPermissions");
            if (response.data.isSuccess) {
                setPermissions(response.data.data); // Mediator'dan gelen string listesi
            }
        } catch (error) {
            console.error("Yetkiler alınamadı", error);
        } finally {
            setLoading(false);
        }
    };

    const hasPermission = (permissionName) => {
        // Eğer kullanıcı Admin ise her şeye yetkisi vardır (Opsiyonel)
        // if (userRole === "Admin") return true; 
        return permissions.includes(permissionName);
    };

    return (
        <AuthContext.Provider value={{ permissions, hasPermission, fetchPermissions, loading }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);