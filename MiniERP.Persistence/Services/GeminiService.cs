using Microsoft.Extensions.Configuration;
using MiniERP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Persistence.Services
{
    public class GeminiService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiConfig:ApiKey"] ?? "";
            _model = configuration["GeminiConfig:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<string> AnalyzeFinancialDataAsync(string dataSummary, CancellationToken cancellationToken)
        {
            // Google Gemini API URL'i
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // AI'ya ne yapacağını söylediğimiz "Karakter" tanımlaması (Prompt)
            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = $"Sen bir kıdemli finans analistisin. Aşağıdaki verileri inceleyip nakit akışı, riskler ve öneriler içeren kısa bir rapor yaz:\n\n{dataSummary}" }
                        }
                    }
                }
            };

            // İsteği gönderiyoruz
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode) return "AI Analizi şu an yapılamıyor.";

            var result = await response.Content.ReadFromJsonAsync<dynamic>();

            // Gemini'den gelen cevabın içindeki metni çekiyoruz
            return result?.candidates[0]?.content?.parts[0]?.text ?? "Cevap alınamadı.";
        }
    }
}
