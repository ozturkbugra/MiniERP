import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

interface Warehouse {
  id: string;
  code: string;
  name: string;
  location: string;
}

const Warehouses = () => {
  const { hasPermission } = useAuthStore();
  
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    code: "",
    name: "",
    location: ""
  });

  const fetchWarehouses = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Warehouses");
      if (response.data.isSuccess) {
        setWarehouses(response.data.data);
      }
    } catch (error) {
      console.error("Depolar çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchWarehouses();
  }, [fetchWarehouses]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    // Depo Kodu giriliyorsa otomatik BÜYÜK HARFE çevir
    const finalValue = name === "code" ? value.toUpperCase() : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Warehouses/${selectedId}`, { 
          id: selectedId, 
          ...formData 
        });
      } else {
        await api.post("/Warehouses", formData);
      }
      setShowModal(false);
      fetchWarehouses();
    } catch (error) {
      console.error("Depo kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Bu depoyu silmek istediğinize emin misiniz? (İçinde stok varsa silinemez!)")) {
      try {
        await api.delete(`/Warehouses/${id}`);
        fetchWarehouses();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ code: "", name: "", location: "" });
    setShowModal(true);
  };

  const openEditModal = (warehouse: Warehouse) => {
    setSelectedId(warehouse.id);
    setFormData({ 
      code: warehouse.code, 
      name: warehouse.name, 
      location: warehouse.location || "" 
    });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "DEPO KODU", 
      accessor: (w: Warehouse) => (
        <span className="badge bg-secondary-subtle text-secondary px-2 py-1 border border-secondary-subtle">
          {w.code}
        </span>
      ) 
    },
    { 
      header: "DEPO ADI", 
      accessor: (w: Warehouse) => <span className="fw-medium text-light">{w.name}</span> 
    },
    { 
      header: "LOKASYON", 
      accessor: (w: Warehouse) => <span className="text-muted small"><i className="bi bi-geo-alt me-1"></i>{w.location || "-"}</span> 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (w: Warehouse) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Warehouses?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(w)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Warehouses?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(w.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-warehouses animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Yükleniyor...</span>
          </div>
        </div>
      ) : (
        <DataTable<Warehouse>
          title="Depo Yönetimi"
          description="Sistemdeki ürünlerin tutulacağı fiziksel veya sanal depoları tanımlayın."
          columns={columns}
          data={warehouses}
          onAdd={hasPermission(APP_PERMISSIONS.Warehouses?.Create) ? openCreateModal : undefined}
          addText="Yeni Depo Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Depo Güncelle" : "Yeni Depo Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        saveButtonText={selectedId ? "Güncelle" : "Kaydet"}
      >
        <div className="row g-3">
          <div className="col-md-4">
            <label className="form-label fw-semibold small">Depo Kodu</label>
            <input 
              type="text" 
              name="code" 
              value={formData.code} 
              onChange={handleInputChange} 
              className="form-control form-control-sm text-uppercase" 
              placeholder="Örn: DP-01" 
              autoComplete="off" 
              maxLength={15}
            />
          </div>
          <div className="col-md-8">
            <label className="form-label fw-semibold small">Depo Adı</label>
            <input 
              type="text" 
              name="name" 
              value={formData.name} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Örn: Merkez Depo" 
              autoComplete="off" 
            />
          </div>
          <div className="col-12">
            <label className="form-label fw-semibold small">Lokasyon / Adres</label>
            <input 
              type="text" 
              name="location" 
              value={formData.location} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Örn: Ankara Fabrika, Raf A-12..." 
              autoComplete="off" 
            />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Warehouses;