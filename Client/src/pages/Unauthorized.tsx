import React from 'react';
import { Link } from 'react-router-dom';

const Unauthorized: React.FC = () => {
  return (
    <div className="na-error-page na-error-403">
      <div className="na-error-shell">
        <header className="na-error-head">
          {/* Logo ve İsim kısmını kendi projemize göre güncelledik */}
          <Link to="/" className="na-error-logo">
            <img src="/assets/img/logo.webp" alt="MiniERP" />
            <span>MiniERP</span>
          </Link>
          <span className="na-error-status">Hata 403</span>
        </header>

        <div className="na-error-layout">
          <div className="na-error-main">
            <span className="na-error-icon"><i className="bi bi-shield-lock"></i></span>
            <h2 className="na-error-title">Erişim Kısıtlandı</h2>
            <p className="na-error-text">
              Hesap yetkileriniz bu kaynağa erişmek için yeterli değil. 
              Lütfen daha yüksek yetkili bir hesapla giriş yapın veya sistem yöneticinizden yetki talep edin.
            </p>

            <div className="na-error-actions">
              <Link to="/login" className="btn btn-primary">
                <i className="bi bi-box-arrow-in-right me-1"></i> Giriş Yap
              </Link>
              <Link to="/" className="btn btn-outline-secondary">
                <i className="bi bi-house me-1"></i> Dashboard
              </Link>
            </div>
          </div>

          <aside className="na-error-side">
            <h6>Erişim Kontrol Listesi</h6>
            <div className="na-error-step"><i className="bi bi-check2"></i> Aktif çalışma alanını onayla</div>
            <div className="na-error-step"><i className="bi bi-check2"></i> Atanmış rolünü doğrula</div>
            <div className="na-error-step"><i className="bi bi-arrow-repeat"></i> Yetki güncellemesi talep et</div>
          </aside>
        </div>
      </div>
    </div>
  );
};

export default Unauthorized;