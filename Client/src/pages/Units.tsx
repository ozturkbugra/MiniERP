import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 C# Unit sınıfına (BaseEntity'den gelen Id ile birlikte) tam uyumlu DTO
interface Unit {
  id: string;
  code: string;
  name: string;
}

const Units = () => {
  const { hasPermission } = useAuthStore();
  
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Form State: Sadece Name ve Code
  const [formData, setFormData] = useState({
    code: "",
    name: ""
  });

  const fetchUnits = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Units");
      if (response.data.isSuccess) {
        setUnits(response.data.data);
      }
    } catch (error) {
      console.error("Birimler çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUnits();
  }, [fetchUnits]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    // Eğer Code giriliyorsa otomatik BÜYÜK HARFE çevirelim (UX Best Practice)
    const finalValue = name === "code" ? value.toUpperCase() : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Units/${selectedId}`, { 
          id: selectedId, 
          code: formData.code,
          name: formData.name 
        });
      } else {
        await api.post("/Units", {
          code: formData.code,
          name: formData.name
        });
      }
      setShowModal(false);
      fetchUnits();
    } catch (error) {
      console.error("Birim kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Bu birimi silmek istediğinize emin misiniz? Sistemdeki ürünler bu birimi kullanıyor olabilir!")) {
      try {
        await api.delete(`/Units/${id}`);
        fetchUnits();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ code: "", name: "" });
    setShowModal(true);
  };

  const openEditModal = (unit: Unit) => {
    setSelectedId(unit.id);
    setFormData({ code: unit.code, name: unit.name });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "KISA KOD", 
      accessor: (u: Unit) => (
        <span className="badge bg-secondary-subtle text-secondary px-2 py-1 border border-secondary-subtle">
          {u.code}
        </span>
      ) 
    },
    { 
      header: "BİRİM ADI", 
      accessor: (u: Unit) => <span className="fw-medium text-light">{u.name}</span> 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (u: Unit) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Units?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(u)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Units?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(u.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-units animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Yükleniyor...</span>
          </div>
        </div>
      ) : (
        <DataTable<Unit>
          title="Birim Yönetimi"
          description="Ürünlerin stok takibinde kullanılacak ölçü birimlerini tanımlayın."
          columns={columns}
          data={units}
          onAdd={hasPermission(APP_PERMISSIONS.Units?.Create) ? openCreateModal : undefined}
          addText="Yeni Birim Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Birim Güncelle" : "Yeni Birim Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        saveButtonText={selectedId ? "Güncelle" : "Kaydet"}
      >
        <div className="row g-3">
          <div className="col-md-4">
            <label className="form-label fw-semibold small">Kısa Kod</label>
            <input 
              type="text" 
              name="code" 
              value={formData.code} 
              onChange={handleInputChange} 
              className="form-control form-control-sm text-uppercase" 
              placeholder="Örn: AD, KG, KOLI" 
              autoComplete="off" 
              maxLength={10}
            />
          </div>
          <div className="col-md-8">
            <label className="form-label fw-semibold small">Birim Adı</label>
            <input 
              type="text" 
              name="name" 
              value={formData.name} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Örn: Adet, Kilogram, Koli" 
              autoComplete="off" 
            />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Units;