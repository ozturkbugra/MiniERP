import React from 'react';
import { NavLink } from 'react-router-dom'; // SPA davranışı için NavLink kullanıyoruz
import { useAuthStore } from '../../store/useAuthStore'; // Az önce oluşturduğumuz beyin

interface SidebarProps {
  onSidebarToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ onSidebarToggle }) => {
  // Store'dan kullanıcı bilgilerini ve yetki kontrol fonksiyonunu çekiyoruz
  const { user, hasPermission, logout } = useAuthStore();

  return (
    <aside className="sidebar">
      <div className="sidebar-shell">
        <button className="sidebar-close" type="button" onClick={onSidebarToggle}>
          <i className="bi bi-x-lg"></i>
        </button>

        <nav className="sidebar-nav">
          <ul className="nav-menu">
            {/* Dashboard genelde herkese açıktır ama istersen ona da yetki koyarız */}
            <li className="nav-item">
              <NavLink to="/" className="nav-link">
                <span className="nav-icon"><i className="ph-light ph-squares-four"></i></span>
                <span className="nav-text">Dashboard</span>
              </NavLink>
            </li>

            <li className="nav-heading"><span>Yönetim</span></li>

            {/* KULLANICI YÖNETİMİ: Sadece yetkisi olan görsün */}
            {hasPermission('AppPermissions.Users.View') && (
              <li className="nav-item">
                <NavLink to="/users" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-users-three"></i></span>
                  <span className="nav-text">Kullanıcılar</span>
                </NavLink>
              </li>
            )}

            {/* MUHASEBE: Sadece yetkisi olan görsün */}
            {hasPermission('AppPermissions.Accounting.View') && (
              <li className="nav-item">
                <NavLink to="/accounting" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-receipt"></i></span>
                  <span className="nav-text">Muhasebe</span>
                </NavLink>
              </li>
            )}
          </ul>
        </nav>

        {/* Sidebar Footer - Dinamik Kullanıcı Bilgileri */}
        <div className="sidebar-footer">
          <div className="sidebar-account">
            <NavLink to="/profile" className="sidebar-account-main">
              {/* Kullanıcı fotoğrafı yoksa default avatar basıyoruz */}
              <img src={user?.userName || "/assets/img/profile-img.webp"} alt="User" className="sidebar-account-avatar" />
              <div className="sidebar-account-meta">
                {/* İsim artık store'dan geliyor */}
                <div className="sidebar-account-name">{user?.userName || 'Misafir'}</div>
                <div className="sidebar-account-role">{user?.role || 'Yetkisiz'}</div>
              </div>
            </NavLink>
            <div className="sidebar-account-actions">
              <NavLink to="/settings" className="sidebar-account-action" title="Settings">
                <i className="bi bi-gear"></i>
              </NavLink>
              {/* Çıkış fonksiyonunu bağladık */}
              <button onClick={logout} className="sidebar-account-action sidebar-account-logout" title="Logout" style={{background: 'none', border: 'none'}}>
                <i className="bi bi-box-arrow-right"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;