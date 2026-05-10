import { useState, useEffect, useCallback, useMemo } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';
import AppSelect from '../../components/Common/AppSelect';
import type { SelectOption } from '../../components/Common/AppSelect';

interface ProductLedgerRow {
  id: string;
  date: string;
  firmName: string;
  transactionType: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  runningQuantity: number;
  runningValue: number;
  documentNo: string;
}

interface Product {
  id: string;
  name: string;
  code: string;
}

const ProductLedger = () => {
  const { hasPermission } = useAuthStore();
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  const [selectedProductId, setSelectedProductId] = useState<string>("");
  const [products, setProducts] = useState<Product[]>([]);
  const [ledgerData, setLedgerData] = useState<ProductLedgerRow[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchProducts = useCallback(async () => {
    try {
      const response = await api.get("/Products");
      if (response.data.isSuccess) setProducts(response.data.data);
    } catch (error) {
      console.error("Ürün listesi alınamadı");
    }
  }, []);

  const fetchLedger = useCallback(async () => {
    if (!selectedProductId) return;
    try {
      setLoading(true);
      const response = await api.get(`/Dashboards/ProductLedger/${selectedProductId}`);
      if (response.data.isSuccess) {
        const mappedData = response.data.data.map((item: any, index: number) => ({
          ...item,
          id: `ledger-${index}-${item.documentNo}`
        }));
        setLedgerData(mappedData);
      }
    } catch (error) {
      console.error("Defter verileri çekilemedi");
      setLedgerData([]);
    } finally {
      setLoading(false);
    }
  }, [selectedProductId]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const productOptions = useMemo((): SelectOption[] => {
    return products.map(p => ({ value: p.id, label: `${p.code} - ${p.name}` }));
  }, [products]);

  const columns = [
    { 
      header: "TARİH / EVRAK", 
      accessor: (item: ProductLedgerRow) => (
        <div>
          <div className="fw-bold">{new Date(item.date).toLocaleDateString('tr-TR')}</div>
          <div className="text-muted small">{item.documentNo}</div>
        </div>
      )
    },
    { header: "CARİ / FİRMA", accessor: "firmName" as keyof ProductLedgerRow },
    { 
      header: "İŞLEM", 
      accessor: (item: ProductLedgerRow) => (
        <span className="small text-uppercase fw-medium">
          {item.transactionType || "Diğer"}
        </span>
      ) 
    },
    { 
      header: "MİKTAR", 
      className: "text-center",
      accessor: (item: ProductLedgerRow) => (
        <span className={`fw-bold ${item.quantity > 0 ? 'text-success' : 'text-danger'}`}>
          {item.quantity > 0 ? `+${item.quantity}` : item.quantity}
        </span>
      )
    },
    { 
      header: "KALAN STOK", 
      className: "text-center fw-bold", 
      accessor: (item: ProductLedgerRow) => item.runningQuantity
    },
    { 
      header: "BİRM FİYAT", 
      className: "text-end",
      accessor: (item: ProductLedgerRow) => `${item.unitPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺`
    },
    { 
      header: "STOK DEĞERİ", 
      className: "text-end pe-3 fw-bold", 
      accessor: (item: ProductLedgerRow) => (
        <span>
          {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(item.runningValue)}
        </span>
      )
    }
  ];

  if (!canViewReports) return <div className="alert alert-warning m-4">Erişim yetkiniz bulunmuyor.</div>;

  return (
    <div className="page-product-ledger animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-4 fw-bold">Ürün Defteri</h1>
        <p className="text-muted small">Ürünün tüm stok hareketlerini ve bakiye geçmişini izleyin.</p>
      </div>

      <div className="card shadow-sm border-0 mb-4">
        <div className="card-body py-4">
          <div className="row g-3 align-items-end">
            <div className="col-md-9">
              <AppSelect 
                label="Ürün Seçimi"
                placeholder="Kod veya isim ile arayın..."
                isSearchable={true}
                options={productOptions}
                value={selectedProductId}
                onChange={(val) => setSelectedProductId(val)}
              />
            </div>
            <div className="col-md-3">
              <button 
                className="btn btn-primary w-100 fw-bold"
                onClick={fetchLedger}
                disabled={loading || !selectedProductId}
              >
                {loading ? <span className="spinner-border spinner-border-sm me-2"></span> : <i className="bi bi-search me-2"></i>}
                Sorgula
              </button>
            </div>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : selectedProductId && (
        <DataTable<ProductLedgerRow>
          title="Hesap Hareketleri"
          description="Kronolojik bakiye ve işlem dökümü."
          columns={columns}
          data={ledgerData}
        />
      )}
    </div>
  );
};

export default ProductLedger;