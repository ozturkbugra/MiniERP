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
}

const AppModal: React.FC<AppModalProps> = ({ 
  show, title, size, onClose, onSave, children, saveButtonText = "Kaydet", isDanger = false 
}) => {
  if (!show) return null;

  return (
    <>
      <div className="modal fade show d-block" tabIndex={-1} style={{ backgroundColor: 'rgba(0,0,0,0.4)', backdropFilter: 'blur(4px)' }}>
        <div className={`modal-dialog modal-dialog-centered ${size ? `modal-${size}` : ''}`}>
          {/* 🚀 DÜZELTME 1: modal-content taşmalara izin veriyor */}
          <div className="modal-content border-0 shadow-lg" style={{ overflow: 'visible' }}>
            <div className="modal-header border-0 pb-0">
              <h5 className="modal-title fw-bold">{title}</h5>
              <button type="button" className="btn-close" onClick={onClose}></button>
            </div>
            {/* 🚀 DÜZELTME 2: modal-body taşmalara izin veriyor */}
            <div className="modal-body py-4" style={{ overflow: 'visible' }}>
              {children}
            </div>
            <div className="modal-footer border-0 pt-0">
              <button type="button" className="btn btn-light btn-sm px-3" onClick={onClose}>Vazgeç</button>
              {onSave && (
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
      <div className="modal-backdrop fade show"></div>
    </>
  );
};

export default AppModal;