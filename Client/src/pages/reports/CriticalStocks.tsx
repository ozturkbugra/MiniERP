import { useState, useEffect, useCallback, useMemo } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';
import AppSelect from '../../components/Common/AppSelect';
import type { SelectOption } from '../../components/Common/AppSelect';

// 🚀 Backend'deki CriticalStockResponse ile birebir uyumlu
interface CriticalStockResponse {
  id: string; // DataTable için mapped id
  productId: string;
  productName: string;
  warehouseId: string;
  warehouseName: string;
  total: number;         // Mevcut bakiye
  criticalLevel: number; // Ürün kartındaki kritik limit
}

interface Warehouse {
  id: string;
  name: string;
}

const CriticalStocks = () => {
  const { hasPermission } = useAuthStore();
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  // Filtre State'i
  const [warehouseId, setWarehouseId] = useState<string>("");

  // Veri State'leri
  const [data, setData] = useState<CriticalStockResponse[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // 1. Depoları Çekme
  const fetchWarehouses = useCallback(async () => {
    try {
      const response = await api.get("/Warehouses");
      if (response.data.isSuccess) {
        setWarehouses(response.data.data);
      }
    } catch (error) {
      console.error("Depolar listelenirken hata oluştu");
    }
  }, []);

  // 2. Kritik Stok Sorgulama (Button Trigger)
  const fetchCriticalStock = useCallback(async () => {
    try {
      setLoading(true);
      // 🚀 Endpoint: api/StockTransactions/critical-stock
      const response = await api.get("/StockTransactions/critical-stock", {
        params: {
          warehouseId: warehouseId || null
        }
      });

      if (response.data.isSuccess) {
        const mappedData = response.data.data.map((item: any, index: number) => ({
          ...item,
          id: `${item.productId}-${item.warehouseId}` || `crit-${index}`
        }));
        setData(mappedData);
      }
    } catch (error) {
      console.error("Kritik stok verisi çekilemedi");
      setData([]);
    } finally {
      setLoading(false);
    }
  }, [warehouseId]);

  useEffect(() => {
    fetchWarehouses();
    fetchCriticalStock();
  }, [fetchWarehouses]);

  // 📊 Tablo Sütunları
  const columns = [
    { 
      header: "ÜRÜN ADI", 
      accessor: "productName" as keyof CriticalStockResponse,
      className: "fw-bold text-light"
    },
    { 
      header: "DEPO", 
      accessor: "warehouseName" as keyof CriticalStockResponse,
      className: "text-muted small"
    },
    { 
      header: "MEVCUT STOK", 
      className: "text-center",
      accessor: (item: CriticalStockResponse) => (
        <span className="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 px-3 py-2 fs-6">
          {item.total}
        </span>
      )
    },
    { 
      header: "KRİTİK SEVİYE", 
      className: "text-center",
      accessor: (item: CriticalStockResponse) => (
        <span className="badge bg-secondary-subtle text-secondary border border-secondary-subtle px-3 py-2">
          {item.criticalLevel}
        </span>
      )
    },
    {
        header: "DURUM",
        className: "text-end pe-4",
        accessor: (item: CriticalStockResponse) => {
            const diff = item.criticalLevel - item.total;
            return (
                <span className="text-warning small fw-medium">
                    <i className="bi bi-exclamation-triangle me-1"></i>
                    {diff > 0 ? `${diff} adet eksik` : "Limit değerde"}
                </span>
            );
        }
    }
  ];

  const warehouseOptions = useMemo((): SelectOption[] => {
    return warehouses.map(w => ({
      value: w.id,
      label: w.name
    }));
  }, [warehouses]);

  if (!canViewReports) {
    return <div className="alert alert-danger m-4">Bu raporu görüntüleme yetkiniz bulunmamaktadır.</div>;
  }

  return (
    <div className="page-critical-stocks animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-3 fw-bold">Kritik Stok Listesi</h1>
        <p className="text-muted small">Tanımlanan kritik seviyenin altına düşen ürünleri analiz edin.</p>
      </div>

      {/* Filtre Paneli */}
      <div className="card shadow-sm border-0 mb-4 p-3">
        <div className="card-body">
          <div className="row g-4 align-items-end">
            <div className="col-md-8">
              <AppSelect 
                label="Depoya Göre Filtrele"
                placeholder="Tüm Depolar"
                isSearchable={true}
                options={warehouseOptions}
                value={warehouseId}
                onChange={(val) => setWarehouseId(val)}
              />
            </div>
            <div className="col-md-4">
              <button 
                className="btn btn-primary w-100 shadow-sm fw-bold"
                onClick={fetchCriticalStock}
                disabled={loading}
              >
                {loading ? <span className="spinner-border spinner-border-sm me-2"></span> : <i className="bi bi-arrow-repeat me-2"></i>}
                Listeyi Güncelle
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Veri Tablosu */}
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
          <p className="mt-3 text-muted">Stoklar kontrol ediliyor...</p>
        </div>
      ) : (
        <DataTable<CriticalStockResponse>
          title="Kritik Stok Alarmları"
          description="Aşağıdaki ürünler belirlenen minimum stok seviyesinin altındadır."
          columns={columns}
          data={data}
        />
      )}
    </div>
  );
};

export default CriticalStocks;