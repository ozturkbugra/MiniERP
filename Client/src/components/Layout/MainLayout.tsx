import React, { useState, useEffect } from 'react';
import Header from './Header';
import Sidebar from './Sidebar';
import Footer from './Footer';

const MainLayout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(() => {
    const saved = localStorage.getItem('mini-erp-sidebar');
    return saved !== null ? JSON.parse(saved) : true;
  });

  const toggleSidebar = () => setIsSidebarOpen(!isSidebarOpen);

  useEffect(() => {
    localStorage.setItem('mini-erp-sidebar', JSON.stringify(isSidebarOpen));
    const body = document.body;
    const isMobile = window.innerWidth <= 991;

    // Senin o meşhur class mantığın
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
      <Header onSidebarToggle={toggleSidebar} />
      <Sidebar onSidebarToggle={toggleSidebar} />
      
      {/* Mobilde sidebar açıkken arkaya basınca kapansın diye overlay */}
      <div 
        className={`sidebar-overlay ${isSidebarOpen ? 'active' : ''}`} 
        onClick={toggleSidebar}
      ></div>

      <main className="main">
        <div className="main-content">
          {children} 
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