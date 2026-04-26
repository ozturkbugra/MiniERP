import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
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
  const navigate = useNavigate();
  const { hasPermission } = useAuthStore();
  const { defaultWarehouseId } = useSettingsStore();

  const [invoices, setInvoices] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<any>(null);

  // 🚀 Fiyat görünümü için ara state
  const [displayLinePrice, setDisplayLinePrice] = useState<string>("");

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
    warehouseId: defaultWarehouseId || "",
    quantity: 1,
    unitPrice: 0,
    discountRate: 0,
    vatRate: 20 
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

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [iRes, cRes, pRes, wRes, cashRes, bankRes, oRes] = await Promise.all([
        api.get("/Invoices"), api.get("/Customers"), api.get("/Products"),
        api.get("/Warehouses"), api.get("/Cashes"), api.get("/Banks"), api.get("/Orders")
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
      .filter((c: any) => {
        if (formData.type === 2 || formData.type === 4) return c.type === 1 || c.type === 3 || c.type === "Buyer" || c.type === "Both";
        if (formData.type === 1 || formData.type === 3) return c.type === 2 || c.type === 3 || c.type === "Supplier" || c.type === "Both";
        return true;
      })
      .map((c: any) => ({ label: c.name || "İsimsiz", value: c.id }));
  };

  // 🚀 SİPARİŞTEN AKTARMA VE KDV ÇEKME
  const handleOrderChange = async (orderId: string) => {
    if (!orderId) {
      setFormData({ ...formData, orderId: null, details: [] });
      return;
    }
    try {
      const res = await api.get(`/Orders/${orderId}`);
      if (res.data.isSuccess) {
        const order = res.data.data;
        const mappedDetails = order.lines.map((l: any) => {
          const product = products.find(p => p.id === l.productId);
          const vatRate = product?.vatRate || 20;
          return {
            productId: l.productId,
            productName: l.productName,
            warehouseId: l.warehouseId || order.warehouseId,
            warehouseName: warehouses.find(w => w.value === (l.warehouseId || order.warehouseId))?.label || "Depo",
            quantity: l.quantity,
            unitPrice: l.unitPrice,
            discountRate: 0,
            vatRate: vatRate, 
            lineTotal: l.quantity * l.unitPrice * (1 + vatRate / 100)
          };
        });

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

  const updateLine = (index: number, field: string, value: any) => {
    const newDetails = [...formData.details];
    const line = { ...newDetails[index], [field]: value };
    const gross = line.quantity * line.unitPrice;
    const discount = gross * (line.discountRate / 100);
    line.lineTotal = (gross - discount) * (1 + line.vatRate / 100);
    newDetails[index] = line;
    setFormData({ ...formData, details: newDetails });
  };

  const calculateTotals = () => {
    const subTotal = formData.details.reduce((acc, curr) => acc + (curr.quantity * curr.unitPrice), 0);
    const totalVat = formData.details.reduce((acc, curr) => acc + ((curr.quantity * curr.unitPrice) * (curr.vatRate / 100)), 0);
    return { subTotal, totalVat, grandTotal: subTotal + totalVat };
  };

  const addLine = () => {
    if (!currentLine.productId || currentLine.quantity <= 0) return;
    const product = products.find(p => p.id === currentLine.productId);
    const warehouse = warehouses.find(w => w.value === currentLine.warehouseId);
    const gross = currentLine.quantity * currentLine.unitPrice;
    const vat = gross * (currentLine.vatRate / 100);
    
    const newLine = {
      ...currentLine,
      productName: product?.name,
      warehouseName: warehouse?.label || "Depo",
      lineTotal: gross + vat
    };

    setFormData(prev => ({ ...prev, details: [...prev.details, newLine] }));
    setCurrentLine({ ...currentLine, productId: "", quantity: 1, unitPrice: 0, discountRate: 0, vatRate: 20 });
    setDisplayLinePrice("");
  };

  // 🚀 İŞTE EKSİK OLAN handleSave FONKSİYONU
  const handleSave = async () => {
    if (!formData.customerId) { alert("Lütfen cari hesap seçiniz."); return; }
    if (formData.details.length === 0) { alert("Fatura detayı boş olamaz."); return; }

    try {
      const response = await api.post("/Invoices", formData);
      if (response.data.isSuccess) {
        setShowCreateModal(false);
        fetchData();
        alert("Fatura taslak olarak kaydedildi.");
      }
    } catch (err: any) { 
      alert(err.response?.data?.message || "Kayıt hatası."); 
    }
  };

  const handleApprove = async () => {
    try {
      const response = await api.post("/Invoices/Approve", approveData);
      if (response.data.isSuccess) {
        setShowApproveModal(false);
        fetchData(); 
        alert("Fatura onaylandı ve stok/finans kayıtları işlendi.");
      }
    } catch (err: any) { alert("Onay hatası."); }
  };

  const totals = calculateTotals();

  return (
    <div className="page-invoices animate__animated animate__fadeIn">
      <DataTable<any>
        title="Fatura Yönetimi"
        description="Fatura süreçlerini KDV ve sipariş aktarımı ile yönetin."
        data={invoices}
        onAdd={() => {
          setFormData({ ...formData, customerId: "", warehouseId: defaultWarehouseId || "", details: [], orderId: null });
          setDisplayLinePrice("");
          setShowCreateModal(true);
        }}
        columns={[
          { header: "FATURA NO", accessor: (i: any) => <span className="fw-bold text-primary">{i.invoiceNumber}</span> },
          { header: "TARİH", accessor: (i: any) => new Date(i.invoiceDate).toLocaleDateString('tr-TR') },
          { header: "CARİ", accessor: (i: any) => i.customerName },
          { header: "TOPLAM", accessor: (i: any) => <span className="text-success fw-bold">{i.grandTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</span> },
          { 
            header: "DURUM", 
            accessor: (i: any) => {
              const status = INVOICE_STATUS[i.status] || { label: i.status, badge: "bg-secondary" };
              return <span className={`badge border ${status.badge}`}>{status.label}</span>;
            } 
          },
          {
            header: "İŞLEMLER",
            className: "text-end",
            accessor: (i: any) => (
              <div className="d-flex justify-content-end gap-1">
                <button className="btn btn-sm btn-outline-warning border-0 p-2" onClick={() => navigate(`/invoices/print/${i.id}`)} title="Yazdır"><i className="bi bi-printer"></i></button>
                <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={async () => {
                     const res = await api.get(`/Invoices/${i.id}`);
                     setSelectedInvoice(res.data.data);
                     setShowDetailsModal(true);
                }} title="Görüntüle"><i className="bi bi-eye"></i></button>
                {i.status === "Draft" && (
                  <button className="btn btn-sm btn-success px-3" onClick={() => {
                    setApproveData({ ...approveData, id: i.id, paymentType: 1 });
                    setShowApproveModal(true);
                  }}>Onayla</button>
                )}
              </div>
            )
          }
        ]}
      />

      <AppModal show={showCreateModal} title="Yeni Fatura" onClose={() => setShowCreateModal(false)} onSave={handleSave} size="xl">
        <div className="row g-3">
          <div className="col-md-5 border-end">
            <AppSelect label="Siparişten Aktar" options={approvedOrders} value={formData.orderId || ""} onChange={handleOrderChange} isSearchable placeholder="Onaylı sipariş seç..." />
          </div>
          <div className="col-md-7">
            <div className="row g-2">
              <div className="col-md-4"><label className="small fw-bold">Tarih</label><input type="date" className="form-control form-control-sm" value={formData.invoiceDate} onChange={(e) => setFormData({...formData, invoiceDate: e.target.value})} /></div>
              <div className="col-md-4"><label className="small fw-bold">Tip</label><select className="form-select form-select-sm" value={formData.type} onChange={(e) => setFormData({...formData, type: parseInt(e.target.value)})}>{INVOICE_TYPES.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}</select></div>
              <div className="col-md-4"><AppSelect label="Depo" options={warehouses} value={formData.warehouseId} onChange={(val: string) => setFormData({...formData, warehouseId: val})} /></div>
              <div className="col-12"><AppSelect label="Cari Hesap" options={getFilteredCustomers()} value={formData.customerId} onChange={(val: string) => setFormData({...formData, customerId: val})} isSearchable /></div>
            </div>
          </div>
          
          <div className="col-12 p-3 rounded border">
            <div className="row g-2 align-items-end">
              <div className="col-md-4"><AppSelect label="Ürün" options={products.map(p => ({ label: p.name, value: p.id }))} value={currentLine.productId} onChange={(val: string) => {
                  const p = products.find(x => x.id === val);
                  setCurrentLine({...currentLine, productId: val, unitPrice: p?.defaultPrice || 0, vatRate: p?.vatRate || 20});
                  setDisplayLinePrice(p?.defaultPrice ? maskCurrency(p.defaultPrice.toString().replace(".", ",")) : "");
              }} isSearchable /></div>
              <div className="col-md-1"><label className="small">Miktar</label><input type="number" className="form-control form-control-sm" value={currentLine.quantity} onChange={(e) => setCurrentLine({...currentLine, quantity: parseFloat(e.target.value)})} /></div>
              <div className="col-md-2"><label className="small">Birim Fiyat (₺)</label><input type="text" className="form-control form-control-sm text-end fw-bold" value={displayLinePrice} onChange={(e) => {
                  const masked = maskCurrency(e.target.value);
                  setDisplayLinePrice(masked);
                  setCurrentLine(prev => ({ ...prev, unitPrice: unmaskPrice(masked) }));
              }} onFocus={(e) => e.target.select()} /></div>
              <div className="col-md-2"><label className="small">KDV (%)</label><select className="form-select form-select-sm" value={currentLine.vatRate} onChange={(e) => setCurrentLine({...currentLine, vatRate: parseInt(e.target.value)})}>{[0, 1, 10, 20].map(v => <option key={v} value={v}>%{v}</option>)}</select></div>
              <div className="col-md-2"><button type="button" className="btn btn-primary btn-sm w-100" onClick={addLine}><i className="bi bi-plus-lg me-1"></i>Ekle</button></div>
            </div>
          </div>

          <div className="col-12 mt-2">
            <table className="table table-sm table-hover align-middle">
              <thead className="table-dark"><tr className="small"><th>Ürün</th><th>Depo</th><th className="text-end">Miktar</th><th className="text-end">Fiyat</th><th className="text-center" style={{width: '100px'}}>KDV</th><th className="text-end">Toplam</th><th></th></tr></thead>
              <tbody>
                {formData.details.map((l, i) => (
                  <tr key={i}>
                    <td>{l.productName}</td>
                    <td><span className="badge bg-secondary-subtle text-secondary small">{l.warehouseName}</span></td>
                    <td className="text-end fw-bold">{l.quantity}</td>
                    <td className="text-end">{l.unitPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                    <td><select className="form-select form-select-sm py-0" style={{fontSize: '0.75rem'}} value={l.vatRate} onChange={(e) => updateLine(i, 'vatRate', parseInt(e.target.value))}>{[0, 1, 10, 20].map(v => <option key={v} value={v}>%{v}</option>)}</select></td>
                    <td className="text-end text-success fw-bold">{l.lineTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</td>
                    <td className="text-center"><button className="btn btn-sm text-danger p-0" onClick={() => setFormData({...formData, details: formData.details.filter((_, idx) => idx !== i)})}><i className="bi bi-trash"></i></button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="col-12 border-top pt-3 rounded-bottom">
            <div className="row justify-content-end">
              <div className="col-md-4">
                <div className="d-flex justify-content-between mb-1"><span className="text-muted small">Ara Toplam:</span><span className="fw-bold">{totals.subTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</span></div>
                <div className="d-flex justify-content-between mb-1"><span className="text-muted small">Toplam KDV:</span><span className="fw-bold text-danger">{totals.totalVat.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</span></div>
                <div className="d-flex justify-content-between border-top pt-2"><span className="fw-bold text-primary fs-5">Genel Toplam:</span><span className="fw-bold text-primary fs-5">{totals.grandTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</span></div>
              </div>
            </div>
          </div>
        </div>
      </AppModal>

      {/* 🚀 MODAL: ONAYLAMA */}
      <AppModal show={showApproveModal} title="Faturayı Onayla" onClose={() => setShowApproveModal(false)} onSave={handleApprove} saveButtonText="Onayla">
        <div className="row g-3">
          <div className="col-12"><label className="form-label small fw-bold">Ödeme Tipi</label>
            <div className="d-flex gap-3 mt-2">
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 1} onChange={() => setApproveData({...approveData, paymentType: 1, cashId: null, bankId: null})} /><label className="form-check-label">Veresiye</label></div>
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 2} onChange={() => setApproveData({...approveData, paymentType: 2, bankId: null})} /><label className="form-check-label">Kasa</label></div>
              <div className="form-check"><input className="form-check-input" type="radio" checked={approveData.paymentType === 3} onChange={() => setApproveData({...approveData, paymentType: 3, cashId: null})} /><label className="form-check-label">Banka</label></div>
            </div>
          </div>
          {approveData.paymentType === 2 && <div className="col-12"><AppSelect label="Kasa Seçimi" options={cashes} value={approveData.cashId || ""} onChange={(val: string) => setApproveData({...approveData, cashId: val})} isSearchable /></div>}
          {approveData.paymentType === 3 && <div className="col-12"><AppSelect label="Banka Seçimi" options={banks} value={approveData.bankId || ""} onChange={(val: string) => setApproveData({...approveData, bankId: val})} isSearchable /></div>}
        </div>
      </AppModal>

      {/* 🚀 MODAL: DETAYLAR */}
      <AppModal show={showDetailsModal} title={`Fatura Detayı: ${selectedInvoice?.invoiceNumber}`} onClose={() => setShowDetailsModal(false)} size="lg" saveButton={false}>
        {selectedInvoice && (
          <div className="row g-3">
            <div className="col-md-6"><div className="small text-muted">Cari Hesap</div><div className="fw-bold">{selectedInvoice.customerName}</div></div>
            <div className="col-md-6 text-end"><span className={`badge border ${INVOICE_STATUS[selectedInvoice.status]?.badge}`}>{INVOICE_STATUS[selectedInvoice.status]?.label}</span></div>
            <div className="col-12"><hr className="opacity-10"/></div>
            <table className="table table-sm border align-middle">
              <thead><tr className="small text-muted"><th>Ürün</th><th className="text-end">Miktar</th><th className="text-end">Birim Fiyat</th><th className="text-end">Toplam</th></tr></thead>
              <tbody>
                {selectedInvoice.lines.map((l: any, i: number) => (
                  <tr key={i}><td>{l.productName}</td><td className="text-end">{l.quantity}</td><td className="text-end">{l.unitPrice.toLocaleString('tr-TR')} ₺</td><td className="text-end fw-bold">{l.lineTotal.toLocaleString('tr-TR')} ₺</td></tr>
                ))}
              </tbody>
              <tfoot>
                <tr><td colSpan={3} className="text-end fw-bold text-info fs-5 pt-3">GENEL TOPLAM:</td><td className="text-end fw-bold text-info fs-5 pt-3">{selectedInvoice.grandTotal.toLocaleString('tr-TR')} ₺</td></tr>
              </tfoot>
            </table>
          </div>
        )}
      </AppModal>
    </div>
  );
};

export default Invoices;