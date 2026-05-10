import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';

interface Role {
  id: string;
  name: string;
}

interface UserDetail {
  id: string;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  roles: string[];
}

const UserDetails = () => {
  const { userId } = useParams();
  const navigate = useNavigate();
  const { hasPermission } = useAuthStore();

  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState<UserDetail | null>(null);
  const [allRoles, setAllRoles] = useState<Role[]>([]);
  
  // 🚀 YENİ: Swagger'a göre artık tek bir rol (string) tutuyoruz
  const [selectedRole, setSelectedRole] = useState<string>("");

  // 1. Verileri Çekme
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [userRes, rolesRes] = await Promise.all([
        api.get(`/Auths/GetById/${userId}`),
        api.get("/Roles")
      ]);

      if (userRes.data.isSuccess) {
        setUser(userRes.data.data);
        // Kullanıcının mevcut rolü varsa ilkini seçili yapıyoruz
        if (userRes.data.data.roles && userRes.data.data.roles.length > 0) {
          setSelectedRole(userRes.data.data.roles[0]); 
        }
      }
      if (rolesRes.data.isSuccess) {
        setAllRoles(rolesRes.data.data);
      }
    } catch (error) {
      console.error("Kullanıcı detayları yüklenemedi");
    } finally {
      setLoading(false);
    }
  }, [userId]);

  useEffect(() => { fetchData(); }, [fetchData]);

  // 🚀 YENİ: Rol Seçimini Tekli (Radio) Mantığına Çevirdik
  const handleSelectRole = (roleName: string) => {
    setSelectedRole(roleName);
  };

  // 2. Rol Atama İşlemi (Save)
  const handleSaveRoles = async () => {
    if (!selectedRole) return; 
    
    try {
      // 🚀 YENİ: Payload tam olarak Swagger'ın istediği formata geldi
      const payload = {
        userId: userId,
        roleName: selectedRole
      };
      
      // Endpoint'i Swagger'daki rotaya göre güncelledik
      await api.post("/Auths/AssignRole", payload); 
      
      fetchData(); // Listeyi yenile
    } catch (error) {
      console.error("Rol atama hatası");
    }
  };

  if (loading) return (
    <div className="text-center py-5"><div className="spinner-border text-primary"></div></div>
  );

  return (
    <div className="page-user-details animate__animated animate__fadeIn">
      <div className="mb-4">
        <button className="btn btn-sm btn-link text-decoration-none ps-0" onClick={() => navigate('/users')}>
          <i className="bi bi-arrow-left me-1"></i> Kullanıcılara Dön
        </button>
        <h3 className="fw-bold mt-2">Kullanıcı Detayı ve Rol Yönetimi</h3>
      </div>

      <div className="row g-4">
        {/* Sol Kolon: Kullanıcı Profil Kartı */}
        <div className="col-lg-4">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-body text-center py-5">
              <div className="bg-primary-subtle text-primary rounded-circle d-flex justify-content-center align-items-center mx-auto mb-3" style={{width: '80px', height: '80px', fontSize: '28px', fontWeight: 'bold'}}>
                {user?.firstName.charAt(0)}{user?.lastName.charAt(0)}
              </div>
              <h4 className="fw-bold mb-1">{user?.firstName} {user?.lastName}</h4>
              <p className="text-muted small mb-3">@{user?.userName}</p>
              
              <div className="text-start mt-4 border-top pt-4">
                <div className="mb-3">
                  <label className="text-muted small d-block">E-Posta Adresi</label>
                  <span className="fw-medium">{user?.email}</span>
                </div>
                <div>
                  <label className="text-muted small d-block">Benzersiz ID</label>
                  <code className="small text-break">{user?.id}</code>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Sağ Kolon: Rol Atama Matrisi */}
        <div className="col-lg-8">
          <div className="card border-0 shadow-sm">
            <div className="card-header bg-transparent border-bottom-0 pt-4 px-4 d-flex justify-content-between align-items-center">
              <h5 className="fw-bold mb-0">Rol Atama</h5>
              <button className="btn btn-primary btn-sm px-4 shadow-sm" onClick={handleSaveRoles} disabled={!selectedRole}>
                <i className="bi bi-save me-2"></i> Rolü Kaydet
              </button>
            </div>
            
            <div className="card-body p-4">
              <p className="text-muted small mb-4">Bu personele atamak istediğiniz tekil sistem rolünü seçin.</p>
              
              <div className="row g-3">
                {allRoles.map(role => (
                  <div key={role.id} className="col-md-6">
                    <div 
                      className={`p-3 rounded border transition-all cursor-pointer ${selectedRole === role.name ? 'border-primary bg-primary-subtle' : 'bg-light'}`}
                      onClick={() => handleSelectRole(role.name)}
                    >
                      <div className="form-check mb-0 d-flex align-items-center justify-content-between">
                        <div>
                          <label className="form-check-label fw-bold cursor-pointer" htmlFor={`role-${role.id}`}>
                            {role.name}
                          </label>
                          <div className="text-muted" style={{fontSize: '11px'}}>Sistem Rolü</div>
                        </div>
                        {/* 🚀 Switch yerine Radio Butona geçtik */}
                        <input 
                          className="form-check-input ms-0" 
                          type="radio" 
                          name="roleSelection"
                          id={`role-${role.id}`}
                          checked={selectedRole === role.name}
                          onChange={() => handleSelectRole(role.name)}
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default UserDetails;