import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom' // Yeni ekledik
import App from './App.tsx'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* Bütün App'i BrowserRouter ile sarmalıyoruz ki navigasyon çalışsın */}
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.StrictMode>,
)