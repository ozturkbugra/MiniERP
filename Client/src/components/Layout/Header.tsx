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
            {/* Bildirimler (Dolduruldu) */}
            <div className="header-action-wrap dropdown notification-dropdown">
              <button className="header-action dropdown-toggle" data-bs-toggle="dropdown">
                <i className="bi bi-bell"></i>
                <span className="header-badge">4</span>
              </button>
              
              <div className="dropdown-menu dropdown-menu-end notification-menu">
                <div className="notification-header">
                  <div>
                    <h6>Notifications</h6>
                    <span>4 unread</span>
                  </div>
                  <a href="#" data-notification-action="mark-all-read">Mark all read</a>
                </div>
                <div className="notification-summary">
                  <a href="#" className="notification-summary-item">
                    <strong>7</strong>
                    <span>Today</span>
                  </a>
                  <a href="#" className="notification-summary-item">
                    <strong>23</strong>
                    <span>This week</span>
                  </a>
                  <a href="#" className="notification-summary-item">
                    <strong>3</strong>
                    <span>Approvals</span>
                  </a>
                </div>
                <div className="notification-list">
                  <div className="notification-item unread">
                    <span className="notification-dot"></span>
                    <div className="notification-icon info"><i className="bi bi-rocket-takeoff"></i></div>
                    <div className="notification-content">
                      <div className="notification-title">Deploy ready</div>
                      <div className="notification-text">Sprint release passed QA validation.</div>
                      <span className="notification-time">5m ago</span>
                    </div>
                  </div>
                  <div className="notification-item unread">
                    <span className="notification-dot"></span>
                    <img src="/assets/img/avatars/avatar-2.webp" alt="" className="notification-avatar" />
                    <div className="notification-content">
                      <div className="notification-title">Mia sent feedback</div>
                      <div className="notification-text">Please review updated dashboard spacing.</div>
                      <span className="notification-time">21m ago</span>
                    </div>
                  </div>
                </div>
                <div className="notification-footer">
                  <a href="#">Open notification center <i className="bi bi-arrow-right"></i></a>
                </div>
              </div>
            </div>

            {/* Tema Butonu */}
            <button className="header-action theme-toggle" title="Toggle Theme" onClick={toggleTheme}>
              <i className={isDarkMode ? "ph-light ph-sun" : "ph-light ph-moon-stars"}></i>
            </button>

            {/* Kullanıcı Profili (Dolduruldu) */}
            <div className="header-action-wrap dropdown user-dropdown">
              <button className="dropdown-toggle user-trigger" data-bs-toggle="dropdown">
                <img src="/assets/img/profile-img.webp" alt="User" className="user-avatar" />
                <div className="user-brief">
                  <span className="user-name">Buğra Öztürk</span>
                  <span className="user-role">Developer</span>
                </div>
              </button>
              
              <div className="dropdown-menu dropdown-menu-end user-menu">
                <div className="user-menu-header">
                  <img src="/assets/img/profile-img.webp" alt="User" className="user-menu-avatar" />
                  <div className="user-menu-info">
                    <div className="user-menu-name">Buğra Öztürk</div>
                    <div className="user-menu-email">admin@bugraozturk.com.tr</div>
                  </div>
                </div>
                <div className="user-menu-body">
                  <a className="user-menu-item" href="/profile">
                    <i className="bi bi-person"></i>
                    <span>My Profile</span>
                  </a>
                  <a className="user-menu-item" href="/settings">
                    <i className="bi bi-sliders"></i>
                    <span>Preferences</span>
                  </a>
                  <a className="user-menu-item" href="/activity">
                    <i className="bi bi-activity"></i>
                    <span>Activity Log</span>
                  </a>
                </div>
                <div className="user-menu-footer">
                  <a className="user-menu-logout" href="/login">
                    <i className="bi bi-box-arrow-right"></i>
                    <span>Sign Out</span>
                  </a>
                </div>
              </div>
            </div>
          </div>

          {/* MOBİL AKSİYONLAR */}
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

      {/* MOBİL ARAMA KATMANI */}
      <div className={`mobile-search ${isMobileSearchOpen ? 'active' : ''}`}>
        <form className="search-form">
          <input type="search" placeholder="Search..." />
          <button type="submit"><i className="bi bi-search"></i></button>
        </form>
      </div>

      {/* MOBİL HEADER MENÜ */}
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