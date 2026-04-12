import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance'; 
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 1. Tip Tanımlaması: Backend'den (C#) dönen UserDto ile birebir uyumlu
interface User {
  id: string; // Genellikle Identity'de GUID string olur
  userName: string;
  email: string;
  role: string;
  status: string;
  joinedDate: string;
}

const Users = () => {
  const { hasPermission } = useAuthStore();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showAddModal, setShowAddModal] = useState<boolean>(false);

  // 2. API Veri Çekme Fonksiyonu
  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      // Belirttiğin /Auths/GetAll endpoint'ine vuruyoruz
      const response = await api.get("/Auths/GetAll"); 
      
      if (response.data.isSuccess) {
        setUsers(response.data.data);
      }
    } catch (error) {
      console.error("Kullanıcı listesi çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  // 3. Tablo Sütun Yapılandırması
  const columns = [
    { 
      header: "KULLANICI", 
      accessor: (u: User) => (
        <div className="d-flex align-items-center">
          <img 
            src="/assets/img/profile-img.webp" 
            alt="User" 
            className="rounded-circle me-3" 
            width="36" 
            height="36" 
          />
          <div>
            <div className="fw-bold text-light mb-0">{u.userName}</div>
            <div className="text-muted small" style={{ fontSize: '11px' }}>ID: {u.id.substring(0,8)}...</div>
          </div>
        </div>
      ) 
    },
    { header: "E-POSTA ADRESİ", accessor: "email" as keyof User },
    { 
      header: "YETKİ ROLÜ", 
      accessor: (u: User) => (
        <span className="badge bg-primary-subtle text-primary border border-primary-subtle px-3 py-2">
          {u.role || 'User'}
        </span>
      ) 
    },
    { 
      header: "DURUM", 
      accessor: (u: User) => (
        <span className={`badge px-3 py-2 ${u.status === 'Active' ? 'bg-success-subtle text-success' : 'bg-warning-subtle text-warning'}`}>
          <i className={`bi bi-circle-fill me-1`} style={{ fontSize: '8px' }}></i>
          {u.status || 'Active'}
        </span>
      ) 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (u: User) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Users.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" title="Düzenle">
              <i className="bi bi-pencil-square"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Users.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" title="Kullanıcıyı Sil">
              <i className="bi bi-trash3"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-users animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Yükleniyor...</span>
          </div>
        </div>
      ) : (
        <DataTable<User> 
          title="Kullanıcı Yönetimi" 
          description="Sistem erişim yetkilerini ve kullanıcı profillerini buradan merkezi olarak yönetin."
          columns={columns} 
          data={users}
          onAdd={hasPermission(APP_PERMISSIONS.Users.Create) ? () => setShowAddModal(true) : undefined}
          addText="Yeni Kullanıcı Tanımla"
        />
      )}

      {/* Yeni Kullanıcı Modalı */}
      <AppModal 
        show={showAddModal} 
        title="Yeni Kullanıcı Ekle" 
        onClose={() => setShowAddModal(false)}
        onSave={() => {
          // Buraya ileride POST isteği gelecek aga
          setShowAddModal(false);
        }}
      >
        <div className="row g-3">
          <div className="col-12">
            <label className="form-label fw-semibold small">Kullanıcı Adı</label>
            <input type="text" className="form-control form-control-sm bg-light" placeholder="Kullanıcı adı giriniz" />
          </div>
          <div className="col-12">
            <label className="form-label fw-semibold small">E-Posta</label>
            <input type="email" className="form-control form-control-sm bg-light" placeholder="ornek@minierp.com" />
          </div>
          <div className="col-12">
            <label className="form-label fw-semibold small">Rol Seçimi</label>
            <select className="form-select form-select-sm bg-light">
              <option value="Admin">Admin</option>
              <option value="Editor">Editor</option>
              <option value="User">User</option>
            </select>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Users;