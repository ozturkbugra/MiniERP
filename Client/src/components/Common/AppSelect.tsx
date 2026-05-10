import React, { useState, useRef, useEffect } from 'react';

export interface SelectOption {
  value: string | number;
  label: string;
}

interface AppSelectProps {
  id?: string;
  label?: string;
  options: SelectOption[];
  value?: string | string[] | null;
  onChange: (value: any) => void;
  isSearchable?: boolean;
  placeholder?: string;
}

const AppSelect: React.FC<AppSelectProps> = ({
  id,
  label,
  options,
  value,
  onChange,
  isSearchable = false,
  placeholder = "Seçim yapınız..."
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const wrapperRef = useRef<HTMLDivElement>(null);

  // Ekranda (Modal dışında) bir yere tıklanırsa menüyü kapat
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [wrapperRef]);

  // Arama filtresi
  const filteredOptions = options.filter(opt => 
  opt?.label?.toString().toLowerCase().includes(searchTerm.toLowerCase())
);

  // Seçili elemanın adını bul
  const selectedLabel = options.find(opt => opt.value === value)?.label || placeholder;

  return (
    <div className="form-group mb-3 position-relative" ref={wrapperRef}>
      {/* Label'dan text-light silindi, tema karar verecek */}
      {label && <label className="form-label fw-semibold small" htmlFor={id}>{label}</label>}
      
      {/* 🚀 Sahte (Custom) Select Kutusu - Temanın form-select class'ı renkleri halleder */}
      <div 
        className={`form-select d-flex justify-content-between align-items-center cursor-pointer ${isOpen ? 'shadow-sm border-primary' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        style={{ cursor: 'pointer', minHeight: '38px' }}
      >
        <span className={!value ? "text-muted" : ""}>{selectedLabel}</span>
      </div>

      {/* 🚀 Açılır Menü (Dropdown) - Bootstrap'ın dropdown-menu class'ı kullanıldı */}
      {isOpen && (
        <div 
          className="dropdown-menu show w-100 position-absolute shadow-lg p-0 mt-1" 
          style={{ zIndex: 9999, display: 'flex', flexDirection: 'column' }}
        >
          {/* Arama Kutusu */}
          {isSearchable && (
            <div className="p-2 border-bottom">
              <input 
                type="text" 
                className="form-control form-control-sm" 
                placeholder="Ara..." 
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                autoFocus 
              />
            </div>
          )}

          {/* Seçenekler Listesi */}
          <div style={{ overflowY: 'auto', maxHeight: '250px' }} className="py-1">
            
            {/* Placeholder / Seçimi Temizle */}
            <div 
              className="dropdown-item text-muted small"
              style={{ cursor: 'pointer', borderBottom: '1px solid var(--bs-border-color)' }}
              onClick={() => { onChange(""); setIsOpen(false); setSearchTerm(""); }}
            >
              {placeholder}
            </div>

            {filteredOptions.length > 0 ? (
              filteredOptions.map((opt, index) => (
                <div 
                  key={index}
                  // Seçili eleman için Bootstrap'ın 'active' class'ını kullandık
                  className={`dropdown-item ${value === opt.value ? 'active' : ''}`}
                  style={{ cursor: 'pointer' }}
                  onClick={() => {
                    onChange(opt.value);
                    setIsOpen(false);
                    setSearchTerm(""); 
                  }}
                >
                  {opt.label}
                </div>
              ))
            ) : (
              <div className="dropdown-item text-muted small text-center pointer-events-none">Sonuç bulunamadı</div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default AppSelect;