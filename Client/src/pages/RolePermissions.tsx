import { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import api from '../api/axiosInstance';
import AppModal from '../components/Common/AppModal';
import DataTable from '../components/Common/DataTable';

interface PermissionGroup {
  moduleName: string;
  permissions: string[];
}

interface PermissionRow {
  id: string;
  module: string;
  name: string;
  fullPath: string;
}

const RolePermissions = () => {
  const { roleId } = useParams();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  
  // Tüm yetki listesi (Gruplanmış yapı)
  const [allPermissions, setAllPermissions] = useState<PermissionGroup[]>([]);
  // Role ait yetkiler (Düz string dizisi - Swagger'daki data)
  const [currentPermissions, setCurrentPermissions] = useState<string[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  // 1. Veri Çekme (Swagger'daki flat array yapısına göre güncellendi)
  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [allRes, roleRes] = await Promise.all([
        api.get("/Roles/GetAllPermissions"),
        api.get(`/Roles/GetPermissionsByRoleId/${roleId}`)
      ]);

      if (allRes.data.isSuccess) setAllPermissions(allRes.data.data);
      
      if (roleRes.data.isSuccess) {
        // Swagger görüntüsüne göre direkt data'yı alıyoruz, flatMap'e gerek yok
        const perms = roleRes.data.data; 
        setCurrentPermissions(perms);
        setSelectedPermissions(perms); // Modal'da işaretli gelmesi için
      }
    } catch (error) {
      console.error("Yetkiler çekilirken hata oluştu");
    } finally {
      setLoading(false);
    }
  }, [roleId]);

  useEffect(() => { fetchData(); }, [fetchData]);

  // 2. DataTable için Veri Eşleştirme
  const tableData = useMemo(() => {
    const rows: PermissionRow[] = [];
    
    // Elimizdeki düz yetki kodlarını (string), modül isimlerini bulmak için allPermissions ile eşleştiriyoruz
    allPermissions.forEach(group => {
      group.permissions.forEach(perm => {
        if (currentPermissions.includes(perm)) {
          rows.push({
            id: perm,
            module: group.moduleName,
            name: perm.split('.').pop() || perm, // Permissions.Users.View -> View
            fullPath: perm
          });
        }
      });
    });
    return rows;
  }, [allPermissions, currentPermissions]);

  const columns = [
    { header: "MODÜL", accessor: "module" as keyof PermissionRow },
    { header: "YETKİ TANIMI", accessor: "name" as keyof PermissionRow },
    { header: "YETKİ KODU", accessor: "fullPath" as keyof PermissionRow },
  ];

  // 3. Yetki Atama (AssignPermissions POST işlemi)
  const handleSave = async () => {
    try {
      await api.post("/Roles/AssignPermissions", {
        roleId: roleId,
        permissions: selectedPermissions
      });
      setShowModal(false);
      fetchData(); // Listeyi yenile
    } catch (error) {
      console.error("Yetkiler atanırken hata oluştu");
    }
  };

  return (
    <div className="page-role-permissions">
      <div className="mb-4 d-flex justify-content-between align-items-center">
        <div>
          <button className="btn btn-sm btn-link text-decoration-none ps-0" onClick={() => navigate('/roles')}>
            <i className="bi bi-arrow-left"></i> Rollere Dön
          </button>
          <h4 className="mb-0 fw-bold">Rol Yetki Detayları</h4>
        </div>
        <button className="btn btn-primary shadow-sm" onClick={() => setShowModal(true)}>
          <i className="bi bi-shield-plus me-2"></i> Yetkileri Düzenle
        </button>
      </div>

      {loading ? (
        <div className="text-center py-5"><div className="spinner-border text-primary"></div></div>
      ) : (
        <DataTable<PermissionRow>
          title="Atanmış Yetkiler"
          description="Bu rolün sahip olduğu aktif yetkiler aşağıda listelenmiştir."
          columns={columns}
          data={tableData}
        />
      )}

      {/* 🛡️ YETKİ SEÇİM MATRİSİ MODALI */}
      <AppModal 
        show={showModal} 
        title="Yetki Matrisi" 
        onClose={() => setShowModal(false)} 
        onSave={handleSave}
        size="lg"
      >
        <div className="permission-scroll-area" style={{ maxHeight: '65vh', overflowY: 'auto' }}>
          {allPermissions.map(group => (
            <div key={group.moduleName} className="mb-4">
              <div className="bg-light p-2 mb-2 fw-bold border-start border-primary border-4 rounded-end">
                {group.moduleName} Modülü
              </div>
              <div className="row g-2 px-2">
                {group.permissions.map(perm => (
                  <div key={perm} className="col-md-6">
                    <div className={`form-check form-switch border p-2 rounded transition-all ${selectedPermissions.includes(perm) ? 'bg-primary-subtle border-primary' : ''}`}>
                      <input 
                        className="form-check-input ms-0 me-2" 
                        type="checkbox" 
                        id={perm}
                        checked={selectedPermissions.includes(perm)}
                        onChange={() => {
                          setSelectedPermissions(prev => 
                            prev.includes(perm) ? prev.filter(p => p !== perm) : [...prev, perm]
                          );
                        }}
                      />
                      <label className="form-check-label small cursor-pointer w-100" htmlFor={perm}>
                        {perm.split('.').pop()}
                        <div className="text-muted" style={{ fontSize: '10px' }}>{perm}</div>
                      </label>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </AppModal>
    </div>
  );
};

export default RolePermissions;