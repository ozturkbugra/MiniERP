import React, { useState, useEffect } from 'react';

interface HeaderProps {
  onSidebarToggle: () => void;
}

const Header: React.FC<HeaderProps> = ({ onSidebarToggle }) => {
  // State'ler: Tema, Mobil Arama ve Mobil Menü kontrolü
  const [isDarkMode, setIsDarkMode] = useState(() => localStorage.getItem('mini-erp-theme') === 'dark');
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const toggleTheme = () => setIsDarkMode(!isDarkMode);

  useEffect(() => {
    const html = document.documentElement;
    const theme = isDarkMode ? 'dark' : 'light';
    html.setAttribute('data-theme', theme);
    html.setAttribute('data-bs-theme', theme);
    localStorage.setItem('mini-erp-theme', theme);
  }, [isDarkMode]);

  return (
    <>
      <header className="header">
        <div className="header-left">
          <button className="sidebar-toggle" title="Toggle Sidebar" onClick={onSidebarToggle}>
            <span className="menu-lines"><span></span><span></span><span></span></span>
          </button>

          <a href="/" className="header-brand">
            <span className="header-logo"><img src="/assets/img/logo.webp" alt="Logo" /></span>
            <span className="header-context">
              <strong className="header-context-title">MiniERP</strong>
            </span>
          </a>
        </div>

        {/* Masaüstü Arama */}
        <div className="header-search-wrap">
          <form className="search-form">
            <i className="bi bi-search search-icon"></i>
            <input type="search" placeholder="Search projects, invoices, users..." />
            <kbd className="search-shortcut">/</kbd>
          </form>
        </div>

        <div className="header-right">
          <div className="header-actions-desktop">
            {/* Bildirimler, Mesajlar vb. Dropdownlar (Bootstrap JS ile çalışır) */}
            <div className="header-action-wrap dropdown notification-dropdown">
              <button className="header-action dropdown-toggle" data-bs-toggle="dropdown">
                <i className="bi bi-bell"></i>
                <span className="header-badge">4</span>
              </button>
              {/* Dropdown içeriği buraya gelecek... */}
            </div>

            {/* Tema Butonu */}
            <button className="header-action theme-toggle" title="Toggle Theme" onClick={toggleTheme}>
              <i className={isDarkMode ? "ph-light ph-sun" : "ph-light ph-moon-stars"}></i>
            </button>

            {/* Kullanıcı Profili */}
            <div className="header-action-wrap dropdown user-dropdown">
              <button className="dropdown-toggle user-trigger" data-bs-toggle="dropdown">
                <img src="/assets/img/profile-img.webp" alt="User" className="user-avatar" />
                <div className="user-brief">
                  <span className="user-name">Buğra Öztürk</span>
                  <span className="user-role">Developer</span>
                </div>
              </button>
              <div className="dropdown-menu dropdown-menu-end user-menu">
                {/* Profil menü linkleri... */}
              </div>
            </div>
          </div>

          {/* MOBİL AKSİYONLAR - Arama ve More butonu */}
          <div className="header-actions-mobile">
            <button 
              className="header-action search-toggle" 
              onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}
            >
              <i className="bi bi-search"></i>
            </button>
            <button 
              className="header-action mobile-menu-toggle" 
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            >
              <i className="bi bi-three-dots"></i>
            </button>
          </div>
        </div>
      </header>

      {/* MOBİL ARAMA KATMANI - State'e göre açılır */}
      <div className={`mobile-search ${isMobileSearchOpen ? 'active' : ''}`}>
        <form className="search-form">
          <input type="search" placeholder="Search..." />
          <button type="submit"><i className="bi bi-search"></i></button>
        </form>
      </div>

      {/* MOBİL HEADER MENÜ - State'e göre açılır */}
      <div className={`mobile-header-menu ${isMobileMenuOpen ? 'active' : ''}`}>
        <div className="mobile-header-menu-content">
          <button className="mobile-menu-item theme-toggle" onClick={toggleTheme}>
            <i className={isDarkMode ? "ph-light ph-sun" : "ph-light ph-moon-stars"}></i>
            <span className="mobile-menu-label">Theme</span>
          </button>
          <a href="/profile" className="mobile-menu-item">
            <i className="bi bi-person"></i>
            <span className="mobile-menu-label">Profile</span>
          </a>
          <a href="/login" className="mobile-menu-item mobile-menu-item-danger">
            <i className="bi bi-box-arrow-right"></i>
            <span className="mobile-menu-label">Sign Out</span>
          </a>
        </div>
      </div>
    </>
  );
};

export default Header;