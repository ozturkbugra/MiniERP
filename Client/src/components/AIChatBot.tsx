import React, { useState, useEffect } from 'react';
import api from '../api/axiosInstance'; 

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

const AIChatBot = () => {
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState<string>('');
  const [isLoading, setIsLoading] = useState<boolean>(false);
  
  // 🚀 YENİ: Dark Mode state'i
  const [isDarkMode, setIsDarkMode] = useState<boolean>(false);

  // 🚀 YENİ: HTML etiketindeki data-theme değişimlerini canlı dinliyoruz
  useEffect(() => {
    const htmlElement = document.documentElement; // <html> etiketini seçer

    // İlk açılışta temayı kontrol et
    const checkTheme = () => {
      setIsDarkMode(htmlElement.getAttribute('data-theme') === 'dark');
    };
    checkTheme();

    // Sen sağ üstten temayı değiştirdiğinde anında yakalamak için gözlemci (Observer) kuruyoruz
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.attributeName === 'data-theme') {
          checkTheme();
        }
      });
    });

    observer.observe(htmlElement, { attributes: true });

    return () => observer.disconnect(); // Bileşen ekrandan kalkarsa gözlemciyi kapat
  }, []);

  const handleSendMessage = async () => {
    if (!input.trim()) return;

    const newHistory: ChatMessage[] = [...messages, { role: 'user', content: input }];
    setMessages(newHistory);
    setInput('');
    setIsLoading(true);

    try {
      const response = await api.post('/AI/chat', { history: newHistory });

      setMessages([
        ...newHistory,
        { role: 'assistant', content: response.data.data }
      ]);
    } catch (error) {
      console.error("AI Bağlantı Hatası:", error);
      setMessages([
        ...newHistory,
        { role: 'assistant', content: 'Aga sunucuya bağlanırken bir sorun oldu.' }
      ]);
    } finally {
      setIsLoading(false);
    }
  };

  // 🎨 TEMA RENKLERİ (isDarkMode state'ine göre dinamik değişecek)
  const theme = {
    bg: isDarkMode ? '#1e1e2d' : 'white',
    text: isDarkMode ? '#a1a5b7' : 'black',
    border: isDarkMode ? '#323248' : '#ccc',
    inputBg: isDarkMode ? '#151521' : 'white',
    inputText: isDarkMode ? '#ffffff' : 'black',
    userMsgBg: isDarkMode ? '#323248' : '#e9ecef',
    userMsgText: isDarkMode ? '#ffffff' : 'black',
    headerBg: isDarkMode ? '#009ef7' : '#0d6efd', // Karanlıkta mavi biraz daha parlak olsun
  };

  return (
    <div style={{ position: 'fixed', bottom: '20px', right: '20px', zIndex: 9999 }}>
      
      {isOpen && (
        <div style={{ 
            width: '350px', height: '450px', 
            backgroundColor: theme.bg, // Dinamik Arkaplan
            border: `1px solid ${theme.border}`, // Dinamik Çerçeve
            borderRadius: '10px', display: 'flex', 
            flexDirection: 'column', marginBottom: '10px', 
            boxShadow: isDarkMode ? '0 4px 15px rgba(0,0,0,0.5)' : '0 4px 8px rgba(0,0,0,0.1)',
            transition: 'all 0.3s ease' // Renk geçişleri yumuşak olsun
        }}>
          
          <div style={{ backgroundColor: theme.headerBg, color: 'white', padding: '10px', borderTopLeftRadius: '10px', borderTopRightRadius: '10px', display: 'flex', justifyContent: 'space-between', transition: 'all 0.3s ease' }}>
            <strong>Mini ERP Asistanı</strong>
            <button onClick={() => setIsOpen(false)} style={{ background: 'none', border: 'none', color: 'white', cursor: 'pointer', fontSize: '16px' }}>✖</button>
          </div>

          <div style={{ flex: 1, padding: '10px', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '10px' }}>
            {messages.length === 0 && <span style={{ color: theme.text, fontSize: '14px', textAlign: 'center', marginTop: 'auto', marginBottom: 'auto' }}>Sana nasıl yardımcı olabilirim?</span>}
            
            {messages.map((msg, index) => (
              <div key={index} style={{ 
                  alignSelf: msg.role === 'user' ? 'flex-end' : 'flex-start', 
                  backgroundColor: msg.role === 'user' ? theme.userMsgBg : theme.headerBg, 
                  color: msg.role === 'user' ? theme.userMsgText : 'white',
                  padding: '10px 14px', borderRadius: '15px', maxWidth: '85%', fontSize: '14px',
                  lineHeight: '1.4',
                  boxShadow: '0 1px 2px rgba(0,0,0,0.1)'
              }}>
                {msg.content}
              </div>
            ))}
            {isLoading && <span style={{ color: theme.text, fontSize: '12px', fontStyle: 'italic' }}>Asistan analiz ediyor...</span>}
          </div>

          <div style={{ padding: '10px', borderTop: `1px solid ${theme.border}`, display: 'flex', gap: '8px', backgroundColor: theme.bg, borderBottomLeftRadius: '10px', borderBottomRightRadius: '10px' }}>
            <input 
              type="text" 
              value={input} 
              onChange={(e) => setInput(e.target.value)} 
              onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
              placeholder="Asistana sor..." 
              style={{ 
                flex: 1, padding: '10px', borderRadius: '6px', 
                border: `1px solid ${theme.border}`, 
                backgroundColor: theme.inputBg, // Dinamik input arkaplanı
                color: theme.inputText, // Dinamik input yazısı
                outline: 'none'
              }}
            />
            <button onClick={handleSendMessage} style={{ padding: '8px 16px', backgroundColor: theme.headerBg, color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: '500' }}>
              Gönder
            </button>
          </div>
        </div>
      )}

      {!isOpen && (
        <button 
          onClick={() => setIsOpen(true)}
          style={{
            width: '60px', height: '60px', borderRadius: '50%', backgroundColor: theme.headerBg,
            color: 'white', border: 'none', cursor: 'pointer', fontSize: '26px',
            boxShadow: isDarkMode ? '0 4px 12px rgba(0,0,0,0.6)' : '0 4px 8px rgba(0,0,0,0.2)',
            transition: 'all 0.3s ease',
            display: 'flex', justifyContent: 'center', alignItems: 'center'
          }}
        >
          🤖
        </button>
      )}
    </div>
  );
};

export default AIChatBot;