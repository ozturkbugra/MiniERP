import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import html2pdf from 'html2pdf.js';
import api from '../api/axiosInstance'; 

const InvoiceDetail = () => {
  const { id } = useParams(); 
  const navigate = useNavigate();

  const [invoice, setInvoice] = useState<any | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);

  useEffect(() => {
    const fetchInvoiceDetails = async () => {
      try {
        const response = await api.get(`/Invoices/${id}`);
        if (response.data.isSuccess) {
          setInvoice(response.data.data);
        }
      } catch (error) {
        console.error("Fatura detayları çekilemedi", error);
      }
    };

    if (id) {
      fetchInvoiceDetails();
    }
  }, [id]);

  const handleDownloadPdf = () => {
    if (!invoice) return;
    
    setIsGenerating(true);
    const element = document.getElementById('invoice-print-area');

    if (!element) {
      console.error("Yazdırılacak fatura alanı bulunamadı!");
      setIsGenerating(false);
      return;
    }

    const opt: any = {
      margin: 10,
      filename: `${invoice.invoiceNumber}.pdf`,
      image: { type: 'jpeg', quality: 0.98 },
      html2canvas: { 
        scale: 2, 
        useCORS: true, 
        letterRendering: true 
      },
      jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };

    html2pdf().set(opt).from(element).save().then(() => {
      setIsGenerating(false);
    });
  };

  const formatCurrency = (value: number) => 
    new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);

  // 🚀 KDV NORMALLEŞTİRİCİ: 0.2 gelirse 20 yapar, 20 gelirse 20 bırakır.
  const getNormalizedVat = (rate: any) => {
    const numRate = Number(rate) || 0;
    return numRate > 0 && numRate < 1 ? numRate * 100 : numRate;
  };

  if (!invoice) {
    return (
      <div className="p-5 text-center">
        <div className="spinner-border text-primary" role="status"></div>
        <div className="mt-3 text-muted">Fatura yükleniyor...</div>
      </div>
    );
  }

  // 🚀 KDV HESAPLAMA (DİNAMİK)
  const lines = invoice.lines || invoice.items || [];
  
  const calculatedSubTotal = invoice.subTotal || lines.reduce((acc: number, item: any) => {
    return acc + (item.quantity * item.unitPrice);
  }, 0);

  const calculatedVatTotal = invoice.vatTotal || (invoice.grandTotal - calculatedSubTotal);

  return (
    <div className="page-invoice-detail animate__animated animate__fadeIn">
      {/* 🛠️ Üst Panel */}
      <div className="d-flex justify-content-between align-items-center mb-4 no-print">
        <div>
          <h1 className="page-title fs-3 fw-bold">Fatura Görüntüleme</h1>
          <nav className="breadcrumb">
            <span className="breadcrumb-item cursor-pointer text-primary" onClick={() => navigate(-1)}>Faturalar</span>
            <span className="breadcrumb-item active">{invoice.invoiceNumber}</span>
          </nav>
        </div>
        <div>
          <button 
            className="btn btn-primary fw-bold shadow-sm"
            onClick={handleDownloadPdf}
            disabled={isGenerating}
          >
            {isGenerating ? (
              <><span className="spinner-border spinner-border-sm me-2"></span> PDF Hazırlanıyor...</>
            ) : (
              <><i className="bi bi-file-earmark-pdf me-2"></i> PDF İndir / Yazdır</>
            )}
          </button>
        </div>
      </div>

      {/* 📄 PDF'E DÖNÜŞECEK A4 ALANI */}
      <div className="d-flex justify-content-center">
        <div 
          id="invoice-print-area" 
          className="card border-0 shadow-sm" 
          style={{ width: '100%', maxWidth: '850px', minHeight: '1122px', padding: '50px' }}
        >
          {/* HEADER */}
          <div className="row mb-5 pb-3 border-bottom border-2">
            <div className="col-6">
              <h2 className="fw-bolder text-primary mb-1" style={{ letterSpacing: '1px' }}>MINI ERP YAZILIM A.Ş.</h2>
              <p className="text-muted small mb-0">Teknokent Bilişim Vadisi, No: 42</p>
              <p className="text-muted small mb-0">Çankaya / ANKARA</p>
              <p className="text-muted small mb-0">Tel: +90 312 000 00 00</p>
              <p className="text-muted small mb-0">Vergi Dairesi: Çankaya | V.No: 1234567890</p>
            </div>
            <div className="col-6 text-end">
              <h1 className="text-uppercase fw-bold text-muted mb-3" style={{ opacity: 0.3, letterSpacing: '4px' }}>FATURA</h1>
              <table className="table table-sm table-borderless text-end float-end mb-0" style={{ maxWidth: '250px' }}>
                <tbody>
                  <tr>
                    <td className="fw-bold text-muted">Fatura No:</td>
                    <td className="fw-bold">{invoice.invoiceNumber}</td>
                  </tr>
                  <tr>
                    <td className="fw-bold text-muted">Tarih:</td>
                    <td className="">{new Date(invoice.invoiceDate).toLocaleDateString('tr-TR')}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          {/* MÜŞTERİ BİLGİLERİ */}
          <div className="row mb-5">
            <div className="col-7">
              <div className="p-3 rounded bg-light">
                <h6 className="fw-bold text-uppercase text-muted mb-2" style={{ fontSize: '12px' }}>SAYIN (MÜŞTERİ BİLGİLERİ)</h6>
                <h6 className="fw-bold mb-1">{invoice.customerName}</h6>
                {invoice.customerAddress && <p className="small mb-1">{invoice.customerAddress}</p>}
                {(invoice.customerTaxOffice || invoice.customerTaxNo) && (
                  <p className="small mb-0 text-muted">
                    {invoice.customerTaxOffice && <><strong>V.Dairesi:</strong> {invoice.customerTaxOffice} &nbsp;|&nbsp;</>} 
                    {invoice.customerTaxNo && <><strong>V.No:</strong> {invoice.customerTaxNo}</>}
                  </p>
                )}
              </div>
            </div>
          </div>

          {/* FATURA KALEMLERİ (ÜRÜNLER) */}
          <div className="row mb-4 flex-grow-1">
            <div className="col-12">
              <table className="table table-bordered border-dark align-middle">
                <thead className="table-light border-dark">
                  <tr className="small text-center">
                    <th scope="col" style={{ width: '40px' }}>#</th>
                    <th scope="col" className="text-start">Ürün Açıklaması</th>
                    <th scope="col" style={{ width: '80px' }}>Miktar</th>
                    <th scope="col" style={{ width: '100px' }}>Birim Fiyat</th>
                    <th scope="col" style={{ width: '70px' }}>KDV</th>
                    <th scope="col" style={{ width: '120px' }}>Tutar</th>
                  </tr>
                </thead>
                <tbody>
                  {lines.map((item: any, index: number) => {
                    // 🚀 KDV oranını burada çekip ekrana tam sayı olarak basıyoruz
                    const displayVat = getNormalizedVat(item.vatRate);
                    
                    return (
                      <tr key={index} className="small">
                        <td className="text-center fw-bold text-muted">{index + 1}</td>
                        <td className="fw-medium text-start">{item.productName}</td>
                        <td className="text-center">{item.quantity}</td>
                        <td className="text-end">{formatCurrency(item.unitPrice)}</td>
                        {/* Ekrana %20 olarak basıyoruz */}
                        <td className="text-center text-muted">%{displayVat}</td>
                        <td className="text-end fw-bold">
                          {formatCurrency(item.lineTotal || (item.quantity * item.unitPrice * (1 + (displayVat/100))))}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* ALT TOPLAMLAR */}
          <div className="row justify-content-end mb-5">
            <div className="col-5">
              <table className="table table-sm table-borderless">
                <tbody>
                  <tr className="border-bottom">
                    <td className="fw-bold text-muted">Ara Toplam</td>
                    <td className="text-end fw-bold">{formatCurrency(calculatedSubTotal)}</td>
                  </tr>
                  <tr className="border-bottom">
                    <td className="fw-bold text-muted">Toplam KDV</td>
                    <td className="text-end fw-bold">{formatCurrency(calculatedVatTotal)}</td>
                  </tr>
                  <tr>
                    <td className="fw-bolder fs-5 text-primary pt-3">GENEL TOPLAM</td>
                    <td className="text-end fw-bolder fs-5 text-primary pt-3">{formatCurrency(invoice.grandTotal)}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          {/* FOOTER (Notlar) */}
          <div className="row mt-auto pt-4 border-top">
            <div className="col-12 text-center text-muted small">
              <p className="mb-0">Bu fatura Mini ERP Sistemi tarafından elektronik olarak oluşturulmuştur.</p>
              <p className="mb-0">Bizi tercih ettiğiniz için teşekkür ederiz.</p>
            </div>
          </div>
          
        </div>
      </div>
    </div>
  );
};

export default InvoiceDetail;