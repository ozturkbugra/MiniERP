// src/components/Common/DataTable.tsx
import React, { useState } from 'react';

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
  const [searchTerm, setSearchTerm] = useState("");

  // Arama filtresi: Tüm kolonlarda arama yapar
  const filteredData = data.filter(item => 
    JSON.stringify(item).toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="card shadow-sm border-0">
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
        <div className="d-flex mb-4">
          <div className="input-group input-group-sm" style={{ maxWidth: '250px' }}>
            <span className="input-group-text bg-light border-0"><i className="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              className="form-control bg-light border-0" 
              placeholder="Tabloda ara..." 
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>

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
              {filteredData.length > 0 ? filteredData.map((item) => (
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
                  <td colSpan={columns.length} className="text-center py-5 text-muted">Aranan kriterlere uygun kayıt bulunamadı.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card-footer bg-transparent border-top-0 py-3 px-4">
        <div className="d-flex justify-content-between align-items-center">
          <span className="text-muted small">Toplam <strong>{filteredData.length}</strong> kayıt listeleniyor</span>
          <nav>
            <ul className="pagination pagination-sm mb-0">
              <li className="page-item disabled"><button className="page-link">Önceki</button></li>
              <li className="page-item active"><button className="page-link">1</button></li>
              <li className="page-item disabled"><button className="page-link">Sonraki</button></li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  );
};

export default DataTable;