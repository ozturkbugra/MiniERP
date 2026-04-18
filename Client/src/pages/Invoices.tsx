import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect, { type SelectOption } from '../components/Common/AppSelect';
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { useSettingsStore } from '../store/useSettingsStore';
import { APP_PERMISSIONS } from '../constants/permissions';

const INVOICE_STATUS: Record<string, { label: string, badge: string }> = {
  "Draft": { label: "Taslak", badge: "bg-secondary-subtle text-secondary border-secondary" },
  "Approved": { label: "Onaylandı", badge: "bg-success-subtle text-success border-success" },
  "Canceled": { label: "İptal Edildi", badge: "bg-danger-subtle text-danger border-danger" }
};

const INVOICE_TYPES = [
  { label: "Satış Faturası", value: 2 },
  { label: "Alım Faturası", value: 1 },
  { label: "Satış İade", value: 4 },
  { label: "Alış İade", value: 3 }
];

const Invoices = () => {
  const { hasPermission } = useAuthStore();
  const { defaultWarehouseId } = useSettingsStore();

  const [invoices, setInvoices] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  
  const [selectedInvoice, setSelectedInvoice] = useState<any>(null);

  const [rawCustomers, setRawCustomers] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<SelectOption[]>([]);
  const [cashes, setCashes] = useState<SelectOption[]>([]);
  const [banks, setBanks] = useState<SelectOption[]>([]);
  const [approvedOrders, setApprovedOrders] = useState<SelectOption[]>([]);

  const [formData, setFormData] = useState({
    type: 2, 
    invoiceDate: new Date().toISOString().split('T')[0],
    customerId: "",
    warehouseId: defaultWarehouseId || "",
    orderId: null as string | null,
    details: [] as any[]
  });

  const [approveData, setApproveData] = useState({
    id: "",
    paymentType: 1, 
    cashId: null as string | null,
    bankId: null as string | null
  });

  const [currentLine, setCurrentLine] = useState({
    productId: "",
    warehouseId: defaultWarehouseId || "", // 🚀 Satır bazlı depo default geliyor
    quantity: 1,
    unitPrice: 0,
    discountRate: 0,
    vatRate: 20
  });

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [iRes, cRes, pRes, wRes, cashRes, bankRes, oRes] = await Promise.all([
        api.get("/Invoices"),
        api.get("/Customers"),
        api.get("/Products"),
        api.get("/Warehouses"),
        api.get("/Cashes"),
        api.get("/Banks"),
        api.get("/Orders")
      ]);

      if (iRes.data.isSuccess) setInvoices(iRes.data.data);
      if (cRes.data.isSuccess) setRawCustomers(cRes.data.data);
      if (pRes.data.isSuccess) setProducts(pRes.data.data);
      if (wRes.data.isSuccess) setWarehouses(wRes.data.data.map((x: any) => ({ label: x.name || "Depo", value: x.id })));
      if (cashRes.data.isSuccess) setCashes(cashRes.data.data.map((x: any) => ({ label: x.name || "Kasa", value: x.id })));
      if (bankRes.data.isSuccess) setBanks(bankRes.data.data.map((x: any) => ({ label: x.bankName || "Banka", value: x.id })));
      
      if (oRes.data.isSuccess) {
        const approved = oRes.data.data
          .filter((o: any) => o.status === "Approved")
          .map((o: any) => ({ label: `${o.orderNumber} - ${o.customerName}`, value: o.id }));
        setApprovedOrders(approved);
      }
    } catch (err) { console.error(err); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  const getFilteredCustomers = (): SelectOption[] => {
    return rawCustomers
      .filter(c => {
        if (formData.type === 2 || formData.type === 4) return c.type === 1 || c.type === 3 || c.type === "Buyer" || c.type === "Both";
        if (formData.type === 1 || formData.type === 3) return c.type === 2 || c.type === 3 || c.type === "Supplier" || c.type === "Both";
        return true;
      })
      .map(c => ({ label: c.name || "İsimsiz", value: c.id }));
  };

  const handleOrderChange = async (orderId: string) => {
    if (!orderId) {
      setFormData({ ...formData, orderId: null, details: [] });
      return;
    }
    try {
      const res = await api.get(`/Orders/${orderId}`);
      if (res.data.isSuccess) {
        const order = res.data.data;
        const mappedDetails = order.lines.map((l: any) => ({
          productId: l.productId,
          productName: l.productName,
          warehouseId: l.warehouseId || order.warehouseId,
          warehouseName: warehouses.find(w => w.value === (l.warehouseId || order.warehouseId))?.label || "Depo",
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          discountRate: 0,
          vatRate: 20,
          lineTotal: l.quantity * l.unitPrice * 1.20
        }));

        setFormData({
          ...formData,
          orderId: orderId,
          customerId: order.customerId || "", 
          warehouseId: order.warehouseId || formData.warehouseId,
          details: mappedDetails,
          type: order.type === "Sales" || order.type === 2 ? 2 : 1
        });
      }
    } catch (err) { alert("Sipariş verisi alınamadı."); }
  };

  const addLine = () => {
    if (!currentLine.productId || currentLine.quantity <= 0) return;
    const product = products.find(p => p.id === currentLine.productId);
    const warehouse = warehouses.find(w => w.value === currentLine.warehouseId);
    
    const gross = currentLine.quantity * currentLine.unitPrice;
    const discount = gross * (currentLine.discountRate / 100);
    const vat = (gross - discount) * (currentLine.vatRate / 100);
    
    const newLine = {
      ...currentLine,
      productName: product?.name,
      warehouseName: warehouse?.label || "Depo", // 🚀 Tabloda görünmesi için ekledik
      lineTotal: gross - discount + vat
    };

    setFormData(prev => ({ ...prev, details: [...prev.details, newLine] }));
    setCurrentLine({ ...currentLine, productId: "", quantity: 1, unitPrice: 0, discountRate: 0 });
  };

  const handleSave = async () => {
    if (!formData.customerId) { alert("Lütfen önce uygun bir cari hesap seçiniz."); return; }
    if (formData.details.length === 0) { alert("Fatura detayı boş olamaz."); return; }

    try {
      const response = await api.post("/Invoices", formData);
      if (response.data.isSuccess) {
        setShowCreateModal(false);
        fetchData();
      } else {
        alert(response.data.message || "Fatura kaydedilemedi.");
      }
    } catch (err: any) { 
      alert(err.response?.data?.message || "Kayıt sırasında bir hata oluştu."); 
    }
  };

  const handleApprove = async () => {
    try {
      const response = await api.post("/Invoices/Approve", approveData);
      if (response.data.isSuccess) {
        setShowApproveModal(false);
        fetchData();
        alert("Fatura başarıyla onaylandı.");
      }
    } catch (err: any) { alert(err.response?.data?.message || "Onay hatası."); }
  };

  return (
    <div className="page-invoices animate__animated animate__fadeIn">
      <DataTable<any>
        title="Fatura Yönetimi"
        description="Fatura ve iade süreçlerini yönetin."
        data={invoices}
        onAdd={() => {
          setFormData({ ...formData, customerId: "", warehouseId: defaultWarehouseId || "", details: [], orderId: null });
          setShowCreateModal(true);
        }}
        columns={[
          { header: "FATURA NO", accessor: (i) => <span className="fw-bold">{i.invoiceNumber}</span> },
          { header: "TARİH", accessor: (i) => new Date(i.invoiceDate).toLocaleDateString('tr-TR') },
          { header: "CARİ", accessor: (i) => i.customerName },
          { header: "TOPLAM", accessor: (i) => <span className="text-success fw-bold">{i.grandTotal.toLocaleString('tr-TR')} ₺</span> },
          { 
            header: "DURUM", 
            accessor: (i) => {
              const status = INVOICE_STATUS[i.status] || { label: i.status, badge: "bg-secondary" };
              return <span className={`badge border ${status.badge}`}>{status.label}</span>;
            } 
          },
          {
            header: "İŞLEMLER",
            className: "text-end",
            accessor: (i) => (
              <div className="d-flex justify-content-end gap-1">
                <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={async () => {
                   const res = await api.get(`/Invoices/${i.id}`);
                   setSelectedInvoice(res.data.data);
                   setShowDetailsModal(true);
                }} title="Görüntüle"><i className="bi bi-eye"></i></button>
                {i.status === "Draft" && hasPermission(APP_PERMISSIONS.Invoices.Approve) && (
                  <button className="btn btn-sm btn-success px-3" onClick={() => {
                    setApproveData({ ...approveData, id: i.id, paymentType: 1 });
                    setShowApproveModal(true);
                  }}>Onayla</button>
                )}
                {i.status === "Approved" && hasPermission(APP_PERMISSIONS.Invoices.Cancel) && (
                  <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={async () => {
                    if(!confirm("Fatura iptal edilsin mi?")) return;
                    await api.post("/Invoices/Cancel", { id: i.id });
                    fetchData();
                  }} title="İptal Et"><i className="bi bi-x-circle"></i></button>
                )}
              </div>
            )
          }
        ]}
      />

      <AppModal show={showCreateModal} title="Yeni Fatura" onClose={() => setShowCreateModal(false)} onSave={handleSave} size="xl">
        <div className="row g-3">
          <div className="col-md-5 border-end">
            <div className="p-2 bg-primary-subtle rounded border border-primary mb-3">
              <AppSelect 
                label="Siparişten Aktar" 
                options={approvedOrders} 
                value={formData.orderId || ""} 
                onChange={handleOrderChange} 
                isSearchable 
                placeholder="Onaylı sipariş..."
              />
            </div>
          </div>
          <div className="col-md-7">
            <div className="row g-2">
              <div className="col-md-4">
                <label className="form-label small fw-bold">Tarih</label>
                <input type="date" className="form-control form-control-sm" value={formData.invoiceDate} onChange={(e) => setFormData({...formData, invoiceDate: e.target.value})} />
              </div>
              <div className="col-md-4">
                <label className="form-label small fw-bold">Fatura Tipi</label>
                <select className="form-select form-select-sm" value={formData.type} onChange={(e) => setFormData({...formData, type: parseInt(e.target.value), customerId: ""})}>
                  {INVOICE_TYPES.map((t: any) => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <div className="col-md-4">
                {/* 🚀 Header Depo Seçimi (Default olarak ayarlardan gelir) */}
                <AppSelect 
                    label="Fatura Deposu" 
                    options={warehouses} 
                    value={formData.warehouseId} 
                    onChange={(val) => setFormData({...formData, warehouseId: val, details: formData.details.map(d => ({...d, warehouseId: val}))})} 
                />
              </div>
              <div className="col-12">
                <AppSelect 
                    label={formData.type === 2 || formData.type === 4 ? "Müşteri (Alıcı)" : "Tedarikçi (Satıcı)"} 
                    options={getFilteredCustomers()} 
                    value={formData.customerId} 
                    onChange={(val) => setFormData({...formData, customerId: val})} 
                    isSearchable 
                />
              </div>
            </div>
          </div>
          
          <div className="col-12 p-3 rounded border">
            <div className="row g-2 align-items-end">
              <div className="col-md-3">
                <AppSelect 
                  label="Ürün" 
                  options={products.map(p => ({ label: p.name, value: p.id }))} 
                  value={currentLine.productId} 
                  onChange={(val) => {
                    const p = products.find(x => x.id === val);
                    setCurrentLine({...currentLine, productId: val, unitPrice: p?.defaultPrice || 0});
                  }} 
                  isSearchable
                />
              </div>
              <div className="col-md-3">
                {/* 🚀 Satır Bazlı Depo Seçimi */}
                <AppSelect 
                  label="Çıkış Deposu" 
                  options={warehouses} 
                  value={currentLine.warehouseId} 
                  onChange={(val) => setCurrentLine({...currentLine, warehouseId: val})} 
                />
              </div>
              <div className="col-md-2"><label className="form-label small">Miktar</label><input type="number" className="form-control form-control-sm" value={currentLine.quantity} onChange={(e) => setCurrentLine({...currentLine, quantity: parseFloat(e.target.value)})} /></div>
              <div className="col-md-3"><label className="form-label small">Birim Fiyat</label><input type="number" className="form-control form-control-sm" value={currentLine.unitPrice} onChange={(e) => setCurrentLine({...currentLine, unitPrice: parseFloat(e.target.value)})} /></div>
              <div className="col-md-1"><button type="button" className="btn btn-primary btn-sm w-100" onClick={addLine}>Ekle</button></div>
            </div>
          </div>

          <div className="col-12 mt-2">
            <table className="table table-sm table-dark">
              <thead><tr className="small text-muted"><th>Ürün</th><th>Depo</th><th className="text-end">Miktar</th><th className="text-end">Fiyat</th><th className="text-end">Toplam</th></tr></thead>
              <tbody>
                {formData.details.map((l, i) => (
                  <tr key={i} className="align-middle">
                    <td>{l.productName}</td>
                    <td><span className="badge bg-secondary-subtle text-secondary small">{l.warehouseName}</span></td>
                    <td className="text-end fw-bold">{l.quantity}</td>
                    <td className="text-end">{l.unitPrice} ₺</td>
                    <td className="text-end text-success fw-bold">{l.lineTotal.toLocaleString('tr-TR')} ₺</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </AppModal>

      {/* Approve ve Detail modalları aynı... */}
      <AppModal show={showApproveModal} title="Faturayı Onayla" onClose={() => setShowApproveModal(false)} onSave={handleApprove} saveButtonText="Onayla ve İşle">
        <div className="row g-3">
          <div className="col-12">
            <label className="form-label small fw-bold">Ödeme Tipi</label>
            <div className="d-flex gap-3 mt-2">
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 1} onChange={() => setApproveData({...approveData, paymentType: 1, cashId: null, bankId: null})} /><label className="form-check-label">Veresiye</label></div>
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 2} onChange={() => setApproveData({...approveData, paymentType: 2, bankId: null})} /><label className="form-check-label">Kasa</label></div>
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 3} onChange={() => setApproveData({...approveData, paymentType: 3, cashId: null})} /><label className="form-check-label">Banka</label></div>
            </div>
          </div>
          {approveData.paymentType === 2 && <div className="col-12"><AppSelect label="Kasa" options={cashes} value={approveData.cashId || ""} onChange={(val) => setApproveData({...approveData, cashId: val})} isSearchable /></div>}
          {approveData.paymentType === 3 && <div className="col-12"><AppSelect label="Banka" options={banks} value={approveData.bankId || ""} onChange={(val) => setApproveData({...approveData, bankId: val})} isSearchable /></div>}
        </div>
      </AppModal>

      <AppModal show={showDetailsModal} title={`Detay: ${selectedInvoice?.invoiceNumber}`} onClose={() => setShowDetailsModal(false)} size="lg" saveButton={false}>
        {selectedInvoice && (
          <div className="row g-3">
            <div className="col-md-6"><div className="small text-muted">Cari</div><div className="fw-bold">{selectedInvoice.customerName}</div></div>
            <div className="col-md-6 text-end"><div className="small text-muted">Durum</div><span className={`badge border ${INVOICE_STATUS[selectedInvoice.status]?.badge}`}>{INVOICE_STATUS[selectedInvoice.status]?.label}</span></div>
            <div className="col-12"><hr className="opacity-10"/></div>
            <table className="table table-sm table-dark">
              <thead><tr className="small text-muted"><th>Ürün</th><th className="text-end">Miktar</th><th className="text-end">Birim Fiyat</th><th className="text-end">Toplam</th></tr></thead>
              <tbody>
                {selectedInvoice.lines.map((l: any, i: number) => (
                  <tr key={i}><td>{l.productName}</td><td className="text-end">{l.quantity}</td><td className="text-end">{l.unitPrice} ₺</td><td className="text-end">{l.lineTotal.toLocaleString('tr-TR')} ₺</td></tr>
                ))}
              </tbody>
              <tfoot>
                <tr><td colSpan={3} className="text-end fw-bold text-info fs-5">GENEL TOPLAM:</td><td className="text-end fw-bold text-info fs-5">{selectedInvoice.grandTotal.toLocaleString('tr-TR')} ₺</td></tr>
              </tfoot>
            </table>
          </div>
        )}
      </AppModal>
    </div>
  );
};

export default Invoices;