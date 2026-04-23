import { useState, useEffect, useCallback, useMemo } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';
import AppSelect from '../../components/Common/AppSelect';
import type { SelectOption } from '../../components/Common/AppSelect';

// 🚀 Backend'den dönen Cari Ekstresi Modeli
interface CustomerStatementRow {
  id: string; // DataTable için mapped id
  transactionId: string;
  date: string;
  description: string;
  debit: number;   // Borç (Müşterinin bize borçlandığı tutar)
  credit: number;  // Alacak (Müşterinin ödediği/alacaklandığı tutar)
  balance: number; // Kalan Bakiye
}

const CustomerStatement = () => {
  const { hasPermission } = useAuthStore();
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  const [selectedCustomerId, setSelectedCustomerId] = useState<string>("");
  const [customers, setCustomers] = useState<SelectOption[]>([]);
  const [statementData, setStatementData] = useState<CustomerStatementRow[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // 1. Müşteri Listesini Çekme (Dropdown için CustomerBalances kullanıyoruz)
  const fetchCustomers = useCallback(async () => {
    try {
      const response = await api.get("/Transactions/CustomerBalances");
      if (response.data.isSuccess) {
        const options = response.data.data.map((c: any) => ({
          value: c.id,
          label: c.name
        }));
        setCustomers(options);
      }
    } catch (error) {
      console.error("Cari listesi alınamadı");
    }
  }, []);

  // 2. Seçili Carinin Ekstresini Çekme
  const fetchStatement = useCallback(async () => {
    if (!selectedCustomerId) return;

    try {
      setLoading(true);
      const response = await api.get(`/Transactions/CustomerStatement/${selectedCustomerId}`);

      if (response.data.isSuccess) {
        const mappedData = response.data.data.map((item: any, index: number) => ({
          ...item,
          id: item.transactionId || `stmt-${index}`
        }));
        setStatementData(mappedData);
      }
    } catch (error) {
      console.error("Cari ekstresi çekilemedi");
      setStatementData([]);
    } finally {
      setLoading(false);
    }
  }, [selectedCustomerId]);

  useEffect(() => {
    fetchCustomers();
  }, [fetchCustomers]);

  const formatCurrency = (value: number) => 
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);

  // 📊 Tablo Sütunları
  const columns = [
    { 
      header: "TARİH", 
      accessor: (item: CustomerStatementRow) => (
        <span className="small fw-medium">
          {new Date(item.date).toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric' })}
        </span>
      )
    },
    { 
      header: "İŞLEM AÇIKLAMASI", 
      accessor: (item: CustomerStatementRow) => (
        <span className="small fw-medium text-wrap" style={{maxWidth: '250px', display: 'block'}}>
          {item.description || "Muhtelif İşlem"}
        </span>
      )
    },
    { 
      header: "BORÇ (+)", 
      className: "text-end",
      accessor: (item: CustomerStatementRow) => (
        <span className={item.debit > 0 ? "fw-bold" : "text-muted"}>
          {item.debit > 0 ? formatCurrency(item.debit) : "-"}
        </span>
      )
    },
    { 
      header: "ALACAK (-)", 
      className: "text-end",
      accessor: (item: CustomerStatementRow) => (
        <span className={item.credit > 0 ? "fw-bold text-success" : "text-muted"}>
          {item.credit > 0 ? formatCurrency(item.credit) : "-"}
        </span>
      )
    },
    { 
      header: "BAKİYE", 
      className: "text-end pe-4 fs-6 fw-bold", 
      accessor: (item: CustomerStatementRow) => {
        // Bakiye gösterimi: Artı ise müşteri borçlu (Bize ait varlık), Eksi ise biz borçluyuz
        const isWeOwe = item.balance < 0;
        return (
          <span className={isWeOwe ? "text-danger" : "text-primary"}>
             {/* Eksi işaretini formatın içinde otomatik bırakıyoruz veya mutlak değer kullanabiliriz */}
             {formatCurrency(item.balance)}
          </span>
        );
      }
    }
  ];

  if (!canViewReports) return <div className="alert alert-warning m-4">Bu raporu görüntüleme yetkiniz bulunmamaktadır.</div>;

  return (
    <div className="page-customer-statement animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-4 fw-bold">Cari Hesap Ekstresi</h1>
        <p className="text-muted small">Seçilen müşteri veya tedarikçinin tüm fatura ve ödeme hareketlerini kronolojik olarak inceleyin.</p>
      </div>

      {/* 🔍 Filtre Paneli */}
      <div className="card shadow-sm border-0 mb-4">
        <div className="card-body py-4">
          <div className="row g-3 align-items-end">
            <div className="col-md-9">
              <AppSelect 
                label="Cari Seçimi"
                placeholder="Ekstresini görmek istediğiniz cariyi arayın..."
                isSearchable={true}
                options={customers}
                value={selectedCustomerId}
                onChange={(val) => setSelectedCustomerId(val)}
              />
            </div>
            <div className="col-md-3">
              <button 
                className="btn btn-primary w-100 fw-bold shadow-sm"
                onClick={fetchStatement}
                disabled={loading || !selectedCustomerId}
              >
                {loading ? <span className="spinner-border spinner-border-sm me-2"></span> : <i className="bi bi-file-earmark-spreadsheet me-2"></i>}
                Ekstre Çıkar
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* 📊 Veri Tablosu */}
      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
          <div className="mt-2 text-muted small">Cari hareketler hesaplanıyor...</div>
        </div>
      ) : selectedCustomerId && (
        <DataTable<CustomerStatementRow>
          title="Hesap Hareket Dökümü"
          description="Borç sütunu müşterinin size olan borcunu, Alacak sütunu ise yaptığı ödemeleri/iadeyi ifade eder."
          columns={columns}
          data={statementData}
        />
      )}
    </div>
  );
};

export default CustomerStatement;