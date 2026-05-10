import { useState, useEffect, useCallback } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';

// 🚀 Backend'den dönen JSON verisine uygun model
interface CustomerBalanceRow {
  id: string;
  name: string;
  totalDebit: number;   // Toplam Borç
  totalCredit: number;  // Toplam Alacak
  balance: number;      // Güncel Bakiye
}

const CustomerBalances = () => {
  const { hasPermission } = useAuthStore();
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  const [data, setData] = useState<CustomerBalanceRow[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // Cari Bakiyelerini Çekme
  const fetchBalances = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Transactions/CustomerBalances");
      if (response.data.isSuccess) {
        setData(response.data.data);
      }
    } catch (error) {
      console.error("Cari bakiyeler listelenirken hata oluştu");
      setData([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBalances();
  }, [fetchBalances]);

  // Para formatlayıcı
  const formatCurrency = (value: number) => 
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);

  // 📊 Tablo Sütunları
  const columns = [
    { 
      header: "CARİ KART / MÜŞTERİ ADI", 
      accessor: "name" as keyof CustomerBalanceRow,
      className: "fw-bold"
    },
    { 
      header: "TOPLAM BORÇ HAREKETİ", 
      className: "text-end",
      accessor: (item: CustomerBalanceRow) => (
        <span className="text-muted">
          {formatCurrency(item.totalDebit)}
        </span>
      )
    },
    { 
      header: "TOPLAM ALACAK HAREKETİ", 
      className: "text-end",
      accessor: (item: CustomerBalanceRow) => (
        <span className="text-muted">
          {formatCurrency(item.totalCredit)}
        </span>
      )
    },
    {
      header: "CARİ DURUMU",
      className: "text-center",
      accessor: (item: CustomerBalanceRow) => {
        if (item.balance > 0) {
          return <span className="badge bg-success bg-opacity-10 border border-success-subtle px-2">BİZE BORÇLU</span>;
        } else if (item.balance < 0) {
          return <span className="badge bg-danger bg-opacity-10 border border-danger-subtle px-2">BİZ BORÇLUYUZ</span>;
        } else {
          return <span className="badge bg-secondary bg-opacity-10 border border-secondary-subtle px-2">KAPANDI</span>;
        }
      }
    },
    { 
      header: "NET BAKİYE", 
      className: "text-end pe-4 fs-6",
      accessor: (item: CustomerBalanceRow) => (
        <span className={`fw-bold ${item.balance > 0 ? 'text-success' : item.balance < 0 ? 'text-danger' : ''}`}>
          {formatCurrency(Math.abs(item.balance))} {/* Eksiyi gizleyip durumu Durum kolonunda gösteriyoruz */}
        </span>
      )
    }
  ];

  if (!canViewReports) {
    return <div className="alert alert-warning m-4">Bu raporu görüntüleme yetkiniz bulunmamaktadır.</div>;
  }

  return (
    <div className="page-customer-balances animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <div className="d-flex justify-content-between align-items-center">
          <div>
            <h1 className="page-title fs-4 fw-bold">Cari Bakiye Raporu</h1>
            <p className="text-muted small mb-0">Tüm müşterilerinizin ve tedarikçilerinizin anlık borç / alacak durumları.</p>
          </div>
          <button className="btn btn-outline-primary btn-sm fw-bold shadow-sm" onClick={fetchBalances} disabled={loading}>
            <i className={`bi bi-arrow-clockwise me-1 ${loading ? 'spin' : ''}`}></i> Yenile
          </button>
        </div>
      </div>

      {/* 📊 Veri Tablosu */}
      <div className="row">
        <div className="col-12">
          {loading ? (
            <div className="card shadow-sm border-0 py-5">
              <div className="text-center">
                <div className="spinner-border text-primary" role="status"></div>
                <div className="mt-2 text-muted small">Cari hesaplar taranıyor...</div>
              </div>
            </div>
          ) : (
            <DataTable<CustomerBalanceRow>
              title="Borç & Alacak Listesi"
              description="Arama kutusunu kullanarak spesifik bir cariyi hızlıca bulabilirsiniz."
              columns={columns}
              data={data}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default CustomerBalances;