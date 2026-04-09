import React from 'react';

interface SidebarProps {
  onSidebarToggle: () => void;
}

const Sidebar: React.FC<SidebarProps> = ({ onSidebarToggle }) => {
  // Mevcut yolu alıyoruz (Örn: /users veya /accounting)
  const currentPath = window.location.pathname;

  // Linkin aktif olup olmadığını kontrol eden küçük bir yardımcı fonksiyon
  const isActive = (path: string) => currentPath === path ? 'active' : '';

  return (
    <aside className="sidebar">
      <div className="sidebar-shell">
        <button 
          className="sidebar-close" 
          type="button" 
          aria-label="Close sidebar" 
          onClick={onSidebarToggle}
        >
          <i className="bi bi-x-lg"></i>
        </button>

        <nav className="sidebar-nav">
          <ul className="nav-menu">

            <li className="nav-item">
              {/* isActive fonksiyonu ile path kontrolü yapıyoruz */}
              <a className={`nav-link ${isActive('/')}`} href="/">
                <span className="nav-icon"><i className="ph-light ph-squares-four"></i></span>
                <span className="nav-text">Dashboard</span>
                <span className="nav-meta">Home</span>
              </a>
            </li>

            <li className="nav-heading"><span>Yönetim</span></li>

            <li className="nav-item">
              <a className={`nav-link ${isActive('/users')}`} href="/users">
                <span className="nav-icon"><i className="ph-light ph-users-three"></i></span>
                <span className="nav-text">Kullanıcılar</span>
              </a>
            </li>

            <li className="nav-item">
              <a className={`nav-link ${isActive('/accounting')}`} href="/accounting">
                <span className="nav-icon"><i className="ph-light ph-receipt"></i></span>
                <span className="nav-text">Muhasebe</span>
              </a>
            </li>

          </ul>
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-account">
            <a href="/profile" className={`sidebar-account-main ${isActive('/profile')}`}>
              <img src="/assets/img/profile-img.webp" alt="User" className="sidebar-account-avatar" />
              <div className="sidebar-account-meta">
                <div className="sidebar-account-name">Buğra Öztürk</div>
                <div className="sidebar-account-role">Admin</div>
              </div>
            </a>
            <div className="sidebar-account-actions">
              <a href="/settings" className={`sidebar-account-action ${isActive('/settings')}`} title="Settings">
                <i className="bi bi-gear"></i>
              </a>
              <a href="/login" className="sidebar-account-action sidebar-account-logout" title="Logout">
                <i className="bi bi-box-arrow-right"></i>
              </a>
            </div>
          </div>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;