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

  const filteredData = data.filter(item => 
    JSON.stringify(item).toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="card">
      <div className="card-header d-flex justify-content-between align-items-center">
        <div>
          <h5 className="card-title mb-0">{title}</h5>
          {description && <p className="card-subtitle mt-1">{description}</p>}
        </div>
        {onAdd && (
          <button onClick={onAdd} className="btn btn-primary btn-sm">
            <i className="bi bi-plus-lg me-1"></i> {addText}
          </button>
        )}
      </div>
      
      <div className="card-body pt-0">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <div className="search-form d-flex align-items-center" style={{ width: '300px' }}>
            <i className="bi bi-search me-2 text-muted"></i>
            <input 
              type="text" 
              className="form-control form-control-sm border-0 bg-light" 
              placeholder="Tabloda ara..." 
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>

        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead>
              <tr>
                {columns.map((col, idx) => (
                  <th key={idx} className={col.className}>{col.header}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filteredData.map((item) => (
                <tr key={item.id}>
                  {columns.map((col, idx) => (
                    <td key={idx} className={col.className}>
                      {typeof col.accessor === 'function' 
                        ? col.accessor(item) 
                        : (item[col.accessor] as React.ReactNode)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="card-footer bg-transparent border-top-0 py-3">
        <div className="d-flex justify-content-between align-items-center">
          <span className="text-muted small">Toplam {filteredData.length} kayıt gösteriliyor</span>
          <nav>
            <ul className="pagination pagination-sm mb-0">
              <li className="page-item disabled"><a className="page-link">Önceki</a></li>
              <li className="page-item active"><a className="page-link">1</a></li>
              <li className="page-item"><a className="page-link">Sonraki</a></li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  );
};

export default DataTable;