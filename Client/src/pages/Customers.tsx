import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 C# CustomerType Enum eşleşmeleri
const CUSTOMER_TYPES: Record<string, { id: number, label: string, badgeClass: string }> = {
  "Buyer": { id: 1, label: "Müşteri", badgeClass: "bg-info-subtle text-info border-info-subtle" },
  "Supplier": { id: 2, label: "Tedarikçi", badgeClass: "bg-warning-subtle text-warning border-warning-subtle" },
  "Both": { id: 3, label: "Müşteri + Tedarikçi", badgeClass: "bg-primary-subtle text-primary border-primary-subtle" }
};

interface Customer {
  id: string;
  name: string;
  taxDepartment?: string;
  taxNumber?: string;
  phone?: string;
  email?: string;
  address?: string;
  type: string; // API'den string geliyor
}

const Customers = () => {
  const { hasPermission } = useAuthStore();
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    name: "",
    taxDepartment: "",
    taxNumber: "",
    phone: "",
    email: "",
    address: "",
    type: 1 // Default: Buyer
  });

  const fetchCustomers = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Customers");
      if (response.data.isSuccess) {
        setCustomers(response.data.data);
      }
    } catch (error) {
      console.error("Cariler çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCustomers();
  }, [fetchCustomers]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    const finalValue = name === "type" ? parseInt(value, 10) : value;
    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Customers/${selectedId}`, { id: selectedId, ...formData });
      } else {
        await api.post("/Customers", formData);
      }
      setShowModal(false);
      fetchCustomers();
    } catch (error) {
      console.error("Cari kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Bu cariyi silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Customers/${id}`);
        fetchCustomers();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ name: "", taxDepartment: "", taxNumber: "", phone: "", email: "", address: "", type: 1 });
    setShowModal(true);
  };

  const openEditModal = (c: Customer) => {
    setSelectedId(c.id);
    const typeId = CUSTOMER_TYPES[c.type]?.id || 1;
    setFormData({ 
      name: c.name, 
      taxDepartment: c.taxDepartment || "", 
      taxNumber: c.taxNumber || "", 
      phone: c.phone || "", 
      email: c.email || "", 
      address: c.address || "", 
      type: typeId 
    });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "UNVAN / AD SOYAD", 
      accessor: (c: Customer) => (
        <div>
          <div className="fw-medium text-light">{c.name}</div>
          <div className="text-muted small">{c.email || "-"}</div>
        </div>
      ) 
    },
    { 
      header: "VERGİ BİLGİSİ", 
      accessor: (c: Customer) => (
        <div className="small text-muted">
          {c.taxDepartment || "-"} / {c.taxNumber || "-"}
        </div>
      ) 
    },
    { 
      header: "TİP", 
      accessor: (c: Customer) => {
        const typeInfo = CUSTOMER_TYPES[c.type] || { label: c.type, badgeClass: "bg-secondary" };
        return <span className={`badge border ${typeInfo.badgeClass}`}>{typeInfo.label}</span>;
      }
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (c: Customer) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Customers?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(c)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Customers?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(c.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-customers animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : (
        <DataTable<Customer>
          title="Cari Kart Yönetimi"
          description="Müşteri ve tedarikçilerinizin iletişim ve vergi bilgilerini yönetin."
          columns={columns}
          data={customers}
          onAdd={hasPermission(APP_PERMISSIONS.Customers?.Create) ? openCreateModal : undefined}
          addText="Yeni Cari Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Cari Güncelle" : "Yeni Cari Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
      >
        <div className="row g-3">
          <div className="col-md-8">
            <label className="form-label small fw-bold">Unvan / Ad Soyad</label>
            <input type="text" name="name" value={formData.name} onChange={handleInputChange} className="form-control form-control-sm" autoComplete="off" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">Cari Tipi</label>
            <select name="type" value={formData.type} onChange={handleInputChange} className="form-select form-select-sm">
              <option value={1}>Müşteri (Alıcı)</option>
              <option value={2}>Tedarikçi (Satıcı)</option>
              <option value={3}>Her İkisi (Alıcı+Satıcı)</option>
            </select>
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">Vergi Dairesi</label>
            <input type="text" name="taxDepartment" value={formData.taxDepartment} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">Vergi No / T.C.</label>
            <input type="text" name="taxNumber" value={formData.taxNumber} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">Telefon</label>
            <input type="text" name="phone" value={formData.phone} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">E-Posta</label>
            <input type="email" name="email" value={formData.email} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-12">
            <label className="form-label small fw-bold">Adres</label>
            <textarea name="address" value={formData.address} onChange={handleInputChange} className="form-control form-control-sm" rows={2}></textarea>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Customers;