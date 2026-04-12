import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import toast from 'react-hot-toast'; // Bildirim kütüphanesi

// Kendi axios instance'ımızı oluşturuyoruz
const api = axios.create({
  baseURL: 'https://localhost:7006/api', // Senin .NET API adresin buraya gelecek
  headers: {
    'Content-Type': 'application/json',
  },
});

// 🛂 1. KAPI: İSTEK GÖNDERİLİRKEN (Request Interceptor)
api.interceptors.request.use(
  (config) => {
    // Tarayıcı hafızasındaki token'ı alıyoruz
    const token = localStorage.getItem('token');
    
    // Eğer token varsa, her isteğin kafasına "Authorization" header'ını takıyoruz
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 🚪 2. KAPI: CEVAP GELİRKEN (Response Interceptor)
api.interceptors.response.use(
  (response) => response, // Her şey yolundaysa cevabı olduğu gibi geçir
  (error) => {
    // Eğer backend 401 (Yetkisiz) veya 403 (Yasak) dönerse
    if (error.response?.status === 401) {
      console.warn("Oturum süresi doldu veya yetkisiz erişim!");
      
      // Zustand store'daki logout fonksiyonunu çağırıp kullanıcıyı temizle
      const { logout } = useAuthStore.getState();
      logout();
      
      // Kullanıcıyı login sayfasına fırlat
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => {
    // 🟢 Başarılı işlemlerde (Ekleme/Silme/Güncelleme gibi) otomatik mesaj verelim
    // Eğer istek bir POST, PUT veya DELETE ise ve backend 'message' dönmüşse ekrana bas
    if (["post", "put", "delete"].includes(response.config.method || "") && response.data.message) {
      toast.success(response.data.message); 
    }
    return response;
  },
  (error) => {
    const message = error.response?.data?.message || "Bir hata oluştu aga!";
    
    if (error.response?.status === 401) {
      toast.error("Oturum süresi doldu, tekrar giriş yap!");
      const { logout } = useAuthStore.getState();
      logout();
    } else {
      // 🔴 HERHANGİ BİR HATA OLUNCA BURASI ÇALIŞIR
      toast.error(message); 
    }
    
    return Promise.reject(error);
  }
);

export default api;