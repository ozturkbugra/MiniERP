import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 API'den gelen string değerleri (TRY, USD) karşılayacak şekilde güncelledik
const CURRENCY_TYPES: Record<string, { id: number, label: string, badgeClass: string }> = {
  "TRY": { id: 1, label: "TRY", badgeClass: "bg-success-subtle text-success border-success-subtle" },
  "USD": { id: 2, label: "USD", badgeClass: "bg-primary-subtle text-primary border-primary-subtle" },
  "EUR": { id: 3, label: "EUR", badgeClass: "bg-warning-subtle text-warning border-warning-subtle" },
  "GBP": { id: 4, label: "GBP", badgeClass: "bg-danger-subtle text-danger border-danger-subtle" }
};

// Formda select'i yönetmek için ID -> String haritası (Güncelleme için lazım)
const ID_TO_CURRENCY: Record<number, string> = { 1: "TRY", 2: "USD", 3: "EUR", 4: "GBP" };

interface Bank {
  id: string;
  bankName: string;
  accountName: string;
  iban: string;
  branchName?: string;
  currencyType: string; // Artık string geliyor ("TRY" gibi)
}

const Banks = () => {
  const { hasPermission } = useAuthStore();
  const [banks, setBanks] = useState<Bank[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    bankName: "",
    accountName: "",
    branchName: "",
    iban: "",
    currencyType: 1 // Form içindeki select için hala sayı tutuyoruz
  });

  const fetchBanks = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Banks");
      if (response.data.isSuccess) {
        setBanks(response.data.data);
      }
    } catch (error) {
      console.error("Bankalar çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBanks();
  }, [fetchBanks]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    let finalValue: string | number = value;

    if (name === "iban") {
      finalValue = value.replace(/\s+/g, '').toUpperCase();
    } else if (name === "currencyType") {
      finalValue = parseInt(value, 10);
    }

    setFormData(prev => ({ ...prev, [name]: finalValue }));
  };

  const handleSave = async () => {
    try {
      if (selectedId) {
        await api.put(`/Banks/${selectedId}`, { id: selectedId, ...formData });
      } else {
        await api.post("/Banks", formData);
      }
      setShowModal(false);
      fetchBanks();
    } catch (error) {
      console.error("Banka kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Banks/${id}`);
        fetchBanks();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ bankName: "", accountName: "", branchName: "", iban: "", currencyType: 1 });
    setShowModal(true);
  };

  const openEditModal = (bank: Bank) => {
    setSelectedId(bank.id);
    // 🚀 ÖNEMLİ: API'den gelen "TRY" metnini formun anladığı 1 sayısına çeviriyoruz
    const currentId = CURRENCY_TYPES[bank.currencyType]?.id || 1;
    setFormData({ 
      bankName: bank.bankName, 
      accountName: bank.accountName,
      branchName: bank.branchName || "",
      iban: bank.iban,
      currencyType: currentId
    });
    setShowModal(true);
  };

  const columns = [
    { 
      header: "BANKA ADI", 
      accessor: (b: Bank) => (
        <div>
          <div className="fw-medium">{b.bankName}</div>
          <div className="text-muted small">{b.branchName || "-"}</div>
        </div>
      ) 
    },
    { 
      header: "HESAP BİLGİSİ", 
      accessor: (b: Bank) => (
        <div>
          <div className="fw-medium">{b.accountName}</div>
          <span className="badge bg-secondary-subtle text-secondary border border-secondary-subtle small font-monospace">
            {b.iban}
          </span>
        </div>
      ) 
    },
    { 
      header: "DÖVİZ", 
      accessor: (b: Bank) => {
        // 🚀 API'den gelen "TRY" stringini direkt key olarak kullanıyoruz
        const currency = CURRENCY_TYPES[b.currencyType] || { label: b.currencyType, badgeClass: "bg-secondary" };
        return (
          <span className={`badge border ${currency.badgeClass}`}>
            {currency.label}
          </span>
        );
      }
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (b: Bank) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Banks?.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(b)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Banks?.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(b.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-banks animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : (
        <DataTable<Bank>
          title="Banka Yönetimi"
          description="Banka hesaplarınızı ve döviz türlerini yönetin."
          columns={columns}
          data={banks}
          onAdd={hasPermission(APP_PERMISSIONS.Banks?.Create) ? openCreateModal : undefined}
          addText="Yeni Banka Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Banka Güncelle" : "Yeni Banka Tanımla"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
      >
        <div className="row g-3">
          <div className="col-md-6"><label className="form-label small">Banka Adı</label><input type="text" name="bankName" value={formData.bankName} onChange={handleInputChange} className="form-control form-control-sm" /></div>
          <div className="col-md-6"><label className="form-label small">Şube Adı</label><input type="text" name="branchName" value={formData.branchName} onChange={handleInputChange} className="form-control form-control-sm" /></div>
          <div className="col-md-8"><label className="form-label small">Hesap Adı</label><input type="text" name="accountName" value={formData.accountName} onChange={handleInputChange} className="form-control form-control-sm" /></div>
          <div className="col-md-4">
            <label className="form-label small">Para Birimi</label>
            <select name="currencyType" value={formData.currencyType} onChange={handleInputChange} className="form-select form-select-sm">
              <option value={1}>TRY</option>
              <option value={2}>USD</option>
              <option value={3}>EUR</option>
              <option value={4}>GBP</option>
            </select>
          </div>
          <div className="col-12"><label className="form-label small">IBAN</label><input type="text" name="iban" value={formData.iban} onChange={handleInputChange} className="form-control form-control-sm" /></div>
        </div>
      </AppModal>
    </div>
  );
};

export default Banks;