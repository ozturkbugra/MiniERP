import React, { useEffect, useState, useRef } from 'react';
// import axios from 'axios'; 

interface MonthlySales {
  month: string;
  amount: number;
}

interface DashboardSummary {
  totalSales: number;
  totalStockValue: number;
  totalCashBankBalance: number;
  monthlySales: MonthlySales[];
}

const Dashboard: React.FC = () => {
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  
  // 🚀 EKLENDİ: Grafiğin HTML içine basılacağı yerin referansı
  const chartRef = useRef<HTMLDivElement>(null); 

  // 1. API'den Veri Çekme
  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        // Şimdilik mock veri
        setData({
          totalSales: 981035,
          totalStockValue: 650452410,
          totalCashBankBalance: 35672.47,
          monthlySales: [
            { month: "Oca", amount: 200000 },
            { month: "Şub", amount: 250000 },
            { month: "Mar", amount: 344880 },
            { month: "Nis", amount: 636155 }
          ]
        });
      } catch (error) {
        console.error("Dashboard verisi çekilemedi", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  // 🚀 2. EKLENDİ: Veri geldikten sonra Script ile Grafiği Çizme
  useEffect(() => {
    // Veri yüklendiyse ve çizeceğimiz div hazırsa
    if (!loading && data && chartRef.current) {
      
      const chartOptions = {
        series: [{
          name: 'Aylık Ciro',
          data: data.monthlySales.map(item => item.amount) // Y Eksenindeki veriler
        }],
        chart: {
          type: 'area',
          height: 350,
          toolbar: { show: false },
          fontFamily: 'inherit'
        },
        colors: ['#4154f1'], // NiceAdmin primary rengi
        fill: {
          type: 'gradient',
          gradient: {
            shadeIntensity: 1,
            opacityFrom: 0.4,
            opacityTo: 0.0,
            stops: [0, 100]
          }
        },
        dataLabels: { enabled: false },
        stroke: { curve: 'smooth', width: 3 },
        xaxis: {
          categories: data.monthlySales.map(item => item.month), // X Eksenindeki aylar
          labels: { style: { colors: '#899bbd' } }
        },
        yaxis: {
          labels: {
            style: { colors: '#899bbd' },
            formatter: (value: number) => `${value.toLocaleString('tr-TR')} ₺`
          }
        },
        tooltip: {
          y: { formatter: (value: number) => `${value.toLocaleString('tr-TR')} ₺` }
        }
      };

      // 🚀 assets klasöründen gelen global ApexCharts objesini çağırıyoruz
      // TypeScript kızmasın diye (window as any) yapıyoruz
      const chart = new (window as any).ApexCharts(chartRef.current, chartOptions);
      chart.render();

      // Component ekrandan kalktığında hafızadan (memory) grafiği temizliyoruz
      return () => {
        chart.destroy();
      };
    }
  }, [loading, data]);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
  };

  if (loading || !data) {
    return <div className="text-center mt-5"><div className="spinner-border text-primary" role="status"></div></div>;
  }

  return (
    <div className="main-content">
      <div className="page-header mb-4">
        <h1 className="page-title fs-3 fw-bold">Dashboard</h1>
      </div>

      {/* ÜST KARTLAR (TOP CARDS) */}
      <div className="row g-4 mb-4">
        <div className="col-xxl-4 col-md-6">
          <div className="card info-card sales-card h-100 border-0 shadow-sm">
            <div className="card-body">
              <h5 className="card-title text-muted fw-semibold mb-3">Toplam Ciro</h5>
              <div className="d-flex align-items-center">
                <div className="card-icon rounded-circle d-flex align-items-center justify-content-center bg-primary bg-opacity-10 text-primary" style={{ width: '60px', height: '60px', fontSize: '28px' }}>
                  <i className="bi bi-cart-check"></i>
                </div>
                <div className="ps-3">
                  <h4 className="fw-bold mb-0">{formatCurrency(data.totalSales)}</h4>
                  <span className="text-success small pt-1 fw-bold">Sistem geneli</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xxl-4 col-md-6">
          <div className="card info-card revenue-card h-100 border-0 shadow-sm">
            <div className="card-body">
              <h5 className="card-title text-muted fw-semibold mb-3">Depo Stok Değeri</h5>
              <div className="d-flex align-items-center">
                <div className="card-icon rounded-circle d-flex align-items-center justify-content-center bg-success bg-opacity-10 text-success" style={{ width: '60px', height: '60px', fontSize: '28px' }}>
                  <i className="bi bi-box-seam"></i>
                </div>
                <div className="ps-3">
                  <h4 className="fw-bold mb-0">{formatCurrency(data.totalStockValue)}</h4>
                  <span className="text-muted small pt-1">Anlık fiziksel bakiye</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xxl-4 col-xl-12">
          <div className="card info-card customers-card h-100 border-0 shadow-sm">
            <div className="card-body">
              <h5 className="card-title text-muted fw-semibold mb-3">Kasa & Banka</h5>
              <div className="d-flex align-items-center">
                <div className="card-icon rounded-circle d-flex align-items-center justify-content-center bg-warning bg-opacity-10 text-warning" style={{ width: '60px', height: '60px', fontSize: '28px' }}>
                  <i className="bi bi-wallet2"></i>
                </div>
                <div className="ps-3">
                  <h4 className="fw-bold mb-0">{formatCurrency(data.totalCashBankBalance)}</h4>
                  <span className="text-muted small pt-1">Toplam Nakit Varlık</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* AYLIK SATIŞ GRAFİĞİ */}
      <div className="row">
        <div className="col-12">
          <div className="card border-0 shadow-sm">
            <div className="card-header border-bottom-0 pt-4 pb-0">
              <h5 className="card-title fw-bold mb-0">Aylık Satış Trendi</h5>
              <span className="text-muted small">Son 6 Aylık Ciro Dağılımı</span>
            </div>
            <div className="card-body">
              {/* 🚀 EKLENDİ: React component'i (<Chart />) yerine düz div koyduk, ref bağladık */}
              <div ref={chartRef} style={{ minHeight: '350px' }}></div>
            </div>
          </div>
        </div>
      </div>

    </div>
  );
};

export default Dashboard;