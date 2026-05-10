import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom' // Yeni ekledik
import App from './App.tsx'
import './index.css'
import { Toaster } from 'react-hot-toast';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* Bütün App'i BrowserRouter ile sarmalıyoruz ki navigasyon çalışsın */}
    <BrowserRouter>
      <Toaster position="top-right" reverseOrder={false} />
      <App />
    </BrowserRouter>
  </React.StrictMode>,
)