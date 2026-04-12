// src/components/Common/DataTable.tsx
import React, { useState, useMemo } from 'react';

interface Column<T> {
  header: string;
  accessor: keyof T | ((item: T) => React.ReactNode);
  className?: string;
}

interface DataTableProps<T> {
  title: string;
  description?: string;
  columns: Column<T>[];
  data: T[];
  onAdd?: () => void;
  addText?: string;
}

const DataTable = <T extends { id: any }>({ 
  title, description, columns, data, onAdd, addText = "Yeni Ekle" 
}: DataTableProps<T>) => {
  // 📝 State Tanımları
  const [searchTerm, setSearchTerm] = useState("");
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(5); // Varsayılan 5 satır

  // 🔍 1. Arama Filtresi
  const filteredData = useMemo(() => {
    return data.filter(item => 
      JSON.stringify(item).toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [data, searchTerm]);

  // 📏 2. Sayfalama Hesaplamaları
  const totalPages = Math.ceil(filteredData.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const paginatedData = filteredData.slice(startIndex, startIndex + itemsPerPage);

  // Sayfa değişiminde en üste kaydırma veya resetleme için yardımcılar
  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setCurrentPage(1); // Arama yapıldığında 1. sayfaya dön
  };

  return (
    <div className="card shadow-sm border-0">
      {/* Kart Başlığı ve Ekleme Butonu */}
      <div className="card-header d-flex justify-content-between align-items-center bg-transparent border-bottom-0 pt-4 px-4">
        <div>
          <h5 className="card-title fw-bold mb-1">{title}</h5>
          {description && <p className="card-subtitle text-muted small">{description}</p>}
        </div>
        {onAdd && (
          <button onClick={onAdd} className="btn btn-primary btn-sm px-3 shadow-sm">
            <i className="bi bi-plus-lg me-1"></i> {addText}
          </button>
        )}
      </div>
      
      <div className="card-body px-4">
        {/* Arama ve Sayfa Başına Satır Sayısı Seçimi */}
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div className="d-flex align-items-center gap-2">
            <select 
              className="form-select form-select-sm w-auto border-0 bg-light"
              value={itemsPerPage}
              onChange={(e) => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }}
            >
              <option value={5}>5</option>
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
            <span className="text-muted small">satır göster</span>
          </div>

          <div className="input-group input-group-sm" style={{ maxWidth: '250px' }}>
            <span className="input-group-text bg-light border-0"><i className="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              className="form-control bg-light border-0" 
              placeholder="Tabloda ara..." 
              value={searchTerm}
              onChange={handleSearchChange}
            />
          </div>
        </div>

        {/* Tablo Alanı */}
        <div className="table-responsive">
          <table className="table table-hover align-middle custom-table">
            <thead>
              <tr>
                {columns.map((col, idx) => (
                  <th key={idx} className={`text-uppercase small fw-bold text-muted ${col.className || ''}`}>{col.header}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {paginatedData.length > 0 ? paginatedData.map((item) => (
                <tr key={item.id}>
                  {columns.map((col, idx) => (
                    <td key={idx} className={col.className}>
                      {typeof col.accessor === 'function' 
                        ? col.accessor(item) 
                        : (item[col.accessor] as React.ReactNode)}
                    </td>
                  ))}
                </tr>
              )) : (
                <tr>
                  <td colSpan={columns.length} className="text-center py-5 text-muted">
                    Aranan kriterlere uygun kayıt bulunamadı.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Sayfalama Kontrolleri */}
      <div className="card-footer bg-transparent border-top-0 py-3 px-4">
        <div className="d-flex justify-content-between align-items-center">
          <span className="text-muted small">
            <strong>{filteredData.length}</strong> kayıttan <strong>{startIndex + 1}</strong> - <strong>{Math.min(startIndex + itemsPerPage, filteredData.length)}</strong> arası gösteriliyor
          </span>
          
          <nav>
            <ul className="pagination pagination-sm mb-0 shadow-none">
              <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}>
                <button className="page-link" onClick={() => handlePageChange(currentPage - 1)}>
                  <i className="bi bi-chevron-left small"></i>
                </button>
              </li>
              
              {/* Sayfa Numaraları (Basit döngü) */}
              {[...Array(totalPages)].map((_, i) => (
                <li key={i} className={`page-item ${currentPage === i + 1 ? 'active' : ''}`}>
                  <button className="page-link" onClick={() => handlePageChange(i + 1)}>{i + 1}</button>
                </li>
              ))}

              <li className={`page-item ${currentPage === totalPages || totalPages === 0 ? 'disabled' : ''}`}>
                <button className="page-link" onClick={() => handlePageChange(currentPage + 1)}>
                  <i className="bi bi-chevron-right small"></i>
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  );
};

export default DataTable;