import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 API'den gelen "TRY", "USD" gibi stringleri badge ve ID ile eşleştiriyoruz
const CURRENCY_TYPES: Record<string, { id: number, label: string, badgeClass: string }> = {
  "TRY": { id: 1, label: "TRY", badgeClass: "bg-success-subtle text-success border-success-subtle" },
  "USD": { id: 2, label: "USD", badgeClass: "bg-primary-subtle text-primary border-primary-subtle" },
  "EUR": { id: 3, label: "EUR", badgeClass: "bg-warning-subtle text-warning border-warning-subtle" },
  "GBP": { id: 4, label: "GBP", badgeClass: "bg-danger-subtle text-danger border-danger-subtle" }
};

interface Cash {
  id: string;
  name: string;
  currencyType: string; // API'den string geliyor
}

const Cashes = () => {
  const { hasPermission } = useAuthStore();
  
  const [cashes, setCashes] = useState<Cash[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    name: "",
    currencyType: 1 // Select için ID tutuyoruz
  });

  const fetchCashes = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Cashes");
      if (response.data.isSuccess) {
        setCashes(response.data.data);
      }
    } catch (error) {
      console.error("Kasalar çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCashes();
  }, [fetchCashes]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    const finalValue = name === "currencyType" ? parseInt(value, 10) : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Cashes/${selectedId}`, { id: selectedId, ...formData });
      } else {
        await api.post("/Cashes", formData);
      }
      setShowModal(false);
      fetchCashes();
    } catch (error) {
      console.error("Kasa kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Bu kasayı silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Cashes/${id}`);
        fetchCashes();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ name: "", currencyType: 1 });
    setShowModal(true);
  };

  const openEditModal = (cash: Cash) => {
    setSelectedId(cash.id);
    const currentId = CURRENCY_TYPES[cash.currencyType]?.id || 1;
    setFormData({ name: cash.name, currencyType: currentId });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "KASA ADI", 
      accessor: (c: Cash) => <span className="fw-medium">{c.name}</span> 
    },
    { 
      header: "DÖVİZ TÜRÜ", 
      accessor: (c: Cash) => {
        const currency = CURRENCY_TYPES[c.currencyType] || { label: c.currencyType, badgeClass: "bg-secondary" };
        return <span className={`badge border ${currency.badgeClass}`}>{currency.label}</span>;
      }
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (c: Cash) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Cashes?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(c)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Cashes?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(c.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-cashes animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : (
        <DataTable<Cash>
          title="Kasa Yönetimi"
          description="Sistemdeki nakit işlemlerini takip edeceğiniz kasaları tanımlayın."
          columns={columns}
          data={cashes}
          onAdd={hasPermission(APP_PERMISSIONS.Cashes?.Create) ? openCreateModal : undefined}
          addText="Yeni Kasa Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Kasa Güncelle" : "Yeni Kasa Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
      >
        <div className="row g-3">
          <div className="col-md-8">
            <label className="form-label fw-semibold small">Kasa Adı</label>
            <input type="text" name="name" value={formData.name} onChange={handleInputChange} className="form-control form-control-sm" placeholder="Örn: Merkez TL Kasası" autoComplete="off" />
          </div>
          <div className="col-md-4">
            <label className="form-label fw-semibold small">Para Birimi</label>
            <select name="currencyType" value={formData.currencyType} onChange={handleInputChange} className="form-select form-select-sm">
              <option value={1}>TRY - Türk Lirası</option>
              <option value={2}>USD - Amerikan Doları</option>
              <option value={3}>EUR - Euro</option>
              <option value={4}>GBP - İngiliz Sterlini</option>
            </select>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Cashes;