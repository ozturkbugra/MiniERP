import React, { useState, useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import Header from './Header';
import Sidebar from './Sidebar';

const MainLayout: React.FC = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(() => {
    const saved = localStorage.getItem('mini-erp-sidebar');
    return saved !== null ? JSON.parse(saved) : true;
  });

  // 🚀 YENİ: Arama state'ini burada tutuyoruz
  const [searchTerm, setSearchTerm] = useState("");

  const toggleSidebar = () => setIsSidebarOpen(!isSidebarOpen);

  useEffect(() => {
    localStorage.setItem('mini-erp-sidebar', JSON.stringify(isSidebarOpen));
    const body = document.body;
    const isMobile = window.innerWidth <= 991;

    if (isMobile) {
      if (isSidebarOpen) {
        body.classList.add('sidebar-open');
        body.classList.remove('sidebar-hidden');
      } else {
        body.classList.add('sidebar-hidden');
        body.classList.remove('sidebar-open');
      }
    } else {
      body.classList.remove('sidebar-open');
      if (!isSidebarOpen) {
        body.classList.add('sidebar-hidden');
      } else {
        body.classList.remove('sidebar-hidden');
      }
    }
  }, [isSidebarOpen]);

  return (
    <>
      {/* 🚀 YENİ: onSearchChange prop'u eklendi */}
      <Header onSidebarToggle={toggleSidebar} onSearchChange={setSearchTerm} />
      
      {/* 🚀 YENİ: searchTerm prop'u eklendi */}
      <Sidebar onSidebarToggle={toggleSidebar} searchTerm={searchTerm} />
      
      <div 
        className={`sidebar-overlay ${isSidebarOpen ? 'active' : ''}`} 
        onClick={toggleSidebar}
      ></div>

      <main className="main">
        <div className="main-content">
          <Outlet /> 
        </div>
        <footer className="footer">
          <div className="footer-content">
            <div className="footer-credits">
              <div className="footer-copyright">&copy; 2026 <a href="/">MiniERP</a></div>
            </div>
          </div>
        </footer>
      </main>

      <a href="#" className="back-to-top" onClick={(e) => { e.preventDefault(); window.scrollTo({ top: 0, behavior: 'smooth' }); }}>
        <i className="bi bi-arrow-up"></i>
      </a>
    </>
  );
};

export default MainLayout;