import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect, { type SelectOption } from '../components/Common/AppSelect';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { useSettingsStore } from '../store/useSettingsStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 Durum Badgeleri
const ORDER_STATUS: Record<string, { label: string, badge: string }> = {
  "Pending": { label: "Beklemede", badge: "bg-warning-subtle text-warning border-warning" },
  "Approved": { label: "Onaylandı", badge: "bg-success-subtle text-success border-success" },
  "Invoiced": { label: "Faturalandı", badge: "bg-info-subtle text-info border-info" },
  "Canceled": { label: "İptal Edildi", badge: "bg-danger-subtle text-danger border-danger" }
};

const ORDER_TYPES = [
  { label: "Satış Siparişi", value: 2 },
  { label: "Satınalma Siparişi", value: 1 }
];

const Orders = () => {
  const { hasPermission } = useAuthStore();
  const { defaultWarehouseId } = useSettingsStore();

  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Modal Kontrolleri
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [selectedOrder, setSelectedOrder] = useState<any>(null);

  // 🚀 Para formatı için ara state
  const [displayPrice, setDisplayPrice] = useState<string>("");

  // Bağımlılıklar
  const [rawCustomers, setRawCustomers] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<SelectOption[]>([]);

  // Form State
  const [formData, setFormData] = useState({
    orderNumber: "",
    orderDate: new Date().toISOString().split('T')[0],
    description: "",
    type: 2, 
    customerId: "",
    warehouseId: defaultWarehouseId || "",
    orderLines: [] as any[]
  });

  const [currentLine, setCurrentLine] = useState({
    productId: "",
    warehouseId: defaultWarehouseId || "",
    quantity: 1,
    unitPrice: 0
  });

  // 🛠️ FORMATLAMA YARDIMCILARI
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

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [oRes, cRes, pRes, wRes] = await Promise.all([
        api.get("/Orders"),
        api.get("/Customers"),
        api.get("/Products"),
        api.get("/Warehouses")
      ]);
      if (oRes.data.isSuccess) setOrders(oRes.data.data);
      if (cRes.data.isSuccess) setRawCustomers(cRes.data.data);
      if (pRes.data.isSuccess) setProducts(pRes.data.data);
      if (wRes.data.isSuccess) setWarehouses(wRes.data.data.map((x: any) => ({ label: x.name || "Depo", value: x.id })));
    } catch (err) { console.error(err); } finally { setLoading(false); }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  // 🚀 DİNAMİK CARİ FİLTRELEME (İstediğin mantık burası aga)
  const getFilteredCustomers = (): SelectOption[] => {
    return rawCustomers
      .filter(c => {
        // Satınalma (1) seçiliyse: Tedarikçi (Supplier/2) veya Her İkisi (Both/3)
        if (formData.type === 1) return c.type === "Supplier" || c.type === "Both" || c.type === 2 || c.type === 3;
        // Satış (2) seçiliyse: Alıcı (Buyer/1) veya Her İkisi (Both/3)
        if (formData.type === 2) return c.type === "Buyer" || c.type === "Both" || c.type === 1 || c.type === 3;
        return true;
      })
      .map(c => ({ label: c.name || "İsimsiz", value: c.id }));
  };

  const handleViewDetails = async (id: string) => {
    try {
      const res = await api.get(`/Orders/${id}`);
      if (res.data.isSuccess) {
        setSelectedOrder(res.data.data);
        setShowDetailsModal(true);
      }
    } catch (err) { alert("Detaylar yüklenemedi."); }
  };

  const handleProductChange = (val: string) => {
    const product = products.find(p => p.id === val);
    const price = product?.defaultPrice || 0;
    setCurrentLine(prev => ({ ...prev, productId: val, unitPrice: price }));
    setDisplayPrice(price > 0 ? maskCurrency(price.toString().replace(".", ",")) : "");
  };

  const addLine = () => {
    if (!currentLine.productId || currentLine.quantity <= 0) return;
    const product = products.find(p => p.id === currentLine.productId);
    const warehouseLabel = warehouses.find(w => w.value === currentLine.warehouseId)?.label || "-";
    
    const newLine = {
      ...currentLine,
      productName: product?.name || "-",
      warehouseName: warehouseLabel,
      lineTotal: currentLine.quantity * currentLine.unitPrice
    };

    setFormData(prev => ({ ...prev, orderLines: [...prev.orderLines, newLine] }));
    setCurrentLine({ productId: "", warehouseId: defaultWarehouseId || "", quantity: 1, unitPrice: 0 });
    setDisplayPrice("");
  };

  const handleSave = async () => {
    if (formData.orderLines.length === 0) { alert("En az bir kalem eklemelisiniz!"); return; }
    try {
      const response = await api.post("/Orders", formData);
      if (response.data.isSuccess) {
        setShowCreateModal(false);
        fetchData();
        setFormData({ ...formData, orderLines: [] });
      }
    } catch (err) { alert("Hata oluştu"); }
  };

  return (
    <div className="page-orders animate__animated animate__fadeIn">
      <DataTable<any>
        title="Sipariş Yönetimi"
        description="Satış ve satınalma siparişlerini takip edin."
        data={orders}
        onAdd={() => {
            setFormData({...formData, orderNumber: "ORD-" + Date.now().toString().slice(-6), orderLines: []});
            setShowCreateModal(true);
        }}
        columns={[
          { header: "SİPARİŞ NO", accessor: (o) => <span className="fw-bold text-info">{o.orderNumber}</span> },
          { header: "TARİH", accessor: (o) => new Date(o.orderDate).toLocaleDateString('tr-TR') },
          { header: "CARİ HESAP", accessor: (o) => o.customerName },
          { 
            header: "DURUM", 
            accessor: (o) => {
              const status = ORDER_STATUS[o.status] || { label: o.status, badge: "bg-secondary" };
              return <span className={`badge border ${status.badge}`}>{status.label}</span>;
            } 
          },
          {
            header: "İŞLEMLER",
            className: "text-end",
            accessor: (o) => (
              <div className="d-flex justify-content-end gap-2">
                <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => handleViewDetails(o.id)} title="Görüntüle">
                  <i className="bi bi-eye fs-6"></i>
                </button>
                {o.status === "Pending" && hasPermission(APP_PERMISSIONS.Orders.Approve) && (
                  <button className="btn btn-sm btn-success px-3" onClick={async () => {
                      if(confirm("Siparişi onaylıyor musunuz?")) {
                          const res = await api.post(`/Orders/${o.id}/approve`);
                          if (res.data.isSuccess) fetchData();
                      }
                  }}>Onayla</button>
                )}
              </div>
            )
          }
        ]}
      />

      {/* 🚀 MODAL 1: OLUŞTURMA */}
      <AppModal show={showCreateModal} title="Yeni Sipariş Fişi" onClose={() => setShowCreateModal(false)} onSave={handleSave} size="xl">
        <div className="row g-3">
          <div className="col-md-3">
            <label className="form-label small fw-bold text-primary">Sipariş Tipi</label>
            <select className="form-select form-select-sm border-primary" value={formData.type} onChange={(e) => setFormData({...formData, type: parseInt(e.target.value), customerId: ""})}>
              {ORDER_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>
          <div className="col-md-3">
            <label className="form-label small fw-bold">Fiş No</label>
            <input type="text" className="form-control form-control-sm" value={formData.orderNumber} onChange={(e) => setFormData({...formData, orderNumber: e.target.value})} />
          </div>
          <div className="col-md-3">
            <label className="form-label small fw-bold">Tarih</label>
            <input type="date" className="form-control form-control-sm" value={formData.orderDate} onChange={(e) => setFormData({...formData, orderDate: e.target.value})} />
          </div>
          <div className="col-md-3">
            <AppSelect label={formData.type === 1 ? "Tedarikçi" : "Müşteri"} options={getFilteredCustomers()} value={formData.customerId} onChange={(val) => setFormData({...formData, customerId: val})} isSearchable />
          </div>

          <div className="col-12 mt-2">
            <div className="p-3 rounded border">
              <h6 className="small fw-bold mb-3">Yeni Kalem Ekle</h6>
              <div className="row g-2 align-items-end">
                <div className="col-md-4">
                  <AppSelect label="Ürün" options={products.map(p => ({ label: p.name, value: p.id }))} value={currentLine.productId} onChange={handleProductChange} isSearchable />
                </div>
                <div className="col-md-3">
                  <AppSelect label="Depo" options={warehouses} value={currentLine.warehouseId} onChange={(val) => setCurrentLine({...currentLine, warehouseId: val})} />
                </div>
                <div className="col-md-2">
                  <label className="form-label small">Miktar</label>
                  <input type="number" className="form-control form-control-sm" value={currentLine.quantity} onChange={(e) => setCurrentLine({...currentLine, quantity: parseFloat(e.target.value)})} />
                </div>
                <div className="col-md-2">
                  <label className="form-label small">Fiyat (₺)</label>
                  <input type="text" className="form-control form-control-sm text-end fw-bold" value={displayPrice} placeholder="0,00"
                    onChange={(e) => {
                        const masked = maskCurrency(e.target.value);
                        setDisplayPrice(masked);
                        setCurrentLine(p => ({ ...p, unitPrice: unmaskPrice(masked) }));
                    }} onFocus={(e) => e.target.select()}
                  />
                </div>
                <div className="col-md-1">
                  <button type="button" className="btn btn-primary btn-sm w-100" onClick={addLine}><i className="bi bi-plus-lg"></i></button>
                </div>
              </div>
            </div>
          </div>

          <div className="col-12">
            <table className="table table-sm mt-2 align-middle">
              <thead><tr className="small text-muted border-bottom"><th>Ürün</th><th>Depo</th><th className="text-end">Miktar</th><th className="text-end">Fiyat</th><th className="text-end">Toplam</th><th className="text-center">Sil</th></tr></thead>
              <tbody>
                {formData.orderLines.map((l, i) => (
                  <tr key={i} className="border-bottom-0">
                    <td>{l.productName}</td>
                    <td><span className="badge bg-secondary-subtle text-secondary small">{l.warehouseName}</span></td>
                    <td className="text-end">{l.quantity}</td>
                    <td className="text-end">{l.unitPrice.toLocaleString('tr-TR')} ₺</td>
                    <td className="text-end text-success fw-bold">{l.lineTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                    <td className="text-center">
                      <button className="btn btn-sm text-danger" onClick={() => setFormData({...formData, orderLines: formData.orderLines.filter((_, idx) => idx !== i)})}>
                        <i className="bi bi-trash"></i>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
              {formData.orderLines.length > 0 && (
                <tfoot>
                    <tr className="table-light"><td colSpan={4} className="text-end fw-bold">TOPLAM:</td>
                    <td className="text-end fw-bold text-primary">{formData.orderLines.reduce((acc, curr) => acc + curr.lineTotal, 0).toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                    <td></td></tr>
                </tfoot>
              )}
            </table>
          </div>
        </div>
      </AppModal>

      {/* 🚀 MODAL 2: DETAYLAR (Görüntüle) */}
      <AppModal show={showDetailsModal} title={`Sipariş Detayı: ${selectedOrder?.orderNumber}`} onClose={() => setShowDetailsModal(false)} size="lg" saveButton={false}>
        {selectedOrder && (
          <div className="row g-3">
            <div className="col-md-6">
                <div className="small text-muted">Müşteri / Tedarikçi</div>
                <div className="fw-bold fs-5 text-info">{selectedOrder.customerName}</div>
            </div>
            <div className="col-md-3">
                <div className="small text-muted">İşlem Tarihi</div>
                <div className="fw-medium">{new Date(selectedOrder.orderDate).toLocaleDateString('tr-TR')}</div>
            </div>
            <div className="col-md-3">
                <div className="small text-muted">Durum</div>
                <span className={`badge border ${ORDER_STATUS[selectedOrder.status]?.badge}`}>{ORDER_STATUS[selectedOrder.status]?.label}</span>
            </div>
            <div className="col-12"><hr className="opacity-10" /></div>
            <div className="col-12">
              <table className="table table-sm border">
                <thead className="table-light"><tr className="small text-muted"><th>Ürün</th><th>Depo</th><th className="text-end">Miktar</th><th className="text-end">Fiyat</th><th className="text-end">Toplam</th></tr></thead>
                <tbody>
                  {(selectedOrder.lines || selectedOrder.orderLines || []).map((line: any, index: number) => (
                    <tr key={index}>
                      <td>{line.productName}</td>
                      <td>{line.warehouseName}</td>
                      <td className="text-end fw-bold">{line.quantity}</td>
                      <td className="text-end">{line.unitPrice.toLocaleString('tr-TR')} ₺</td>
                      <td className="text-end text-success fw-bold">{(line.quantity * line.unitPrice).toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="fw-bold"><td colSpan={4} className="text-end">GENEL TOPLAM:</td>
                  <td className="text-end text-info fs-5">{ (selectedOrder.lines || selectedOrder.orderLines || []).reduce((acc: any, curr: any) => acc + (curr.quantity * curr.unitPrice), 0).toLocaleString('tr-TR', { minimumFractionDigits: 2 }) } ₺</td></tr>
                </tfoot>
              </table>
            </div>
          </div>
        )}
      </AppModal>
    </div>
  );
};

export default Orders;