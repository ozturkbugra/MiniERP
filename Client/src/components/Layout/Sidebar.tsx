import React, { useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';

interface SidebarProps {
  onSidebarToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ onSidebarToggle }) => {
  // Zustand store'dan ihtiyacımız olan her şeyi çekiyoruz
  const { user, hasPermission, logout, fetchPermissions } = useAuthStore();

  // BİLEŞEN YÜKLENDİĞİNDE: API'den en güncel yetkileri çek
  useEffect(() => {
    if (user) {
      fetchPermissions();
    }
  }, [user, fetchPermissions]);

  return (
    <aside className="sidebar">
      <div className="sidebar-shell">
        {/* Mobil Kapatma Butonu */}
        <button className="sidebar-close" type="button" onClick={onSidebarToggle}>
          <i className="bi bi-x-lg"></i>
        </button>

        <nav className="sidebar-nav">
          <ul className="nav-menu">
            {/* Dashboard: Herkese açık */}
            <li className="nav-item">
              <NavLink to="/" className="nav-link">
                <span className="nav-icon"><i className="ph-light ph-squares-four"></i></span>
                <span className="nav-text">Dashboard</span>
              </NavLink>
            </li>

            {/* STOK VE ÜRÜN YÖNETİMİ */}
            {(hasPermission(APP_PERMISSIONS.Products.View) || hasPermission(APP_PERMISSIONS.Stocks.View)) && (
              <>
                <li className="nav-heading"><span>Stok Yönetimi</span></li>

                {hasPermission(APP_PERMISSIONS.Products.View) && (
                  <li className="nav-item">
                    <NavLink to="/products" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-package"></i></span>
                      <span className="nav-text">Ürünler</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Stocks.View) && (
                  <li className="nav-item">
                    <NavLink to="/stocks" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-stack"></i></span>
                      <span className="nav-text">Stoklar</span>
                    </NavLink>
                  </li>
                )}
              </>
            )}

            {/* FİNANS VE MUHASEBE */}
            {(hasPermission(APP_PERMISSIONS.Finance.View) || hasPermission(APP_PERMISSIONS.Invoices.View)) && (
              <>
                <li className="nav-heading"><span>Finans</span></li>

                {hasPermission(APP_PERMISSIONS.Finance.View) && (
                  <li className="nav-item">
                    <NavLink to="/finance" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-bank"></i></span>
                      <span className="nav-text">Finans İşlemleri</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Invoices.View) && (
                  <li className="nav-item">
                    <NavLink to="/invoices" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-receipt"></i></span>
                      <span className="nav-text">Faturalar</span>
                    </NavLink>
                  </li>
                )}
              </>
            )}

            {/* SİSTEM YÖNETİMİ */}
            {(hasPermission(APP_PERMISSIONS.Users.View) || hasPermission(APP_PERMISSIONS.Roles.View)) && (
              <>
                <li className="nav-heading"><span>Sistem</span></li>

                {/* Kullanıcı Yönetimi */}
                {hasPermission(APP_PERMISSIONS.Users.View) && (
                  <li className="nav-item">
                    <NavLink to="/users" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-users-three"></i></span>
                      <span className="nav-text">Kullanıcılar</span>
                    </NavLink>
                  </li>
                )}

                {/* Rol ve Yetki Yönetimi */}
                {hasPermission(APP_PERMISSIONS.Roles.View) && (
                  <li className="nav-item">
                    <NavLink to="/roles" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-keyhole"></i></span>
                      <span className="nav-text">Roller</span>
                    </NavLink>
                  </li>
                )}
              </>
            )}
          </ul>
        </nav>

        {/* Sidebar Footer: Kullanıcı Bilgileri */}
        <div className="sidebar-footer">
          <div className="sidebar-account">
            <NavLink to="/profile" className="sidebar-account-main">
              <img
                src="/assets/img/profile-img.webp"
                alt="User"
                className="sidebar-account-avatar"
              />
              <div className="sidebar-account-meta">
                <div className="sidebar-account-name">{user?.userName || 'Misafir'}</div>
                <div className="sidebar-account-role">{user?.role || 'Yetkisiz'}</div>
              </div>
            </NavLink>
            <div className="sidebar-account-actions">
              <button
                onClick={logout}
                className="sidebar-account-action sidebar-account-logout"
                title="Logout"
                style={{ background: 'none', border: 'none' }}
              >
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