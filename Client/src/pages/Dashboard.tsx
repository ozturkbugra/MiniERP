import React, { useEffect, useState, useRef, useCallback } from 'react';
import api from '../api/axiosInstance';
import DataTable from '../components/Common/DataTable';

// --- INTERFACES ---
interface MonthlySales {
  month: string;
  amount: number;
}

interface RecentActivity {
  id: string;
  date: string;
  description: string;
  type: string;
  statusColor: string;
}

interface TopDebtor {
  customerId: string;
  customerName: string;
  balance: number;
}

interface DashboardState {
  totalSales: number;
  totalStockValue: number;
  totalCashBankBalance: number;
  monthlySales: MonthlySales[];
  recentActivities: RecentActivity[];
  topDebtors: TopDebtor[];
}

const Dashboard: React.FC = () => {
  const [data, setData] = useState<DashboardState | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  
  // Grafikler için Ref'ler
  const areaChartRef = useRef<HTMLDivElement>(null); 
  const donutChartRef = useRef<HTMLDivElement>(null);

  // 1. API'lerden Veri Çekme (Dual Fetch)
  const fetchDashboardData = useCallback(async () => {
    try {
      setLoading(true);
      // 🚀 İki farklı endpoint'e aynı anda mermi atıyoruz
      const [dashRes, reportsRes] = await Promise.all([
        api.get('/Dashboards/Summary'),
        api.get('/Reports/dashboard-summary')
      ]);

      if (dashRes.data.isSuccess && reportsRes.data.isSuccess) {
        const dashData = dashRes.data.data;
        const reportData = reportsRes.data.data;

        setData({
          totalSales: dashData.totalSales,
          totalStockValue: dashData.totalStockValue,
          totalCashBankBalance: dashData.totalCashBankBalance,
          monthlySales: dashData.monthlySales,
          // DataTable için activity'lere id ekliyoruz
          recentActivities: dashData.recentActivities.map((x: any, i: number) => ({
            ...x,
            id: `act-${i}`
          })),
          // Borçlu listesini Reports endpoint'inden alıyoruz
          topDebtors: reportData.topDebtors
        });
      }
    } catch (error) {
      console.error("Dashboard verileri çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboardData();
  }, [fetchDashboardData]);

  // 2. ApexCharts Çizimi
  useEffect(() => {
    if (!loading && data) {
      let areaChart: any;
      let donutChart: any;

      // --- CİRO ALAN GRAFİĞİ ---
      if (areaChartRef.current) {
        const areaOptions = {
          series: [{ name: 'Aylık Ciro', data: data.monthlySales.map(item => item.amount) }],
          chart: { type: 'area', height: 350, toolbar: { show: false }, fontFamily: 'inherit' },
          colors: ['#4154f1'],
          stroke: { curve: 'smooth', width: 3 },
          xaxis: { categories: data.monthlySales.map(item => item.month) },
          yaxis: { labels: { formatter: (val: number) => val.toLocaleString('tr-TR') + " ₺" } }
        };
        areaChart = new (window as any).ApexCharts(areaChartRef.current, areaOptions);
        areaChart.render();
      }

      // --- BORÇLU PASTASI (Donut) ---
      if (donutChartRef.current && data.topDebtors.length > 0) {
        const donutOptions = {
          series: data.topDebtors.map(d => d.balance),
          labels: data.topDebtors.map(d => d.customerName),
          chart: { type: 'donut', height: 350, fontFamily: 'inherit' },
          colors: ['#4154f1', '#2eca6a', '#ff771d', '#e83e8c', '#0dcaf0'],
          legend: { position: 'bottom' },
          plotOptions: {
            pie: {
              donut: {
                size: '65%',
                labels: {
                  show: true,
                  total: {
                    show: true,
                    label: 'Toplam Borç',
                    formatter: () => data.topDebtors.reduce((a, b) => a + b.balance, 0).toLocaleString('tr-TR') + " ₺"
                  }
                }
              }
            }
          }
        };
        donutChart = new (window as any).ApexCharts(donutChartRef.current, donutOptions);
        donutChart.render();
      }

      return () => {
        if (areaChart) areaChart.destroy();
        if (donutChart) donutChart.destroy();
      };
    }
  }, [loading, data]);

  // Yardımcı Formatlayıcılar
  const formatCurrency = (val: number) => 
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);

  // DataTable Sütunları (Son Hareketler için)
  const activityColumns = [
    { 
      header: "TARİH", 
      accessor: (item: RecentActivity) => new Date(item.date).toLocaleString('tr-TR', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' }) 
    },
    { 
      header: "AÇIKLAMA", 
      accessor: (item: RecentActivity) => (
        <span className={`fw-medium ${item.statusColor}`}>
          <i className="bi bi-info-circle me-2"></i>
          {item.description}
        </span>
      )
    },
    { 
      header: "TİP", 
      accessor: (item: RecentActivity) => <span className="badge bg-secondary-subtle text-secondary border border-secondary-subtle px-2">{item.type}</span> 
    }
  ];

  if (loading || !data) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{height: '80vh'}}>
        <div className="spinner-border text-primary" role="status"></div>
      </div>
    );
  }

  return (
    <div className="main-content animate__animated animate__fadeIn">
      <div className="page-header mb-4">
        <h1 className="page-title fs-3 fw-bold">Komuta Merkezi</h1>
      </div>

      {/* ÜST KPI KARTLARI */}
      <div className="row g-4 mb-4">
        <div className="col-xxl-4 col-md-6">
          <div className="card border-0 shadow-sm">
            <div className="card-body p-4">
              <h5 className="card-title text-muted small fw-bold text-uppercase mb-3">Toplam Ciro</h5>
              <div className="d-flex align-items-center">
                <div className="bg-primary bg-opacity-10 rounded-circle p-3 fs-3 me-3">
                  <i className="bi bi-graph-up-arrow"></i>
                </div>
                <h3 className="fw-bold mb-0">{formatCurrency(data.totalSales)}</h3>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xxl-4 col-md-6">
          <div className="card border-0 shadow-sm">
            <div className="card-body p-4">
              <h5 className="card-title text-muted small fw-bold text-uppercase mb-3">Stok Değeri</h5>
              <div className="d-flex align-items-center">
                <div className="bg-success bg-opacity-10 rounded-circle p-3 fs-3 me-3">
                  <i className="bi bi-box-seam"></i>
                </div>
                <h3 className="fw-bold mb-0">{formatCurrency(data.totalStockValue)}</h3>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xxl-4 col-xl-12">
          <div className="card border-0 shadow-sm">
            <div className="card-body p-4">
              <h5 className="card-title text-muted small fw-bold text-uppercase mb-3">Kasa & Banka</h5>
              <div className="d-flex align-items-center">
                <div className="bg-warning bg-opacity-10 rounded-circle p-3 fs-3 me-3">
                  <i className="bi bi-wallet2"></i>
                </div>
                <h3 className="fw-bold mb-0">{formatCurrency(data.totalCashBankBalance)}</h3>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* GRAFİKLER */}
      <div className="row g-4 mb-4">
        <div className="col-lg-8">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-header bg-transparent border-0 pt-4 px-4">
              <h5 className="card-title fw-bold mb-0">Satış Analizi</h5>
              <span className="text-muted small">Son 6 Aylık Performans</span>
            </div>
            <div className="card-body">
              <div ref={areaChartRef} style={{ minHeight: '350px' }}></div>
            </div>
          </div>
        </div>

        <div className="col-lg-4">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-header bg-transparent border-0 pt-4 px-4">
              <h5 className="card-title fw-bold mb-0">Riskli Alacaklar</h5>
              <span className="text-muted small">En Çok Borcu Olan 5 Cari</span>
            </div>
            <div className="card-body d-flex align-items-center">
              {data.topDebtors.length > 0 ? (
                <div ref={donutChartRef} className="w-100"></div>
              ) : (
                <div className="text-center w-100 text-muted">Riskli alacak kaydı bulunmuyor.</div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* SON İŞLEMLER DATATABLE */}
      <div className="row">
        <div className="col-12">
          <DataTable<RecentActivity>
            title="Sistem Hareketleri"
            description="En son gerçekleştirilen fatura ve stok işlemleri."
            columns={activityColumns}
            data={data.recentActivities}
          />
        </div>
      </div>
    </div>
  );
};

export default Dashboard;