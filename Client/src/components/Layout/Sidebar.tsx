import React, { useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { MENU_ITEMS } from '../../config/sidebarMenu';

interface SidebarProps {
  onSidebarToggle: () => void;
  searchTerm?: string;
}

const normalizeString = (str: string) => {
  return str.toLocaleLowerCase('tr-TR')
    .replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's')
    .replace(/ı/g, 'i').replace(/ö/g, 'o').replace(/ç/g, 'c');
};

const Sidebar: React.FC<SidebarProps> = ({ onSidebarToggle, searchTerm = "" }) => {
  const { user, hasPermission, logout, fetchPermissions } = useAuthStore();

  useEffect(() => {
    if (user) fetchPermissions();
  }, [user, fetchPermissions]);

  const isMatch = (text: string) => {
    if (!searchTerm) return true;
    return normalizeString(text).includes(normalizeString(searchTerm));
  };

  return (
    <aside className="sidebar">
      <div className="sidebar-shell">
        <button className="sidebar-close" onClick={onSidebarToggle}><i className="bi bi-x-lg"></i></button>

        <nav className="sidebar-nav">
          <ul className="nav-menu">
            {MENU_ITEMS.map((section, idx) => {
              // 1. Eğer bir başlık (Header) ise ve çocukları varsa
              if (section.isHeader && section.children) {
                // Sadece yetkisi olan ve arama kriterine uyan çocukları filtrele
                const visibleChildren = section.children.filter(child => 
                  hasPermission(child.permission) && isMatch(child.title)
                );

                // Eğer hiç görünür çocuk yoksa başlığı komple gizle (Mükemmel Mantık!)
                if (visibleChildren.length === 0) return null;

                return (
                  <React.Fragment key={idx}>
                    <li className="nav-heading"><span>{section.title}</span></li>
                    {visibleChildren.map((child, cIdx) => (
                      <li className="nav-item" key={cIdx}>
                        <NavLink to={child.path} className="nav-link">
                          <span className="nav-icon"><i className={`ph-light ${child.icon}`}></i></span>
                          <span className="nav-text">{child.title}</span>
                        </NavLink>
                      </li>
                    ))}
                  </React.Fragment>
                );
              }

              // 2. Eğer bağımsız bir link ise (Örn: Dashboard)
              if (!section.isHeader && isMatch(section.title)) {
                return (
                  <li className="nav-item" key={idx}>
                    <NavLink to={section.path!} className="nav-link">
                      <span className="nav-icon"><i className={`ph-light ${section.icon}`}></i></span>
                      <span className="nav-text">{section.title}</span>
                    </NavLink>
                  </li>
                );
              }

              return null;
            })}
          </ul>
        </nav>

        {/* Footer kısmı aynı kalıyor... */}
        <div className="sidebar-footer">
            {/* Kullanıcı account bilgileri buraya gelecek */}
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;