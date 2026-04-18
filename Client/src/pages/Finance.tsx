import { useState, useEffect, useCallback } from 'react';
import AppModal from '../components/Common/AppModal';
// 🚀 1. ÇÖZÜM: 'type' anahtar kelimesini ekledik
import AppSelect, { type SelectOption } from '../components/Common/AppSelect';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

const Finance = () => {
  const { hasPermission } = useAuthStore();
  const [modalType, setModalType] = useState<'collection' | 'payment' | 'transfer' | 'opening' | null>(null);

  const [rawCustomers, setRawCustomers] = useState<any[]>([]); 
  const [cashes, setCashes] = useState<SelectOption[]>([]);
  const [banks, setBanks] = useState<SelectOption[]>([]);

  const [formData, setFormData] = useState({
    date: new Date().toISOString().split('T')[0],
    description: "",
    amount: 0,
    customerId: null as string | null,
    cashId: null as string | null,
    bankId: null as string | null,
    fromCashId: null as string | null,
    fromBankId: null as string | null,
    toCashId: null as string | null,
    toBankId: null as string | null,
    isDebit: true,
    accountType: 'cash' as 'cash' | 'bank' | 'customer'
  });

  const fetchDependencies = useCallback(async () => {
    try {
      const [cRes, cashRes, bankRes] = await Promise.all([
        api.get("/Customers"), 
        api.get("/Cashes"), 
        api.get("/Banks")
      ]);
      if (cRes.data.isSuccess) setRawCustomers(cRes.data.data);
      if (cashRes.data.isSuccess) setCashes(cashRes.data.data.map((x: any) => ({ label: x.name || "", value: x.id })));
      if (bankRes.data.isSuccess) setBanks(bankRes.data.data.map((x: any) => ({ label: x.bankName || "", value: x.id })));
    } catch (error) { console.error("Hata:", error); }
  }, []);

  useEffect(() => { fetchDependencies(); }, [fetchDependencies]);

  const getFilteredCustomers = (): SelectOption[] => {
    return rawCustomers
      .filter(c => {
        const type = c.type;
        if (modalType === 'collection') return type === 1 || type === 3 || type === "Buyer" || type === "Both";
        if (modalType === 'payment') return type === 2 || type === 3 || type === "Supplier" || type === "Both";
        return true;
      })
      .map(c => ({ label: c.name || "", value: c.id }));
  };

  const handleSave = async () => {
    try {
      let endpoint = "";
      if (modalType === 'collection') endpoint = "/Transactions/Collection";
      else if (modalType === 'payment') endpoint = "/Transactions/Payment";
      else if (modalType === 'transfer') endpoint = "/Transactions/Transfer";
      else if (modalType === 'opening') endpoint = "/Transactions/OpeningBalance";

      const response = await api.post(endpoint, formData);
      if (response.data.isSuccess) {
        setModalType(null);
        resetForm();
      }
    } catch (error) { alert("İşlem hatası."); }
  };

  const resetForm = () => {
    setFormData({
      date: new Date().toISOString().split('T')[0], description: "", amount: 0,
      customerId: null, cashId: null, bankId: null, fromCashId: null, fromBankId: null,
      toCashId: null, toBankId: null, isDebit: true, accountType: 'cash'
    });
  };

  return (
    <div className="page-finance animate__animated animate__fadeIn">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-header bg-transparent border-0 pt-4 px-4">
          <h5 className="fw-bold mb-0">Finansal İşlemler</h5>
          <p className="text-muted small">Tahsilat, ödeme ve virman işlemlerini yönetin.</p>
        </div>
        <div className="card-body p-4 d-flex flex-wrap gap-3">
          {hasPermission(APP_PERMISSIONS.Finance.Transaction) && (
            <>
              <button className="btn btn-success px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('collection'); }}>
                <i className="ph-fill ph-hand-coins fs-5"></i> Tahsilat Yap
              </button>
              <button className="btn btn-danger px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('payment'); }}>
                <i className="ph-fill ph-money fs-5"></i> Ödeme Yap
              </button>
              <button className="btn btn-primary px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('transfer'); }}>
                <i className="ph-fill ph-arrows-left-right fs-5"></i> Virman
              </button>
              <button className="btn btn-outline-secondary px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('opening'); }}>
                <i className="ph-fill ph-flag-banner fs-5"></i> Açılış Bakiyesi
              </button>
            </>
          )}
        </div>
      </div>

      <AppModal 
        show={!!modalType} 
        title={modalType === 'collection' ? "Tahsilat" : modalType === 'payment' ? "Ödeme" : modalType === 'transfer' ? "Transfer" : "Açılış"}
        onClose={() => setModalType(null)}
        onSave={handleSave}
        // 🚀 2. ÇÖZÜM: 'md' yerine 'undefined' yaparak default boyuta çektik
        size={modalType === 'transfer' ? 'lg' : undefined}
      >
        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label small fw-bold">Tarih</label>
            <input type="date" className="form-control form-control-sm" value={formData.date} onChange={(e) => setFormData({...formData, date: e.target.value})} />
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">Tutar</label>
            <input type="number" className="form-control form-control-sm" value={formData.amount} onChange={(e) => setFormData({...formData, amount: parseFloat(e.target.value)})} />
          </div>

          {(modalType === 'collection' || modalType === 'payment') && (
            <>
              <div className="col-12">
                <AppSelect 
                    label={modalType === 'collection' ? "Müşteri" : "Tedarikçi"} 
                    options={getFilteredCustomers()} 
                    value={formData.customerId || ""} 
                    onChange={(val) => setFormData({...formData, customerId: val})} 
                    isSearchable 
                />
              </div>
              <div className="col-md-12">
                <label className="form-label small fw-bold">Ödeme Yeri</label>
                <div className="d-flex gap-3 mb-2">
                  <div className="form-check">
                    <input className="form-check-input" type="radio" checked={formData.accountType === 'cash'} onChange={() => setFormData({...formData, accountType: 'cash', bankId: null})} />
                    <label className="form-check-label small">Kasa</label>
                  </div>
                  <div className="form-check">
                    <input className="form-check-input" type="radio" checked={formData.accountType === 'bank'} onChange={() => setFormData({...formData, accountType: 'bank', cashId: null})} />
                    <label className="form-check-label small">Banka</label>
                  </div>
                </div>
                {formData.accountType === 'cash' ? (
                  <AppSelect label="Kasa" options={cashes} value={formData.cashId || ""} onChange={(val) => setFormData({...formData, cashId: val})} />
                ) : (
                  <AppSelect label="Banka" options={banks} value={formData.bankId || ""} onChange={(val) => setFormData({...formData, bankId: val})} />
                )}
              </div>
            </>
          )}

          {modalType === 'transfer' && (
            <>
              <div className="col-md-6 border-end">
                <h6 className="small fw-bold text-danger border-bottom pb-2">KAYNAK</h6>
                <AppSelect label="Kasa" options={cashes} value={formData.fromCashId || ""} onChange={(val) => setFormData({...formData, fromCashId: val, fromBankId: null})} />
                <div className="text-center my-2 small text-muted">Veya</div>
                <AppSelect label="Banka" options={banks} value={formData.fromBankId || ""} onChange={(val) => setFormData({...formData, fromBankId: val, fromCashId: null})} />
              </div>
              <div className="col-md-6">
                <h6 className="small fw-bold text-success border-bottom pb-2">HEDEF</h6>
                <AppSelect label="Kasa" options={cashes} value={formData.toCashId || ""} onChange={(val) => setFormData({...formData, toCashId: val, toBankId: null})} />
                <div className="text-center my-2 small text-muted">Veya</div>
                <AppSelect label="Banka" options={banks} value={formData.toBankId || ""} onChange={(val) => setFormData({...formData, toBankId: val, toCashId: null})} />
              </div>
            </>
          )}

          {modalType === 'opening' && (
            <>
              <div className="col-md-12">
                <label className="form-label small fw-bold">Hesap Tipi</label>
                <select className="form-select form-select-sm" value={formData.accountType} onChange={(e:any) => setFormData({...formData, accountType: e.target.value, cashId: null, bankId: null, customerId: null})}>
                  <option value="cash">Kasa</option>
                  <option value="bank">Banka</option>
                  <option value="customer">Cari</option>
                </select>
              </div>
              <div className="col-12">
                {formData.accountType === 'cash' && <AppSelect label="Kasa" options={cashes} value={formData.cashId || ""} onChange={(val) => setFormData({...formData, cashId: val})} />}
                {formData.accountType === 'bank' && <AppSelect label="Banka" options={banks} value={formData.bankId || ""} onChange={(val) => setFormData({...formData, bankId: val})} />}
                {formData.accountType === 'customer' && <AppSelect label="Cari" options={getFilteredCustomers()} value={formData.customerId || ""} onChange={(val) => setFormData({...formData, customerId: val})} />}
              </div>
              <div className="col-md-12">
                <div className="form-check form-switch p-0 ms-4">
                  <input className="form-check-input" type="checkbox" checked={formData.isDebit} onChange={(e) => setFormData({...formData, isDebit: e.target.checked})} />
                  <label className="form-check-label small ms-2">{formData.isDebit ? 'BORÇ (Giriş)' : 'ALACAK (Çıkış)'}</label>
                </div>
              </div>
            </>
          )}

          <div className="col-12">
            <label className="form-label small fw-bold">Açıklama</label>
            <textarea className="form-control form-control-sm" rows={2} value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})}></textarea>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Finance;