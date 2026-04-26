import { useState, useEffect, useCallback } from 'react';
import AppModal from '../components/Common/AppModal';
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

  // 🚀 Tutar görünümü için yerel state
  const [displayAmount, setDisplayAmount] = useState<string>("");

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

  // 🛠️ YARDIMCI FORMAT FONKSİYONLARI
  const maskCurrency = (val: string) => {
    let clean = val.replace(/[^0-9,]/g, "");
    const parts = clean.split(",");
    if (parts.length > 2) clean = parts[0] + "," + parts[1];
    const integerPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    return parts.length > 1 ? `${integerPart},${parts[1].slice(0, 2)}` : integerPart;
  };

  const unmaskPrice = (val: string) => {
    if (!val) return 0;
    return parseFloat(val.replace(/\./g, "").replace(",", ".")) || 0;
  };

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
        // API'den gelen tipe göre filtreleme (Guid-safe eşleşme için toLowerCase gerekebilir)
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
    setDisplayAmount("");
  };

  return (
    <div className="page-finance animate__animated animate__fadeIn">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-header bg-transparent border-0 pt-4 px-4">
          <h5 className="fw-bold mb-0 text-primary">Finansal İşlemler</h5>
          <p className="text-muted small">Tahsilat, ödeme ve virman işlemlerini tek merkezden yönetin.</p>
        </div>
        <div className="card-body p-4 d-flex flex-wrap gap-3">
          {hasPermission(APP_PERMISSIONS.Finance.Transaction) && (
            <>
              <button className="btn btn-success px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('collection'); }}>
                <i className="bi bi-cash-stack fs-5"></i> Tahsilat Yap
              </button>
              <button className="btn btn-danger px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('payment'); }}>
                <i className="bi bi-credit-card fs-5"></i> Ödeme Yap
              </button>
              <button className="btn btn-primary px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('transfer'); }}>
                <i className="bi bi-arrow-left-right fs-5"></i> Virman
              </button>
              <button className="btn btn-outline-secondary px-4 py-2 d-flex align-items-center gap-2" onClick={() => { resetForm(); setModalType('opening'); }}>
                <i className="bi bi-flag fs-5"></i> Açılış Bakiyesi
              </button>
            </>
          )}
        </div>
      </div>

      <AppModal 
        show={!!modalType} 
        title={modalType === 'collection' ? "💰 Tahsilat İşlemi" : modalType === 'payment' ? "💸 Ödeme İşlemi" : modalType === 'transfer' ? "🔄 Transfer (Virman)" : "🚩 Açılış Bakiyesi"}
        onClose={() => setModalType(null)}
        onSave={handleSave}
        size={modalType === 'transfer' ? 'lg' : undefined}
      >
        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label small fw-bold">Tarih</label>
            <input type="date" className="form-control form-control-sm" value={formData.date} onChange={(e) => setFormData({...formData, date: e.target.value})} />
          </div>

          {/* 🚀 TUTAR ALANI - MUHASEBE FORMATI ENTEGRE EDİLDİ */}
          <div className="col-md-6">
            <label className="form-label small fw-bold">İşlem Tutarı (₺)</label>
            <input 
                type="text" 
                className="form-control form-control-sm text-end fw-bold border-primary-subtle" 
                placeholder="0,00"
                value={displayAmount}
                onChange={(e) => {
                  const masked = maskCurrency(e.target.value);
                  setDisplayAmount(masked);
                  setFormData(prev => ({ ...prev, amount: unmaskPrice(masked) }));
                }}
                onFocus={(e) => e.target.select()}
            />
          </div>

          {(modalType === 'collection' || modalType === 'payment') && (
            <>
              <div className="col-12">
                <AppSelect 
                    label={modalType === 'collection' ? "Tahsilat Yapılan Müşteri" : "Ödeme Yapılan Tedarikçi"} 
                    options={getFilteredCustomers()} 
                    value={formData.customerId || ""} 
                    onChange={(val) => setFormData({...formData, customerId: val})} 
                    isSearchable 
                />
              </div>
              <div className="col-md-12">
                <label className="form-label small fw-bold">Para Giriş/Çıkış Yeri</label>
                <div className="d-flex gap-4 mb-2 bg-light p-2 rounded-2">
                  <div className="form-check">
                    <input className="form-check-input" type="radio" name="accType" checked={formData.accountType === 'cash'} onChange={() => setFormData({...formData, accountType: 'cash', bankId: null})} />
                    <label className="form-check-label small fw-medium">Kasa</label>
                  </div>
                  <div className="form-check">
                    <input className="form-check-input" type="radio" name="accType" checked={formData.accountType === 'bank'} onChange={() => setFormData({...formData, accountType: 'bank', cashId: null})} />
                    <label className="form-check-label small fw-medium">Banka</label>
                  </div>
                </div>
                {formData.accountType === 'cash' ? (
                  <AppSelect label="İlgili Kasa" options={cashes} value={formData.cashId || ""} onChange={(val) => setFormData({...formData, cashId: val})} isSearchable />
                ) : (
                  <AppSelect label="İlgili Banka Hesabı" options={banks} value={formData.bankId || ""} onChange={(val) => setFormData({...formData, bankId: val})} isSearchable />
                )}
              </div>
            </>
          )}

          {modalType === 'transfer' && (
            <>
              <div className="col-md-6 border-end px-3">
                <h6 className="small fw-bold text-danger border-bottom pb-2 mb-3">KAYNAK HESAP</h6>
                <AppSelect label="Kasa" options={cashes} value={formData.fromCashId || ""} onChange={(val) => setFormData({...formData, fromCashId: val, fromBankId: null})} isSearchable />
                <div className="text-center my-2 small text-muted font-monospace">-- VEYA --</div>
                <AppSelect label="Banka" options={banks} value={formData.fromBankId || ""} onChange={(val) => setFormData({...formData, fromBankId: val, fromCashId: null})} isSearchable />
              </div>
              <div className="col-md-6 px-3">
                <h6 className="small fw-bold text-success border-bottom pb-2 mb-3">HEDEF HESAP</h6>
                <AppSelect label="Kasa" options={cashes} value={formData.toCashId || ""} onChange={(val) => setFormData({...formData, toCashId: val, toBankId: null})} isSearchable />
                <div className="text-center my-2 small text-muted font-monospace">-- VEYA --</div>
                <AppSelect label="Banka" options={banks} value={formData.toBankId || ""} onChange={(val) => setFormData({...formData, toBankId: val, toCashId: null})} isSearchable />
              </div>
            </>
          )}

          {modalType === 'opening' && (
            <>
              <div className="col-md-12">
                <label className="form-label small fw-bold">Açılış Yapılacak Hesap Tipi</label>
                <select className="form-select form-select-sm" value={formData.accountType} onChange={(e:any) => setFormData({...formData, accountType: e.target.value, cashId: null, bankId: null, customerId: null})}>
                  <option value="cash">Kasa</option>
                  <option value="bank">Banka</option>
                  <option value="customer">Cari (Müşteri/Tedarikçi)</option>
                </select>
              </div>
              <div className="col-12">
                {formData.accountType === 'cash' && <AppSelect label="Kasa Seçimi" options={cashes} value={formData.cashId || ""} onChange={(val) => setFormData({...formData, cashId: val})} isSearchable />}
                {formData.accountType === 'bank' && <AppSelect label="Banka Seçimi" options={banks} value={formData.bankId || ""} onChange={(val) => setFormData({...formData, bankId: val})} isSearchable />}
                {formData.accountType === 'customer' && <AppSelect label="Cari Seçimi" options={getFilteredCustomers()} value={formData.customerId || ""} onChange={(val) => setFormData({...formData, customerId: val})} isSearchable />}
              </div>
              <div className="col-md-12 bg-light p-3 rounded-2">
                <div className="form-check form-switch p-0 ms-4">
                  <input className="form-check-input" type="checkbox" id="openingType" checked={formData.isDebit} onChange={(e) => setFormData({...formData, isDebit: e.target.checked})} />
                  <label className="form-check-label small fw-bold ms-2" htmlFor="openingType">
                    {formData.isDebit ? '🔵 BORÇ (Bakiyeyi Artırır)' : '🔴 ALACAK (Bakiyeyi Azaltır)'}
                  </label>
                </div>
              </div>
            </>
          )}

          <div className="col-12">
            <label className="form-label small fw-bold">Açıklama / Notlar</label>
            <textarea className="form-control form-control-sm" rows={2} value={formData.description} onChange={(e) => setFormData({...formData, description: e.target.value})} placeholder="İşlemle ilgili detaylı bilgi..."></textarea>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Finance;