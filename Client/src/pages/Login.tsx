import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../api/axiosInstance'; //
import { useAuthStore } from '../store/useAuthStore'; //

const Login: React.FC = () => {
  // 📝 Form ve UI State'leri
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [remember, setRemember] = useState(false); // Beni Hatırla kontrolü
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  const { setUser } = useAuthStore(); //
  const navigate = useNavigate();

  // 🚀 Login İşlemi
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      // 1. API İsteği: .NET tarafındaki AuthsController/Login
      const response = await api.post('/Auths/Login', { email, password });
      
      // 2. Veriyi Parçalama (Backend'in Generic Response yapısına göre)
      const { isSuccess, data: token, message } = response.data; 

      if (isSuccess && token) {
        // 🗝️ 3. "Beni Hatırla" Mantığı:
        // İşaretliyse localStorage (kalıcı), değilse sessionStorage (sekme kapandığında silinir)
        if (remember) {
          localStorage.setItem('token', token);
          sessionStorage.removeItem('token'); // Çakışma önleyici temizlik
        } else {
          sessionStorage.setItem('token', token);
          localStorage.removeItem('token'); // Çakışma önleyici temizlik
        }
        
        // 4. Zustand Store Güncelleme:
        // Store içindeki setUser, bu token'ı otomatik decode edip kullanıcıyı tanıyacak.
        setUser(token); 

        // 5. Yönlendirme
        navigate('/');
      } else {
        // Backend'den gelen hata mesajı (Örn: "Şifre Yanlış")
        alert(message || "Giriş başarısız! Bilgilerini kontrol et aga.");
      }
    } catch (error: any) {
      // Sunucu hatası veya 401 gibi durumlarda
      const errorMessage = error.response?.data?.message || "Sunucuya bağlanılamadı. Backend'in çalıştığından emin ol!";
      alert(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fauth">
      <main className="fauth-main">
        <div className="fauth-main-inner">
          <Link to="/" className="fauth-logo fauth-logo-center">
            <img src="/assets/img/logo.webp" alt="NiceAdmin" />
            <span>MiniERP</span>
          </Link>

          <div className="fauth-card">
            <div className="fauth-card-head">
              <span className="fauth-kicker"><i className="bi bi-shield-check"></i> Güvenli Erişim</span>
              <h1 className="fauth-title">Tekrar Hoş Geldin</h1>
              <p className="fauth-subtitle">Devam etmek için hesabınla giriş yap aga.</p>
            </div>

            <form className="fauth-form" onSubmit={handleSubmit}>
              {/* E-posta Alanı */}
              <div className="fauth-field">
                <label htmlFor="email" className="form-label">E-posta Adresi</label>
                <input 
                  type="email" 
                  className="form-control" 
                  id="email" 
                  placeholder="isim@ornek.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required 
                />
              </div>

              {/* Şifre Alanı */}
              <div className="fauth-field">
                <div className="fauth-row-between">
                  <label htmlFor="password" className="form-label">Şifre</label>
                  <Link to="/forgot-password" title="Şifremi Unuttum" className="fauth-link">Şifremi Unuttum?</Link>
                </div>
                <div className="input-group">
                  <input 
                    type={showPassword ? "text" : "password"} 
                    className="form-control" 
                    id="password" 
                    placeholder="Şifreni gir"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required 
                  />
                  <button 
                    className="btn btn-outline-secondary" 
                    type="button" 
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    <i className={showPassword ? "bi bi-eye-slash" : "bi bi-eye"}></i>
                  </button>
                </div>
              </div>

              {/* Beni Hatırla Checkbox */}
              <div className="fauth-row-between mb-3">
                <div className="form-check mb-0">
                  <input 
                    className="form-check-input" 
                    type="checkbox" 
                    id="remember" 
                    checked={remember}
                    onChange={(e) => setRemember(e.target.checked)}
                  />
                  <label className="form-check-label" htmlFor="remember">Beni Hatırla</label>
                </div>
              </div>

              {/* Giriş Butonu */}
              <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Giriş Yapılıyor...
                  </>
                ) : 'Giriş Yap'}
              </button>

            
            </form>

          </div>

          <footer className="footer-centered">
            <div className="footer-copyright">&copy; 2026 <a href="#">MiniERP</a>. Tüm Hakları Saklıdır.</div>
            <div className="footer-links"><a href="#">Gizlilik</a><a href="#">Şartlar</a><a href="#">Yardım</a></div>
          </footer>
        </div>
      </main>
    </div>
  );
};

export default Login;