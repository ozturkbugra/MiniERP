import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect from '../components/Common/AppSelect';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { useSettingsStore } from '../store/useSettingsStore';
import { APP_PERMISSIONS } from '../constants/permissions';

const TRANSACTION_TYPES: Record<number, { label: string, badgeClass: string }> = {
  1: { label: "Giriş (Alım)", badgeClass: "bg-success-subtle text-success border-success-subtle" },
  2: { label: "Çıkış (Satış)", badgeClass: "bg-danger-subtle text-danger border-danger-subtle" },
  3: { label: "Devir/Açılış", badgeClass: "bg-primary-subtle text-primary border-primary-subtle" }
};

const PAYMENT_TYPES = [
  { label: "Veresiye (Cari)", value: 1 },
  { label: "Nakit (Kasa)", value: 2 },
  { label: "Banka (EFT/Havale)", value: 3 }
];

const StockTransactions = () => {
  const { hasPermission } = useAuthStore();
  const { defaultWarehouseId } = useSettingsStore();
  
  const [transactions, setTransactions] = useState<any[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);

  const [rawProducts, setRawProducts] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]);
  const [rawCustomers, setRawCustomers] = useState<any[]>([]);
  const [cashes, setCashes] = useState<any[]>([]);
  const [banks, setBanks] = useState<any[]>([]);

  const [formData, setFormData] = useState({
    documentNo: "",
    transactionDate: new Date().toISOString().split('T')[0],
    quantity: 1,
    unitPrice: 0,
    description: "",
    type: 1, 
    productId: "",
    warehouseId: defaultWarehouseId || "",
    customerId: null as string | null,
    paymentType: 1,
    cashId: null as string | null,
    bankId: null as string | null
  });

  const fetchTransactions = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/StockTransactions");
      if (response.data.isSuccess) setTransactions(response.data.data);
    } catch (error) { console.error("Hata:", error); }
    finally { setLoading(false); }
  }, []);

  const fetchDependencies = useCallback(async () => {
    const [pRes, wRes, cRes, cashRes, bankRes] = await Promise.all([
      api.get("/Products"), api.get("/Warehouses"), api.get("/Customers"), api.get("/Cashes"), api.get("/Banks")
    ]);
    
    if (pRes.data.isSuccess) {
      setRawProducts(pRes.data.data);
      setProducts(pRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
    }
    if (wRes.data.isSuccess) setWarehouses(wRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
    if (cRes.data.isSuccess) setRawCustomers(cRes.data.data);
    if (cashRes.data.isSuccess) setCashes(cashRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
    if (bankRes.data.isSuccess) setBanks(bankRes.data.data.map((x: any) => ({ label: x.bankName, value: x.id })));
  }, []);

  useEffect(() => {
    fetchTransactions();
    fetchDependencies();
  }, [fetchTransactions, fetchDependencies]);

  const getFilteredCustomers = () => {
    return rawCustomers
      .filter(c => {
        if (formData.type === 1) return c.type === "Supplier" || c.type === "Both";
        if (formData.type === 2) return c.type === "Buyer" || c.type === "Both";
        return true; 
      })
      .map(c => ({ label: c.name, value: c.id }));
  };

  const handleProductChange = (productId: string) => {
    const selectedProduct = rawProducts.find(p => p.id === productId);
    setFormData(prev => ({ ...prev, productId, unitPrice: selectedProduct ? selectedProduct.defaultPrice : 0 }));
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({ ...prev, [name]: type === 'number' ? parseFloat(value) : value }));
  };

  const handleSave = async () => {
    try {
      const response = await api.post("/StockTransactions", formData);
      if (response.data.isSuccess) {
        setShowModal(false);
        fetchTransactions();
      }
    } catch (error) { console.error("Kayıt başarısız"); }
  };

  const openCreateModal = () => {
    setFormData({
      ...formData,
      documentNo: "ST-" + Date.now().toString().slice(-6),
      warehouseId: defaultWarehouseId || "",
    });
    setShowModal(true);
  };

  return (
    <div className="page-stock-transactions">
      <DataTable<any>
        title="Stok Hareketleri"
        description="Giriş, çıkış ve devir/açılış işlemlerini tek ekrandan yönetin."
        columns={[
          { header: "TARİH", accessor: (t) => new Date(t.transactionDate).toLocaleDateString('tr-TR') },
          { header: "BELGE NO", accessor: (t) => t.documentNo },
          { header: "ÜRÜN", accessor: (t) => t.productName },
          { 
            header: "TİP", 
            accessor: (t) => {
              const info = TRANSACTION_TYPES[t.type] || { label: t.typeName, badgeClass: "bg-secondary" };
              return <span className={`badge border ${info.badgeClass}`}>{info.label}</span>;
            }
          },
          { header: "MİKTAR", accessor: (t) => <span className="fw-bold">{t.quantity}</span> },
          { header: "TOPLAM", accessor: (t) => <span className="text-success fw-bold">{t.totalPrice.toLocaleString('tr-TR')} ₺</span> }
        ]}
        data={transactions}
        onAdd={hasPermission(APP_PERMISSIONS.StockTransactions.Create) ? openCreateModal : undefined}
        addText="Yeni İşlem"
      />

      <AppModal show={showModal} title={formData.type === 3 ? "Yeni Stok Açılış Fişi" : "Yeni Stok Hareketi"} onClose={() => setShowModal(false)} onSave={handleSave} size="lg">
        <div className="row g-3">
          {/* Hareket Tipi Seçimi */}
          <div className="col-md-4">
            <label className="form-label small fw-bold">Hareket Tipi</label>
            <select name="type" value={formData.type} onChange={(e) => setFormData({...formData, type: parseInt(e.target.value), customerId: null})} className="form-select form-select-sm border-primary">
              <option value={1}>Giriş (Alım)</option>
              <option value={2}>Çıkış (Satış)</option>
              <option value={3}>Devir / Açılış</option>
            </select>
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">Fiş No</label>
            <input type="text" name="documentNo" value={formData.documentNo} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">İşlem Tarihi</label>
            <input type="date" name="transactionDate" value={formData.transactionDate} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>

          <hr className="my-2 opacity-10" />

          {/* Stok Bilgileri */}
          <div className="col-md-6">
            <AppSelect label="Ürün Seçimi" options={products} value={formData.productId} onChange={handleProductChange} isSearchable={true} />
          </div>
          <div className="col-md-6">
            <AppSelect label="İşlem Deposu" options={warehouses} value={formData.warehouseId} onChange={(val) => setFormData({...formData, warehouseId: val})} isSearchable={true} />
          </div>

          <div className="col-md-4">
            <label className="form-label small fw-bold">Miktar</label>
            <input type="number" name="quantity" value={formData.quantity} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">Birim Fiyat (₺)</label>
            <input type="number" name="unitPrice" value={formData.unitPrice} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">Toplam</label>
            <div className="form-control form-control-sm bg-dark-subtle fw-bold">{(formData.quantity * formData.unitPrice).toLocaleString('tr-TR')} ₺</div>
          </div>

          {/* 🚀 AÇILIŞ FİŞİ DEĞİLSE CARİ VE ÖDEME ALANLARINI GÖSTER */}
          {formData.type !== 3 && (
            <>
              <hr className="my-2 opacity-10" />
              <div className="col-md-6">
                <AppSelect 
                    label={formData.type === 1 ? "Tedarikçi Seçiniz" : "Müşteri Seçiniz"} 
                    options={getFilteredCustomers()} 
                    value={formData.customerId || ""} 
                    onChange={(val) => setFormData({...formData, customerId: val})} 
                    isSearchable={true} 
                />
              </div>
              <div className="col-md-6">
                <label className="form-label small fw-bold">Ödeme Yöntemi</label>
                <select 
                    name="paymentType" 
                    value={formData.paymentType} 
                    onChange={(e) => setFormData({...formData, paymentType: parseInt(e.target.value), cashId: null, bankId: null})} 
                    className="form-select form-select-sm"
                >
                  {PAYMENT_TYPES.map(p => <option key={p.value} value={p.value}>{p.label}</option>)}
                </select>
              </div>

              {formData.paymentType === 2 && (
                <div className="col-12 animate__animated animate__fadeInDown">
                  <AppSelect label="👉 Nakit Hareketinin İşleneceği KASA" options={cashes} value={formData.cashId || ""} onChange={(val) => setFormData({...formData, cashId: val})} isSearchable={true} />
                </div>
              )}
              {formData.paymentType === 3 && (
                <div className="col-12 animate__animated animate__fadeInDown">
                  <AppSelect label="👉 Banka Hareketinin İşleneceği HESAP" options={banks} value={formData.bankId || ""} onChange={(val) => setFormData({...formData, bankId: val})} isSearchable={true} />
                </div>
              )}
            </>
          )}

          <div className="col-12">
            <label className="form-label small fw-bold">Açıklama</label>
            <textarea name="description" value={formData.description} onChange={handleInputChange} className="form-control form-control-sm" rows={2} placeholder={formData.type === 3 ? "Stok Açılış Fişi..." : ""}></textarea>
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default StockTransactions;