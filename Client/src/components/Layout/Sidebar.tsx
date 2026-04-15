import React, { useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';

interface SidebarProps {
  onSidebarToggle: () => void;
  searchTerm?: string; // 🚀 YENİ
}

// 🚀 YENİ: Türkçe karakterleri ingilizceye çevirip küçük harf yapan sihirbaz
const normalizeString = (str: string) => {
  return str.toLocaleLowerCase('tr-TR')
    .replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's')
    .replace(/ı/g, 'i').replace(/ö/g, 'o').replace(/ç/g, 'c');
};

const Sidebar: React.FC<SidebarProps> = ({ onSidebarToggle, searchTerm = "" }) => {
  const { user, hasPermission, logout, fetchPermissions } = useAuthStore();

  useEffect(() => {
    if (user) {
      fetchPermissions();
    }
  }, [user, fetchPermissions]);

  // 🚀 YENİ: Eşleşme kontrol fonksiyonu
  const isMatch = (menuText: string) => {
    if (!searchTerm) return true;
    return normalizeString(menuText).includes(normalizeString(searchTerm));
  };

  return (
    <aside className="sidebar">
      <div className="sidebar-shell">
        <button className="sidebar-close" type="button" onClick={onSidebarToggle}>
          <i className="bi bi-x-lg"></i>
        </button>

        <nav className="sidebar-nav">
          <ul className="nav-menu">

            {/* Dashboard */}
            {isMatch("Dashboard") && (
              <li className="nav-item">
                <NavLink to="/" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-squares-four"></i></span>
                  <span className="nav-text">Dashboard</span>
                </NavLink>
              </li>
            )}

            {/* Depolar Yetkisi */}
            {hasPermission(APP_PERMISSIONS.Warehouses?.View) && isMatch("Depolar") && (
              <li className="nav-item">
                <NavLink to="/warehouses" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-buildings"></i></span>
                  <span className="nav-text">Depolar</span>
                </NavLink>
              </li>
            )}


            {/* Bankalar Yetkisi */}
            {hasPermission(APP_PERMISSIONS.Banks?.View) && isMatch("Bankalar") && (
              <li className="nav-item">
                <NavLink to="/banks" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-bank"></i></span>
                  <span className="nav-text">Bankalar</span>
                </NavLink>
              </li>
            )}

            {hasPermission(APP_PERMISSIONS.Cashes?.View) && isMatch("Kasalar") && (
              <li className="nav-item">
                <NavLink to="/cashes" className="nav-link">
                  <span className="nav-icon"><i className="ph-light ph-money"></i></span>
                  <span className="nav-text">Kasalar</span>
                </NavLink>
              </li>
            )}

            {hasPermission(APP_PERMISSIONS.Customers.View) && isMatch("Cariler") && (
  <li className="nav-item">
    <NavLink to="/customers" className="nav-link">
      <span className="nav-icon"><i className="ph-light ph-users"></i></span>
      <span className="nav-text">Cari Kartlar</span>
    </NavLink>
  </li>
)}

            {/* TANIMLAMALAR */}
            {(hasPermission(APP_PERMISSIONS.Categories.View) || hasPermission(APP_PERMISSIONS.Brands?.View) || hasPermission(APP_PERMISSIONS.Units?.View)) && (
              <>
                {/* Altındaki elemanlardan en az biri aramayla eşleşiyorsa Başlığı göster */}
                {(isMatch("Kategoriler") || isMatch("Markalar") || isMatch("Birimler")) && (
                  <li className="nav-heading"><span>Tanımlamalar</span></li>
                )}

                {hasPermission(APP_PERMISSIONS.Categories.View) && isMatch("Kategoriler") && (
                  <li className="nav-item">
                    <NavLink to="/categories" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-tag"></i></span>
                      <span className="nav-text">Kategoriler</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Brands?.View) && isMatch("Markalar") && (
                  <li className="nav-item">
                    <NavLink to="/brands" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-bookmarks"></i></span>
                      <span className="nav-text">Markalar</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Units?.View) && isMatch("Birimler") && (
                  <li className="nav-item">
                    <NavLink to="/units" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-scales"></i></span>
                      <span className="nav-text">Birimler</span>
                    </NavLink>
                  </li>
                )}
              </>
            )}

            {/* STOK VE ÜRÜN YÖNETİMİ */}
            {(hasPermission(APP_PERMISSIONS.Products.View) || hasPermission(APP_PERMISSIONS.Stocks.View)) && (
              <>
                {(isMatch("Ürünler") || isMatch("Stoklar")) && (
                  <li className="nav-heading"><span>Stok Yönetimi</span></li>
                )}

                {hasPermission(APP_PERMISSIONS.Products.View) && isMatch("Ürünler") && (
                  <li className="nav-item">
                    <NavLink to="/products" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-package"></i></span>
                      <span className="nav-text">Ürünler</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Stocks.View) && isMatch("Stoklar") && (
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
                {(isMatch("Finans İşlemleri") || isMatch("Faturalar")) && (
                  <li className="nav-heading"><span>Finans</span></li>
                )}

                {hasPermission(APP_PERMISSIONS.Finance.View) && isMatch("Finans İşlemleri") && (
                  <li className="nav-item">
                    <NavLink to="/finance" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-bank"></i></span>
                      <span className="nav-text">Finans İşlemleri</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Invoices.View) && isMatch("Faturalar") && (
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
                {(isMatch("Kullanıcılar") || isMatch("Roller")) && (
                  <li className="nav-heading"><span>Sistem</span></li>
                )}

                {hasPermission(APP_PERMISSIONS.Users.View) && isMatch("Kullanıcılar") && (
                  <li className="nav-item">
                    <NavLink to="/users" className="nav-link">
                      <span className="nav-icon"><i className="ph-light ph-users-three"></i></span>
                      <span className="nav-text">Kullanıcılar</span>
                    </NavLink>
                  </li>
                )}

                {hasPermission(APP_PERMISSIONS.Roles.View) && isMatch("Roller") && (
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