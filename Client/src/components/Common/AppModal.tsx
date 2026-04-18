import React from 'react';

interface AppModalProps {
  show: boolean;
  title: string;
  size?: 'sm' | 'lg' | 'xl';
  onClose: () => void;
  onSave?: () => void;
  children: React.ReactNode;
  saveButtonText?: string;
  isDanger?: boolean;
  // 🚀 YENİ: Butonun görünüp görünmeyeceğini belirleyen prop (default: true)
  saveButton?: boolean; 
}

const AppModal: React.FC<AppModalProps> = ({ 
  show, 
  title, 
  size, 
  onClose, 
  onSave, 
  children, 
  saveButtonText = "Kaydet", 
  isDanger = false,
  saveButton = true 
}) => {
  if (!show) return null;

  return (
    <>
      <div className="modal fade show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.4)', backdropFilter: 'blur(4px)', zIndex: 1050 }}>
        <div className={`modal-dialog modal-dialog-centered ${size ? `modal-${size}` : ''}`}>
          {/* 🚀 DÜZELTME: AppSelect gibi açılır menülerin kesilmemesi için overflow: visible */}
          <div className="modal-content border-0 shadow-lg" style={{ overflow: 'visible' }}>
            
            <div className="modal-header border-0 pb-0">
              <h5 className="modal-title fw-bold">{title}</h5>
              <button type="button" className="btn-close" onClick={onClose}></button>
            </div>

            <div className="modal-body py-4" style={{ overflow: 'visible' }}>
              {children}
            </div>

            <div className="modal-footer border-0 pt-0">
              <button type="button" className="btn btn-light btn-sm px-3" onClick={onClose}>
                {saveButton ? "Vazgeç" : "Kapat"}
              </button>
              
              {/* 🚀 Eğer saveButton true ise VE onSave fonksiyonu tanımlıysa göster */}
              {saveButton && onSave && (
                <button 
                  type="button" 
                  className={`btn btn-sm px-4 ${isDanger ? 'btn-danger' : 'btn-primary'}`} 
                  onClick={onSave}
                >
                  {saveButtonText}
                </button>
              )}
            </div>
            
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" style={{ zIndex: 1040 }}></div>
    </>
  );
};

export default AppModal;