import axios from 'axios';
import { useAuthStore } from '../store/useAuthStore';
import toast from 'react-hot-toast'; // Bildirim kütüphanesi

// Kendi axios instance'ımızı oluşturuyoruz
const api = axios.create({
  baseURL: 'https://localhost:7006/api', // Senin .NET API adresin
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

// 🚪 2. KAPI: CEVAP GELİRKEN (Response Interceptor - TEK BLOK)
api.interceptors.response.use(
  (response) => {
    // 🟢 BAŞARILI İŞLEMLER (POST, PUT, DELETE)
    if (["post", "put", "delete"].includes(response.config.method || "") && response.data?.message) {
      toast.success(response.data.message); 
    }
    return response;
  },
  (error) => {
    // 🔴 HATA YAKALAMA VE MESAJ ÇIKARTMA
    let errorMessage = "Bir hata oluştu aga!"; // Varsayılan mesaj
    
    if (error.response?.data) {
      const data = error.response.data;
      
      // 1. İhtimal: Backend'den (FluentValidation'dan) Errors dizisi geldiyse
      if (data.Errors && Array.isArray(data.Errors) && data.Errors.length > 0) {
        errorMessage = data.Errors.join('\n'); // Birden fazla hata varsa alt alta ekle
      } 
      // 2. İhtimal: Küçük harfle 'errors' geldiyse (.NET default davranışı bazen böyledir)
      else if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
        errorMessage = data.errors.join('\n');
      }
      // 3. İhtimal: Dizi yok ama tekil bir Message veya message varsa
      else if (data.Message || data.message) {
        errorMessage = data.Message || data.message;
      }
    }

    // ⛔ DURUM KODLARINA GÖRE AKSİYONLAR
    if (error.response?.status === 401) {
      // Token patlamış veya yok
      toast.error("Oturum süresi doldu, tekrar giriş yap!");
      const { logout } = useAuthStore.getState();
      logout();
      window.location.href = '/login';
    } 
    else if (error.response?.status === 403) {
      // Yetkisi olmayan bir yere tıklamaya çalıştı
      toast.error("Bu işlemi yapmaya yetkiniz yok!");
    }
    else {
      // 422, 400, 500 gibi diğer tüm hatalarda backend'in asıl mesajını göster
      toast.error(errorMessage); 
    }
    
    return Promise.reject(error);
  }
);

export default api;