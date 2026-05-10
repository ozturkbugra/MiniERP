using Microsoft.Extensions.Configuration;
using MiniERP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
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
            // appsettings.json içinden anahtarı ve modeli çekiyoruz
            _apiKey = configuration["GeminiConfig:ApiKey"] ?? "";
            _model = configuration["GeminiConfig:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<string> ChatWithAIAsync(List<ChatMessage> history, CancellationToken cancellationToken)
        {
            // 1. URL Hazırlığı (Model adında boşluk olmamasına dikkat ediyoruz)
            var modelName = _model.Trim();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            // 🚀 2. GÜVENLİK ENGELİ ÇÖZÜMÜ: API Key'i URL yerine Header'dan gönderiyoruz
            request.Headers.Add("X-goog-api-key", _apiKey);

            // 3. Gemini'nin beklediği JSON şablonunu oluşturuyoruz
            var requestBody = new
            {
                contents = history.Select(m => new
                {
                    // Backend'deki "assistant" rolünü Google'ın anladığı "model" rolüne çeviriyoruz
                    role = m.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.Content } }
                }).ToArray()
            };

            request.Content = JsonContent.Create(requestBody);

            // 4. İsteği Fırlat
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return $"Aga bir hata aldık: {response.StatusCode} - {error}";
            }

            // 🚀 5. JSON PARSE ÇÖZÜMÜ: dynamic yerine System.Text.Json.Nodes kullanıyoruz
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonNode = JsonNode.Parse(jsonString);

            try
            {
                // Köşeli parantezlerle JSON içinde güvenle sörf yapıp sadece cevabı çekiyoruz
                return jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "Cevap boş döndü şef.";
            }
            catch
            {
                return "AI cevabı işlerken bir hata oluştu.";
            }
        }
    }
}
