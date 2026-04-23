import { useState, useEffect, useCallback, useMemo } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';
// 🚀 HATA DÜZELTMESİ: 'type' anahtar kelimesini ekledik
import AppSelect from '../../components/Common/AppSelect';
import type { SelectOption } from '../../components/Common/AppSelect';

interface StockSnapshotResponse {
  id: string;
  productName: string;
  remainingQuantity: number;
  totalValue: number;
}

interface Warehouse {
  id: string;
  name: string;
}

const StockSnapshot = () => {
  const { hasPermission } = useAuthStore();
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  const [targetDate, setTargetDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [warehouseId, setWarehouseId] = useState<string>("");

  const [data, setData] = useState<StockSnapshotResponse[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

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

  // 🚀 BUTONLA ÇALIŞACAK ANA FONKSİYON
  const fetchSnapshot = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Dashboards/StockSnapshot", {
        params: {
          targetDate: targetDate,
          warehouseId: warehouseId || null
        }
      });

      if (response.data.isSuccess) {
        const mappedData = response.data.data.map((item: any, index: number) => ({
          ...item,
          id: item.productId || `row-${index}`
        }));
        setData(mappedData);
      }
    } catch (error) {
      console.error("Stok raporu çekilemedi");
      setData([]);
    } finally {
      setLoading(false);
    }
  }, [targetDate, warehouseId]);

  // 🚀 KRİTİK DEĞİŞİKLİK: 
  // useEffect artık 'fetchSnapshot'ı izlemiyor. 
  // Sadece sayfa ilk açıldığında (mount) çalışıyor.
  useEffect(() => {
    fetchWarehouses();
    fetchSnapshot(); // İlk açılışta veriler gelsin diye burada çağırıyoruz
  }, [fetchWarehouses]); // fetchSnapshot'ı buraya eklemiyoruz ki input değişiminde tetiklenmesin

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
  };

  const columns = [
    { 
      header: "ÜRÜN ADI", 
      accessor: "productName" as keyof StockSnapshotResponse,
      className: "fw-medium"
    },
    { 
      header: "KALAN MİKTAR", 
      className: "text-center",
      accessor: (item: StockSnapshotResponse) => (
        <span className="badge bg-primary-subtle text-primary border border-primary-subtle px-3 py-2">
          {item.remainingQuantity}
        </span>
      )
    },
    { 
      header: "TOPLAM DEĞER", 
      className: "text-end pe-4",
      accessor: (item: StockSnapshotResponse) => (
        <span className="fw-bold text-success">
          {formatCurrency(item.totalValue)}
        </span>
      )
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
    <div className="page-reports animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-3 fw-bold">Stokta Kalan</h1>
        <nav className="breadcrumb">
          <span className="breadcrumb-item">Raporlar</span>
          <span className="breadcrumb-item active">Zaman Makinesi</span>
        </nav>
      </div>

      <div className="card shadow-sm border-0 mb-4 p-3">
        <div className="card-body">
          <div className="row g-4 align-items-end">
            <div className="col-md-4">
              <label className="form-label fw-bold small text-muted">Hedef Tarih</label>
              <input 
                type="date" 
                className="form-control"
                value={targetDate}
                onChange={(e) => setTargetDate(e.target.value)}
              />
            </div>
            <div className="col-md-4">
              <AppSelect 
                label="Depo Seçimi"
                placeholder="Tüm Depolar"
                isSearchable={true}
                options={warehouseOptions}
                value={warehouseId}
                onChange={(val) => setWarehouseId(val)}
              />
            </div>
            <div className="col-md-4">
              {/* 🚀 ARTIK TETİKLEYİCİ SADECE BU BUTON */}
              <button 
                className="btn btn-primary w-100 shadow-sm fw-bold"
                onClick={fetchSnapshot}
                disabled={loading}
              >
                {loading ? <span className="spinner-border spinner-border-sm me-2"></span> : <i className="bi bi-search me-2"></i>}
                Verileri Getir
              </button>
            </div>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
          <p className="mt-3 text-muted">Rapor hazırlanıyor...</p>
        </div>
      ) : (
        <DataTable<StockSnapshotResponse>
          title="Stok Durum Listesi"
          description={`${targetDate} tarihi itibarıyla depo mevcudu.`}
          columns={columns}
          data={data}
        />
      )}
    </div>
  );
};

export default StockSnapshot;