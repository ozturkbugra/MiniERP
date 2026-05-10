import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

interface Brand {
  id: string;
  name: string;
}

const Brands = () => {
  const { hasPermission } = useAuthStore();
  
  // State Tanımları
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Form State - Sadece isim kaldı
  const [formData, setFormData] = useState({
    name: ""
  });

  // 1. Markaları Çekme
  const fetchBrands = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Brands");
      if (response.data.isSuccess) {
        setBrands(response.data.data);
      }
    } catch (error) {
      console.error("Markalar çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBrands();
  }, [fetchBrands]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ name: e.target.value });
  };

  // 2. Kaydetme
  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Brands/${selectedId}`, { 
          id: selectedId, 
          name: formData.name 
        });
      } else {
        await api.post("/Brands", formData);
      }
      setShowModal(false);
      fetchBrands(); 
    } catch (error) {
      console.error("Marka kaydedilirken hata oluştu");
    }
  };

  // 3. Silme
  const handleDelete = async (id: string) => {
    if (window.confirm("Bu markayı silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Brands/${id}`);
        fetchBrands();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ name: "" });
    setShowModal(true);
  };

  const openEditModal = (brand: Brand) => {
    setSelectedId(brand.id);
    setFormData({ name: brand.name });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "MARKA ADI", 
      accessor: (b: Brand) => (
        <span className="fw-medium">{b.name}</span>
      ) 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (b: Brand) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Brands?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(b)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Brands?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(b.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-brands animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Yükleniyor...</span>
          </div>
        </div>
      ) : (
        <DataTable<Brand>
          title="Marka Yönetimi"
          description="Sistemdeki ürünlere ait markaları buradan yönetebilirsiniz."
          columns={columns}
          data={brands}
          onAdd={hasPermission(APP_PERMISSIONS.Brands?.Create) ? openCreateModal : undefined}
          addText="Yeni Marka Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Marka Güncelle" : "Yeni Marka Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        saveButtonText={selectedId ? "Güncelle" : "Kaydet"}
      >
        <div className="row">
          <div className="col-12">
            <label className="form-label fw-semibold small">Marka Adı</label>
            <input 
              type="text" 
              name="name" 
              value={formData.name} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Örn: Samsung, Apple, Bosch" 
              autoComplete="off" 
            />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Brands;