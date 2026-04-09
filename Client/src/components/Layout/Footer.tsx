import React from 'react';

const Footer: React.FC = () => {
  return (
    <footer className="footer">
      <div className="footer-content">
        <div className="footer-links">
          <a href="#">Docs</a>
          <a href="#">Privacy</a>
          <a href="#">Security</a>
          <a href="#">Support</a>
        </div>

        <div className="footer-credits">
          <div className="footer-copyright">
            &copy; {new Date().getFullYear()} <a href="https://bugraozturk.com.tr" target="_blank" rel="noreferrer">MiniERP</a>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;