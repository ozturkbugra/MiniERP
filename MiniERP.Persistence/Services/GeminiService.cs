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

        public async Task<string> ChatWithAIAsync(List<ChatMessage> history, CancellationToken cancellationToken)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Gemini 'assistant' yerine 'model' kelimesini kullanır, onu mapliyoruz
            var contents = history.Select(m => new {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            }).ToArray();

            var requestBody = new { contents };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode) return "Asistan şu an cevap veremiyor.";

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            return result?.candidates[0]?.content?.parts[0]?.text ?? "Anlayamadım şef, tekrar eder misin?";
        }
    }
}
