import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

interface Category {
  id: string;
  name: string;
  description?: string;
}

const Categories = () => {
  const { hasPermission } = useAuthStore();
  
  // State Tanımları
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Form State
  const [formData, setFormData] = useState({
    name: "",
    description: ""
  });

  // 1. Kategorileri Çekme (Read)
  const fetchCategories = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Categories");
      
      if (response.data.isSuccess) {
        setCategories(response.data.data);
      }
    } catch (error) {
      console.error("Kategoriler çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  // Form Input Yöneticisi
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  // 2. Kaydetme (Create & Update)
  const handleSave = async () => {
    try {
      if (selectedId) {
        // GÜNCELLEME (PUT) - Controller'ın beklediği gibi ID'yi de yolluyoruz
        await api.put(`/Categories/${selectedId}`, { 
          id: selectedId, 
          ...formData 
        });
      } else {
        // YENİ KAYIT (POST)
        await api.post("/Categories", formData);
      }

      setShowModal(false);
      fetchCategories(); // Tabloyu tazele
    } catch (error) {
      console.error("Kategori kaydedilirken hata oluştu");
    }
  };

  // 3. Silme (Delete)
  const handleDelete = async (id: string) => {
    if (window.confirm("Bu kategoriyi silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Categories/${id}`);
        fetchCategories();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  // Modal Açıcılar
  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ name: "", description: "" });
    setShowModal(true);
  };

  const openEditModal = (category: Category) => {
    setSelectedId(category.id);
    setFormData({ 
      name: category.name, 
      description: category.description || "" 
    });
    setShowModal(true);
  };

  // Tablo Sütunları
  const columns = [
    { 
      header: "KATEGORİ ADI", 
      accessor: (c: Category) => (
        <span className="fw-medium">{c.name}</span>
      ) 
    },
    { 
      header: "AÇIKLAMA", 
      accessor: (c: Category) => (
        <span className="text-muted small">{c.description || "-"}</span>
      ) 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (c: Category) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Categories.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(c)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Categories.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(c.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-categories animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Yükleniyor...</span>
          </div>
        </div>
      ) : (
        <DataTable<Category>
          title="Kategori Yönetimi"
          description="Sistemde kullanılan ürün ve hizmet kategorilerini buradan yönetebilirsiniz."
          columns={columns}
          data={categories}
          onAdd={hasPermission(APP_PERMISSIONS.Categories.Create) ? openCreateModal : undefined}
          addText="Yeni Kategori Ekle"
        />
      )}

      {/* Kategori Ekleme/Düzenleme Modalı */}
      <AppModal 
        show={showModal} 
        title={selectedId ? "Kategori Güncelle" : "Yeni Kategori Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        saveButtonText={selectedId ? "Güncelle" : "Kaydet"}
      >
        <div className="row g-3">
          <div className="col-12">
            <label className="form-label fw-semibold small">Kategori Adı</label>
            <input 
              type="text" 
              name="name" 
              value={formData.name} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Örn: Hırdavat, Elektronik" 
              autoComplete="off" 
            />
          </div>
          <div className="col-12">
            <label className="form-label fw-semibold small">Açıklama (Opsiyonel)</label>
            <textarea 
              name="description" 
              value={formData.description} 
              onChange={handleInputChange} 
              className="form-control form-control-sm" 
              placeholder="Kategori hakkında kısa bir açıklama..." 
              rows={3} 
              autoComplete="off" 
            />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Categories;