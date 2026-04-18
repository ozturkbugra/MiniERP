import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect, { type SelectOption } from '../components/Common/AppSelect';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { useSettingsStore } from '../store/useSettingsStore';
import { APP_PERMISSIONS } from '../constants/permissions';

// 🚀 Backend Enum ve Status Karşılıkları
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

  // Bağımlılıklar
  const [customers, setCustomers] = useState<SelectOption[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<SelectOption[]>([]);

  // Form State (Header)
  const [formData, setFormData] = useState({
    orderNumber: "",
    orderDate: new Date().toISOString().split('T')[0],
    description: "",
    type: 2,
    customerId: "",
    warehouseId: defaultWarehouseId || "",
    orderLines: [] as any[]
  });

  // Kalem State (Lines)
  const [currentLine, setCurrentLine] = useState({
    productId: "",
    warehouseId: defaultWarehouseId || "",
    quantity: 1,
    unitPrice: 0
  });

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
      if (cRes.data.isSuccess) setCustomers(cRes.data.data.map((x: any) => ({ label: x.name || "İsimsiz Cari", value: x.id })));
      if (pRes.data.isSuccess) setProducts(pRes.data.data);
      if (wRes.data.isSuccess) setWarehouses(wRes.data.data.map((x: any) => ({ label: x.name || "İsimsiz Depo", value: x.id })));
    } catch (err) {
      console.error("Veri çekme hatası:", err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  // 🚀 Detayları Getir
  const handleViewDetails = async (id: string) => {
    try {
      const res = await api.get(`/Orders/${id}`);
      if (res.data.isSuccess) {
        setSelectedOrder(res.data.data);
        setShowDetailsModal(true);
      }
    } catch (err) { alert("Detaylar yüklenemedi."); }
  };

  // 🚀 Kalem Ekleme
  const addLine = () => {
    if (!currentLine.productId || currentLine.quantity <= 0) return;
    const product = products.find(p => p.id === currentLine.productId);
    const warehouseLabel = warehouses.find(w => w.value === currentLine.warehouseId)?.label || "Bilinmeyen Depo";
    
    const newLine = {
      ...currentLine,
      productName: product?.name || "Bilinmeyen Ürün",
      warehouseName: warehouseLabel,
      lineTotal: currentLine.quantity * currentLine.unitPrice
    };

    setFormData(prev => ({ ...prev, orderLines: [...prev.orderLines, newLine] }));
    setCurrentLine({ productId: "", warehouseId: defaultWarehouseId || "", quantity: 1, unitPrice: 0 });
  };

  const handleSave = async () => {
    if (formData.orderLines.length === 0) { alert("En az bir kalem eklemelisiniz!"); return; }
    try {
      const response = await api.post("/Orders", formData);
      if (response.data.isSuccess) {
        setShowCreateModal(false);
        fetchData();
        setFormData({ ...formData, orderLines: [] }); // Formu temizle
      }
    } catch (err) { alert("Hata oluştu"); }
  };

  return (
    <div className="page-orders animate__animated animate__fadeIn">
      <DataTable<any>
        title="Sipariş Yönetimi"
        description="Müşteri ve tedarikçi siparişlerini yönetin, onaylayın veya iptal edin."
        data={orders}
        onAdd={() => {
            setFormData({...formData, orderNumber: "ORD-" + Date.now().toString().slice(-6), orderLines: []});
            setShowCreateModal(true);
        }}
        columns={[
          { header: "SİPARİŞ NO", accessor: (o) => <span className="fw-bold text-info">{o.orderNumber}</span> },
          { header: "TARİH", accessor: (o) => new Date(o.orderDate).toLocaleDateString('tr-TR') },
          { header: "MÜŞTERİ / TEDARİKÇİ", accessor: (o) => o.customerName },
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
              <div className="d-flex justify-content-end gap-1">
                {/* 👁️ Görüntüle */}
                <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => handleViewDetails(o.id)} title="Detayları Gör">
                  <i className="bi bi-eye fs-6"></i>
                </button>

                {/* ✅ Onayla */}
                {o.status === "Pending" && hasPermission(APP_PERMISSIONS.Orders.Approve) && (
                  <button className="btn btn-sm btn-success px-3" onClick={async () => {
                    if(!confirm("Siparişi onaylıyor musunuz? Stoklar rezerve edilecektir.")) return;
                    const res = await api.post(`/Orders/${o.id}/approve`);
                    if (res.data.isSuccess) fetchData(); else alert(res.data.message);
                  }}>Onayla</button>
                )}

                {/* ❌ İptal Et */}
                {o.status === "Pending" && hasPermission(APP_PERMISSIONS.Orders.Cancel) && (
                  <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={async () => {
                    if(!confirm("Siparişi iptal etmek istediğinize emin misiniz?")) return;
                    const res = await api.post(`/Orders/${o.id}/cancel`);
                    if (res.data.isSuccess) fetchData(); else alert(res.data.message);
                  }} title="Siparişi İptal Et">
                    <i className="bi bi-x-circle fs-6"></i>
                  </button>
                )}
              </div>
            )
          }
        ]}
      />

      {/* 🚀 MODAL 1: SİPARİŞ OLUŞTURMA */}
      <AppModal show={showCreateModal} title="Yeni Sipariş Oluştur" onClose={() => setShowCreateModal(false)} onSave={handleSave} size="xl">
        <div className="row g-3">
          <div className="col-md-3">
            <label className="form-label small fw-bold">Fiş No</label>
            <input type="text" className="form-control form-control-sm" value={formData.orderNumber} onChange={(e) => setFormData({...formData, orderNumber: e.target.value})} />
          </div>
          <div className="col-md-3">
            <label className="form-label small fw-bold">Tarih</label>
            <input type="date" className="form-control form-control-sm" value={formData.orderDate} onChange={(e) => setFormData({...formData, orderDate: e.target.value})} />
          </div>
          <div className="col-md-3">
            <label className="form-label small fw-bold">Sipariş Tipi</label>
            <select className="form-select form-select-sm" value={formData.type} onChange={(e) => setFormData({...formData, type: parseInt(e.target.value)})}>
              {ORDER_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
            </select>
          </div>
          <div className="col-md-3">
            <AppSelect label="Cari Hesap" options={customers} value={formData.customerId} onChange={(val) => setFormData({...formData, customerId: val})} isSearchable />
          </div>

          <div className="col-12 mt-2">
            <div className="p-3 rounded border border-secondary-subtle">
              <h6 className="small fw-bold mb-3">Sipariş Kalemi Ekle</h6>
              <div className="row g-2 align-items-end">
                <div className="col-md-4">
                  <AppSelect 
                    label="Ürün" 
                    options={products.map(p => ({ label: p.name, value: p.id }))} 
                    value={currentLine.productId} 
                    onChange={(val) => {
                      const price = products.find(p => p.id === val)?.defaultPrice || 0;
                      setCurrentLine({...currentLine, productId: val, unitPrice: price});
                    }} 
                    isSearchable 
                  />
                </div>
                <div className="col-md-3">
                  <AppSelect label="Depo" options={warehouses} value={currentLine.warehouseId} onChange={(val) => setCurrentLine({...currentLine, warehouseId: val})} />
                </div>
                <div className="col-md-2">
                  <label className="form-label small">Miktar</label>
                  <input type="number" className="form-control form-control-sm" value={currentLine.quantity} onChange={(e) => setCurrentLine({...currentLine, quantity: parseFloat(e.target.value)})} />
                </div>
                <div className="col-md-2">
                  <label className="form-label small">Fiyat</label>
                  <input type="number" className="form-control form-control-sm" value={currentLine.unitPrice} onChange={(e) => setCurrentLine({...currentLine, unitPrice: parseFloat(e.target.value)})} />
                </div>
                <div className="col-md-1">
                  <button type="button" className="btn btn-primary btn-sm w-100" onClick={addLine}><i className="bi bi-plus-lg"></i></button>
                </div>
              </div>
            </div>
          </div>

          <div className="col-12">
            <table className="table table-sm mt-2">
              <thead><tr className="small text-muted"><th>Ürün Adı</th><th>Depo</th><th className="text-end">Miktar</th><th className="text-end">Fiyat</th><th className="text-end">Toplam</th><th></th></tr></thead>
              <tbody>
                {formData.orderLines.map((l, i) => (
                  <tr key={i} className="align-middle">
                    <td>{l.productName}</td>
                    <td><span className="badge bg-secondary-subtle text-secondary small">{l.warehouseName}</span></td>
                    <td className="text-end">{l.quantity}</td>
                    <td className="text-end">{l.unitPrice} ₺</td>
                    <td className="text-end text-success fw-bold">{l.lineTotal.toLocaleString('tr-TR')} ₺</td>
                    <td className="text-end">
                      <button className="btn btn-sm text-danger p-0" onClick={() => setFormData({...formData, orderLines: formData.orderLines.filter((_, idx) => idx !== i)})}>
                        <i className="bi bi-trash"></i>
                      </button>
                    </td>
                  </tr>
                ))}
                {formData.orderLines.length === 0 && <tr><td colSpan={6} className="text-center text-muted py-3">Henüz kalem eklenmedi.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      </AppModal>

      {/* 🚀 MODAL 2: SİPARİŞ DETAYLARI */}
      <AppModal show={showDetailsModal} title={`Sipariş Detayı: ${selectedOrder?.orderNumber}`} onClose={() => setShowDetailsModal(false)} size="lg" saveButton={false}>
        {selectedOrder && (
          <div className="row g-3">
            <div className="col-md-6">
                <div className="small text-muted mb-1">Müşteri / Tedarikçi</div>
                <div className="fw-bold fs-5 text-info">{selectedOrder.customerName}</div>
            </div>
            <div className="col-md-3">
                <div className="small text-muted mb-1">İşlem Tarihi</div>
                <div className="fw-medium">{new Date(selectedOrder.orderDate).toLocaleDateString('tr-TR')}</div>
            </div>
            <div className="col-md-3">
                <div className="small text-muted mb-1">Güncel Durum</div>
                <span className={`badge border ${ORDER_STATUS[selectedOrder.status]?.badge}`}>{ORDER_STATUS[selectedOrder.status]?.label}</span>
            </div>
            <div className="col-12"><hr className="opacity-10" /></div>
            <div className="col-12">
              <h6 className="fw-bold mb-3 text-primary">Sipariş Kalemleri</h6>
              <table className="table table-sm border-secondary-subtle">
                <thead>
                  <tr className="small text-muted">
                    <th>Ürün</th>
                    <th>Depo</th>
                    <th className="text-end">Miktar</th>
                    <th className="text-end">Fiyat</th>
                    <th className="text-end">Toplam</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedOrder.lines.map((line: any, index: number) => (
                    <tr key={index} className="align-middle">
                      <td>{line.productName}</td>
                      <td><span className="badge bg-secondary-subtle text-secondary small">{line.warehouseName}</span></td>
                      <td className="text-end fw-bold">{line.quantity}</td>
                      <td className="text-end">{line.unitPrice.toLocaleString('tr-TR')} ₺</td>
                      <td className="text-end text-success fw-bold">{(line.quantity * line.unitPrice).toLocaleString('tr-TR')} ₺</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan={4} className="text-end fw-bold pt-3">GENEL TOPLAM:</td>
                    <td className="text-end fw-bold text-info fs-5 pt-3">
                      {selectedOrder.lines.reduce((acc: number, curr: any) => acc + (curr.quantity * curr.unitPrice), 0).toLocaleString('tr-TR')} ₺
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
            <div className="col-12 border-top pt-2">
                <div className="small text-muted">Kayıt Sahibi: <span className="">{selectedOrder.createdByName}</span></div>
            </div>
          </div>
        )}
      </AppModal>
    </div>
  );
};

export default Orders;