import React from 'react';
import MainLayout from './components/Layout/MainLayout';

function App() {
  return (
    <MainLayout>
      {/* Bu araya yazdığın her şey, MainLayout'taki {children} kısmına gidecek */}
      <div style={{ padding: '20px' }}>
        <h1>MiniERP Dashboard'a Hoş Geldin!</h1>
        <p>Burası sayfanın ana içerik alanı. Layout çalışıyor.</p>
      </div>
    </MainLayout>
  );
}

export default App;