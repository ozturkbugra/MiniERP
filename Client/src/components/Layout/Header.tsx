import React, { useState, useEffect } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import { useSettingsStore } from '../../store/useSettingsStore';
import api from '../../api/axiosInstance';
import AppSelect from '../Common/AppSelect'; 

interface HeaderProps {
  onSidebarToggle: () => void;
  onSearchChange: (value: string) => void;
}

const Header: React.FC<HeaderProps> = ({ onSidebarToggle, onSearchChange }) => {
  const [isDarkMode, setIsDarkMode] = useState(() => localStorage.getItem('mini-erp-theme') === 'dark');
  const [isMobileSearchOpen, setIsMobileSearchOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [warehouses, setWarehouses] = useState<{ id: string, name: string }[]>([]);

  const { logout, user } = useAuthStore();
  const { defaultWarehouseId, setDefaultWarehouse } = useSettingsStore();

  const toggleTheme = () => setIsDarkMode(!isDarkMode);

  const handleLogout = async (e: React.MouseEvent) => {
    e.preventDefault();
    if (window.confirm("Çıkış yapmak istediğine emin misin aga?")) {
      await logout();
    }
  };

  useEffect(() => {
    api.get("/Warehouses").then(res => {
      if (res.data.isSuccess) {
        setWarehouses(res.data.data);
        if (!defaultWarehouseId && res.data.data.length > 0) {
          setDefaultWarehouse(res.data.data[0].id);
        }
      }
    });
  }, []);

  useEffect(() => {
    const html = document.documentElement;
    const theme = isDarkMode ? 'dark' : 'light';
    html.setAttribute('data-theme', theme);
    html.setAttribute('data-bs-theme', theme);
    localStorage.setItem('mini-erp-theme', theme);
  }, [isDarkMode]);

  const handleSearchInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    onSearchChange(e.target.value);
  };

  // AppSelect için veriyi formatla
  const warehouseOptions = warehouses.map(w => ({ label: w.name, value: w.id }));

  return (
    <>
      <header className="header">
        <div className="header-left">
          <button className="sidebar-toggle" title="Menüyü Aç/Kapat" onClick={onSidebarToggle}>
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
          <form className="search-form" onSubmit={(e) => e.preventDefault()}>
            <i className="bi bi-search search-icon"></i>
            <input 
              type="search" 
              placeholder="Menüde ara..." 
              onChange={handleSearchInput}
            />
            <kbd className="search-shortcut">/</kbd>
          </form>
        </div>

        <div className="header-right">
          <div className="header-actions-desktop">
            
            {/* 🚀 MASAÜSTÜ ARAMALI DEPO SEÇİCİ */}
            <div className="header-action-wrap d-none d-xl-flex align-items-center me-3" style={{ zIndex: 1050 }}>
               {/* AppSelect'in mb-3 class'ını Header içinde sıfırlamak için wrapper kullandık */}
               <div className="warehouse-select-container" style={{ width: '220px', marginTop: '16px' }}>
                  <AppSelect 
                    options={warehouseOptions}
                    value={defaultWarehouseId}
                    onChange={(val) => setDefaultWarehouse(val)}
                    isSearchable={true}
                    placeholder="Depo Seçiniz..."
                  />
               </div>
            </div>

            <div className="header-action-wrap dropdown notification-dropdown">
              <button className="header-action dropdown-toggle" data-bs-toggle="dropdown">
                <i className="bi bi-bell"></i>
                <span className="header-badge">4</span>
              </button>
              <div className="dropdown-menu dropdown-menu-end notification-menu">
                <div className="notification-header">
                  <div><h6>Bildirimler</h6><span>4 okunmamış</span></div>
                  <a href="#" data-notification-action="mark-all-read">Tümünü okundu işaretle</a>
                </div>
                <div className="notification-list">
                   <div className="notification-item unread">
                    <span className="notification-dot"></span>
                    <div className="notification-icon info"><i className="bi bi-rocket-takeoff"></i></div>
                    <div className="notification-content">
                      <div className="notification-title">Yayına hazır</div>
                      <div className="notification-text">Sprint sürümü QA testinden geçti.</div>
                      <span className="notification-time">5dk önce</span>
                    </div>
                  </div>
                </div>
                <div className="notification-footer"><a href="#">Bildirim merkezini aç <i className="bi bi-arrow-right"></i></a></div>
              </div>
            </div>

            <button className="header-action theme-toggle" title="Temayı Değiştir" onClick={toggleTheme}>
              <i className={isDarkMode ? "ph-light ph-sun" : "ph-light ph-moon-stars"}></i>
            </button>

            <div className="header-action-wrap dropdown user-dropdown">
              <button className="dropdown-toggle user-trigger" data-bs-toggle="dropdown">
                <img src={"/assets/img/profile-img.webp"} alt="User" className="user-avatar" />
                <div className="user-brief">
                  <span className="user-name">{user?.userName || 'Misafir'}</span>
                  <span className="user-role">{user?.role || 'Üye'}</span>
                </div>
              </button>
              
              <div className="dropdown-menu dropdown-menu-end user-menu">
                <div className="user-menu-header">
                  <img src={"/assets/img/profile-img.webp"} alt="User" className="user-menu-avatar" />
                  <div className="user-menu-info">
                    <div className="user-menu-name">{user?.userName}</div>
                    <div className="user-menu-email">{user?.role} Yetkisi</div>
                  </div>
                </div>
                <div className="user-menu-body">
                  <a className="user-menu-item" href="/profile"><i className="bi bi-person"></i><span>Profilim</span></a>
                  <a className="user-menu-item" href="/settings"><i className="bi bi-sliders"></i><span>Ayarlar</span></a>
                </div>
                <div className="user-menu-footer">
                  <button className="user-menu-logout" onClick={handleLogout} style={{ background: 'none', border: 'none', width: '100%', textAlign: 'left', cursor: 'pointer' }}>
                    <i className="bi bi-box-arrow-right"></i><span>Çıkış Yap</span>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div className="header-actions-mobile">
            <button className="header-action search-toggle" onClick={() => setIsMobileSearchOpen(!isMobileSearchOpen)}>
              <i className="bi bi-search"></i>
            </button>
            <button className="header-action mobile-menu-toggle" onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}>
              <i className="bi bi-three-dots"></i>
            </button>
          </div>
        </div>
      </header>

      {/* MOBİL ARAMA */}
      <div className={`mobile-search ${isMobileSearchOpen ? 'active' : ''}`}>
        <form className="search-form" onSubmit={(e) => e.preventDefault()}>
          <input 
            type="search" 
            placeholder="Menüde ara..." 
            onChange={handleSearchInput}
          />
          <button type="submit"><i className="bi bi-search"></i></button>
        </form>
      </div>

      {/* MOBİL HEADER MENÜ */}
      <div className={`mobile-header-menu ${isMobileMenuOpen ? 'active' : ''}`}>
        <div className="mobile-header-menu-content">
          
          {/* MOBİL İÇİN NATIVE SELECT (Parmakla kullanım için daha iyi) */}
          <div className="mobile-menu-item">
             <i className="ph-light ph-buildings"></i>
             <select 
                className="form-select form-select-sm border-0 bg-transparent text-light p-0 ms-2"
                value={defaultWarehouseId || ""}
                onChange={(e) => setDefaultWarehouse(e.target.value)}
              >
                {warehouses.map(w => (
                  <option key={w.id} value={w.id} className="bg-dark text-light">{w.name}</option>
                ))}
              </select>
          </div>

          <button className="mobile-menu-item theme-toggle" onClick={toggleTheme}>
            <i className={isDarkMode ? "ph-light ph-sun" : "ph-light ph-moon-stars"}></i>
            <span className="mobile-menu-label">Tema</span>
          </button>
          <a href="/profile" className="mobile-menu-item">
            <i className="bi bi-person"></i>
            <span className="mobile-menu-label">Profil</span>
          </a>
          <button className="mobile-menu-item mobile-menu-item-danger" onClick={handleLogout} style={{ background: 'none', border: 'none', width: '100%', textAlign: 'left' }}>
            <i className="bi bi-box-arrow-right"></i>
            <span className="mobile-menu-label">Çıkış Yap</span>
          </button>
        </div>
      </div>

    
    </>
  );
};

export default Header;