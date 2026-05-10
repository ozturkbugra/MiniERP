import { useState, useEffect, useCallback, useMemo } from 'react';
import api from '../../api/axiosInstance';
import { useAuthStore } from '../../store/useAuthStore';
import { APP_PERMISSIONS } from '../../constants/permissions';
import DataTable from '../../components/Common/DataTable';
import AppSelect from '../../components/Common/AppSelect';
import type { SelectOption } from '../../components/Common/AppSelect';

// 🚀 Backend'den dönen Ekstre modeli
interface StatementRow {
  id: string; // DataTable için
  transactionId: string;
  date: string;
  description: string;
  debit: number;   // Hesaba Giren (Borç)
  credit: number;  // Hesaptan Çıkan (Alacak)
  balance: number; // Kalan Bakiye
}

// 🚀 Dropdown'da Kasa mı Banka mı ayırabilmek için özel tip
interface AccountOption extends SelectOption {
  isBank: boolean;
}

const AccountStatements = () => {
  const { hasPermission } = useAuthStore();
  // 🚀 Senin kararınla yetkiyi Reports.View olarak ayarladık!
  const canViewReports = hasPermission(APP_PERMISSIONS.Reports?.View);

  const [selectedAccountId, setSelectedAccountId] = useState<string>("");
  const [accounts, setAccounts] = useState<AccountOption[]>([]);
  const [statementData, setStatementData] = useState<StatementRow[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  // 1. Hesapları (Kasa ve Bankalar) Çekme
  const fetchAccounts = useCallback(async () => {
    try {
      // 🚀 Senin FinancialStatus endpoint'inden Kasa ve Bankaları alıyoruz
      const response = await api.get("/Transactions/FinancialStatus");
      if (response.data.isSuccess) {
        const { cashBalances, bankBalances } = response.data.data;
        
        const cashOptions = cashBalances.map((c: any) => ({
          value: c.id,
          label: `[KASA] ${c.name}`,
          isBank: false
        }));

        const bankOptions = bankBalances.map((b: any) => ({
          value: b.id,
          label: `[BANKA] ${b.name}`,
          isBank: true
        }));

        setAccounts([...cashOptions, ...bankOptions]);
      }
    } catch (error) {
      console.error("Hesap listesi alınamadı");
    }
  }, []);

  // 2. Ekstreyi Çekme
  const fetchStatement = useCallback(async () => {
    if (!selectedAccountId) return;
    
    // Seçilen hesabın Banka mı Kasa mı olduğunu buluyoruz
    const selectedAccount = accounts.find(a => a.value === selectedAccountId);
    if (!selectedAccount) return;

    try {
      setLoading(true);
      // Banka ise BankStatement, Kasa ise CashStatement endpoint'ine gidiyoruz
      const endpoint = selectedAccount.isBank 
        ? `/Transactions/BankStatement/${selectedAccountId}` 
        : `/Transactions/CashStatement/${selectedAccountId}`;

      const response = await api.get(endpoint);

      if (response.data.isSuccess) {
        const mappedData = response.data.data.map((item: any, index: number) => ({
          ...item,
          id: item.transactionId || `stmt-${index}`
        }));
        setStatementData(mappedData);
      }
    } catch (error) {
      console.error("Hesap ekstresi çekilemedi");
      setStatementData([]);
    } finally {
      setLoading(false);
    }
  }, [selectedAccountId, accounts]);

  useEffect(() => {
    fetchAccounts();
  }, [fetchAccounts]);

  const formatCurrency = (value: number) => 
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);

  // 📊 Tablo Sütunları
  const columns = [
    { 
      header: "TARİH", 
      accessor: (item: StatementRow) => (
        <span className="small fw-medium">
          {new Date(item.date).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
        </span>
      )
    },
    { 
      header: "İŞLEM AÇIKLAMASI", 
      accessor: "description" as keyof StatementRow,
      className: "small fw-medium"
    },
    { 
      header: "GİRİŞ (BORÇ)", 
      className: "text-end",
      accessor: (item: StatementRow) => (
        <span className={item.debit > 0 ? "text-success fw-bold" : "text-muted"}>
          {item.debit > 0 ? `+${formatCurrency(item.debit)}` : "-"}
        </span>
      )
    },
    { 
      header: "ÇIKIŞ (ALACAK)", 
      className: "text-end",
      accessor: (item: StatementRow) => (
        <span className={item.credit > 0 ? "text-danger fw-bold" : "text-muted"}>
          {item.credit > 0 ? `-${formatCurrency(item.credit)}` : "-"}
        </span>
      )
    },
    { 
      header: "BAKİYE", 
      className: "text-end pe-4 fw-bold", 
      accessor: (item: StatementRow) => (
        <span className={item.balance < 0 ? "text-danger" : ""}>
          {formatCurrency(item.balance)}
        </span>
      )
    }
  ];

  if (!canViewReports) return <div className="alert alert-warning m-4">Bu raporu görüntüleme yetkiniz bulunmamaktadır.</div>;

  return (
    <div className="page-account-statements animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-4 fw-bold">Hesap Ekstreleri</h1>
        <p className="text-muted small">Kasa ve Banka hesaplarınızın tüm hesap hareketlerini ve bakiye geçmişini inceleyin.</p>
      </div>

      <div className="card shadow-sm border-0 mb-4">
        <div className="card-body py-4">
          <div className="row g-3 align-items-end">
            <div className="col-md-9">
              <AppSelect 
                label="Hesap Seçimi (Kasa veya Banka)"
                placeholder="Ekstresini görmek istediğiniz hesabı seçin..."
                isSearchable={true}
                options={accounts}
                value={selectedAccountId}
                onChange={(val) => setSelectedAccountId(val)}
              />
            </div>
            <div className="col-md-3">
              <button 
                className="btn btn-primary w-100 fw-bold shadow-sm"
                onClick={fetchStatement}
                disabled={loading || !selectedAccountId}
              >
                {loading ? <span className="spinner-border spinner-border-sm me-2"></span> : <i className="bi bi-card-list me-2"></i>}
                Ekstre Dök
              </button>
            </div>
          </div>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-5">
          <div className="spinner-border text-primary" role="status"></div>
          <div className="mt-2 text-muted small">Hesap hareketleri derleniyor...</div>
        </div>
      ) : selectedAccountId && (
        <DataTable<StatementRow>
          title="Ekstre Detayı"
          description="Seçili hesabın geçmişten günümüze kronolojik işlem dökümü."
          columns={columns}
          data={statementData}
        />
      )}
    </div>
  );
};

export default AccountStatements;