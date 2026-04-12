import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom'; // 🚀 Yönlendirme için eklendi
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

interface Role {
  id: string;
  name: string;
  description: string;
}

const Roles = () => {
  const { hasPermission } = useAuthStore();
  const navigate = useNavigate(); // 🚀 Hook'u başlattık
  
  // State Tanımları
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  
  // Form State'leri
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
  const [roleName, setRoleName] = useState("");
  const [roleDescription, setRoleDescription] = useState("");

  // 1. Veri Çekme (Read)
  const fetchRoles = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Roles");
      if (response.data.isSuccess) {
        setRoles(response.data.data);
      }
    } catch (error) {
      console.error("Roller yüklenirken hata oluştu");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRoles();
  }, [fetchRoles]);

  // 2. Kaydetme (Create & Update)
  const handleSave = async () => {
    try {
      const payload = { 
        name: roleName, 
        description: roleDescription 
      };

      if (selectedRoleId) {
        // GÜNCELLEME (PUT)
        await api.put(`/Roles/${selectedRoleId}`, { 
          Id: selectedRoleId, 
          ...payload 
        });
      } else {
        // YENİ KAYIT (POST)
        await api.post("/Roles/CreateRole", payload);
      }

      setShowModal(false);
      fetchRoles(); 
    } catch (error: any) {
      console.error("İşlem sırasında hata oluştu:", error.response?.data);
    }
  };

  // 3. Silme (Delete)
  const handleDelete = async (id: string) => {
    if (window.confirm("Bu rolü silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Roles/${id}`);
        fetchRoles();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  // Modal'ı açan yardımcı fonksiyonlar
  const openCreateModal = () => {
    setSelectedRoleId(null);
    setRoleName("");
    setRoleDescription("");
    setShowModal(true);
  };

  const openEditModal = (role: Role) => {
    setSelectedRoleId(role.id);
    setRoleName(role.name);
    setRoleDescription(role.description);
    setShowModal(true);
  };

  // Tablo Sütunları
  const columns = [
    { header: "ROL ADI", accessor: "name" as keyof Role },
    { header: "AÇIKLAMA", accessor: "description" as keyof Role },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (role: Role) => (
        <div className="d-flex justify-content-end gap-2">
          {/* 🛡️ YENİ: ROL DETAYI VE YETKİ ATAMA BUTONU */}
          <button 
            className="btn btn-sm btn-outline-info border-0" 
            onClick={() => navigate(`/roles/${role.id}/permissions`)}
            title="Rol Detayı ve Yetkiler"
          >
            <i className="bi bi-shield-lock"></i> Rol Detayı
          </button>

          {/* Güncelleme Yetkisi Kontrolü */}
          {hasPermission(APP_PERMISSIONS.Roles.Update) && (
            <button className="btn btn-sm btn-outline-primary border-0" onClick={() => openEditModal(role)}>
              <i className="bi bi-pencil"></i>
            </button>
          )}

          {/* Silme Yetkisi Kontrolü */}
          {hasPermission(APP_PERMISSIONS.Roles.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0" onClick={() => handleDelete(role.id)}>
              <i className="bi bi-trash"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-roles">
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : (
        <DataTable<Role>
          title="Rol Yönetimi"
          description="Sistemdeki rolleri ve açıklamalarını buradan yönetebilirsiniz."
          columns={columns}
          data={roles}
          onAdd={hasPermission(APP_PERMISSIONS.Roles.Create) ? openCreateModal : undefined}
          addText="Yeni Rol Ekle"
        />
      )}

      {/* Rol Ekleme/Düzenleme Modalı */}
      <AppModal 
        show={showModal} 
        title={selectedRoleId ? "Rolü Güncelle" : "Yeni Rol Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        saveButtonText={selectedRoleId ? "Güncelle" : "Kaydet"}
      >
        <div className="row g-3">
          <div className="col-12">
            <label className="form-label small fw-bold">Rol Adı</label>
            <input 
              type="text" 
              className="form-control form-control-sm" 
              value={roleName}
              onChange={(e) => setRoleName(e.target.value)}
              placeholder="Örn: Muhasebe Müdürü"
            />
          </div>
          <div className="col-12">
            <label className="form-label small fw-bold">Açıklama</label>
            <textarea 
              className="form-control form-control-sm" 
              rows={3}
              value={roleDescription}
              onChange={(e) => setRoleDescription(e.target.value)}
              placeholder="Bu rolün yetki sınırlarını kısaca açıklayın..."
            ></textarea>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Roles;