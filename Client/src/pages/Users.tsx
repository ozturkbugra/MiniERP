import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect from '../components/Common/AppSelect'; // 🚀 Yeni bileşenimizi import ettik
import api from '../api/axiosInstance'; 
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// Backend'den dönen gerçek modele göre güncellendi
interface User {
  id: string;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  roles?: string[];
}

interface Role {
  id: string;
  name: string;
}

const Users = () => {
  const { hasPermission } = useAuthStore();
  const navigate = useNavigate();
  
  // State Tanımları
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<Role[]>([]); // 🚀 Rolleri tutacağımız state
  const [loading, setLoading] = useState<boolean>(true);
  const [showAddModal, setShowAddModal] = useState<boolean>(false);

  // Form State
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    roleName: "" // 🚀 AppSelect'ten gelecek rol adı
  });

  // 1. Kullanıcıları ve Rolleri Listele
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      // 🚀 Paralel istek: Hem kullanıcıları hem de seçim için rolleri çekiyoruz
      const [usersRes, rolesRes] = await Promise.all([
        api.get("/Auths/GetAll"),
        api.get("/Roles")
      ]);
      
      if (usersRes.data.isSuccess) setUsers(usersRes.data.data);
      if (rolesRes.data.isSuccess) setRoles(rolesRes.data.data);
      
    } catch (error) {
      console.error("Veriler çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Input Değişim Yöneticisi (Text inputlar için)
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  // 2. Yeni Kullanıcı Ekle (Register)
  const handleSave = async () => {
    try {
      const payload = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
        password: formData.password
        // Not: Eğer backend Register endpoint'inde roleName bekliyorsa buraya ekleyebilirsin:
        // roleName: formData.roleName 
      };

      await api.post("/Auths/Register", payload);
      
      // 💡 EĞER İSTERSEN: Register işlemi bittikten sonra seçili rolü atamak için 
      // ikinci bir istek (AssignRole) atabilirsin. Şimdilik sadece kaydediyoruz.

      setShowAddModal(false);
      fetchData(); // Tabloyu yenile
    } catch (error) {
      console.error("Kullanıcı eklenirken hata oluştu");
    }
  };

  // 3. Kullanıcı Sil (Delete)
  const handleDelete = async (id: string) => {
    if (window.confirm("Bu kullanıcıyı sistemden silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Auths/${id}`); 
        fetchData();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  // Yeni Kullanıcı Ekle Modalı Açıcı
  const openCreateModal = () => {
    setFormData({ firstName: "", lastName: "", email: "", password: "", roleName: "" });
    setShowAddModal(true);
  };

  // 🚀 AppSelect için rolleri uygun formata dönüştürüyoruz
  const roleOptions = roles.map(r => ({ value: r.name, label: r.name }));

  // Tablo Sütun Yapılandırması
  const columns = [
    { 
      header: "KULLANICI", 
      accessor: (u: User) => (
        <div className="d-flex align-items-center">
          <div className="bg-primary text-white rounded-circle d-flex justify-content-center align-items-center me-3 shadow-sm" style={{width: '40px', height: '40px', fontSize: '14px', fontWeight: 'bold'}}>
            {(u.firstName?.charAt(0) || '')}{(u.lastName?.charAt(0) || '')}
          </div>
          <div>
            <div className="fw-bold mb-0">{u.firstName} {u.lastName}</div>
            <div className="small text-muted" style={{ fontSize: '11px' }}>@{u.userName}</div>
          </div>
        </div>
      ) 
    },
    { header: "E-POSTA ADRESİ", accessor: "email" as keyof User },
    { 
      header: "ROLLÜ", 
      accessor: (u: User) => {
        if (u.roles && u.roles.length > 0) {
          return (
            <div className="d-flex flex-wrap gap-1">
              {u.roles.map(role => (
                <span key={role} className="badge bg-primary-subtle text-primary border border-primary-subtle px-2 py-1">
                  {role}
                </span>
              ))}
            </div>
          );
        }
        return <span className="text-muted small fst-italic">Detaydan kontrol edin</span>;
      } 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (u: User) => (
        <div className="d-flex justify-content-end gap-2">
          <button 
            className="btn btn-sm btn-outline-info border-0 p-2" 
            onClick={() => navigate(`/users/${u.id}/details`)}
            title="Kullanıcı Detayı ve Rol Atama"
          >
            <i className="bi bi-person-gear fs-6"></i>
          </button>

          {hasPermission(APP_PERMISSIONS.Users.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(u.id)} title="Kullanıcıyı Sil">
              <i className="bi bi-trash3 fs-6"></i>
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
          description="Sisteme giriş yetkisi olan personelleri buradan yönetebilirsiniz."
          columns={columns} 
          data={users}
          onAdd={hasPermission(APP_PERMISSIONS.Users.Create) ? openCreateModal : undefined}
          addText="Yeni Personel Ekle"
        />
      )}

      {/* Yeni Kullanıcı Kayıt (Register) Modalı */}
      <AppModal 
        show={showAddModal} 
        title="Yeni Personel Tanımla" 
        onClose={() => setShowAddModal(false)}
        onSave={handleSave}
      >
       <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label fw-semibold small">Ad</label>
            {/* bg-light silindi, autoComplete="off" eklendi */}
            <input type="text" name="firstName" value={formData.firstName} onChange={handleInputChange} className="form-control form-control-sm" placeholder="Örn: Ahmet" autoComplete="off" />
          </div>
          <div className="col-md-6">
            <label className="form-label fw-semibold small">Soyad</label>
            <input type="text" name="lastName" value={formData.lastName} onChange={handleInputChange} className="form-control form-control-sm" placeholder="Örn: Yılmaz" autoComplete="off" />
          </div>
          <div className="col-12">
            <label className="form-label fw-semibold small">E-Posta</label>
            <input type="email" name="email" value={formData.email} onChange={handleInputChange} className="form-control form-control-sm" placeholder="personel@minierp.com" autoComplete="off" />
          </div>
          <div className="col-md-6">
            <label className="form-label fw-semibold small">Geçici Şifre</label>
            {/* 🚀 KRİTİK NOKTA: autoComplete="new-password" tarayıcının şifre doldurmasını engeller */}
            <input type="password" name="password" value={formData.password} onChange={handleInputChange} className="form-control form-control-sm" placeholder="Şifre belirleyin" autoComplete="new-password" />
          </div>
          <div className="col-md-6">
            {/* 🚀 Native AppSelect Bileşenimiz */}
            <AppSelect 
              label="Sistem Rolü"
              options={roleOptions}
              value={formData.roleName}
              onChange={(val) => setFormData(prev => ({ ...prev, roleName: val }))}
              isSearchable={true} 
              placeholder="Bir rol seçin..."
            />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Users;