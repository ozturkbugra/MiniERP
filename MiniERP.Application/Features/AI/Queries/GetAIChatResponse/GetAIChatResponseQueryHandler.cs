using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Application.Features.AI.Queries.GetAIChatResponse
{
    public sealed class GetAIChatResponseQueryHandler : IRequestHandler<GetAIChatResponseQuery, Result<string>>
    {
        private readonly IAIService _aiService;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IRepository<CashTransaction> _cashRepository;

        public GetAIChatResponseQueryHandler(
            IAIService aiService,
            IRepository<StockTransaction> stockRepository,
            IRepository<BankTransaction> bankRepository,
            IRepository<CashTransaction> cashRepository)
        {
            _aiService = aiService;
            _stockRepository = stockRepository;
            _bankRepository = bankRepository;
            _cashRepository = cashRepository;
        }

        public async Task<Result<string>> Handle(GetAIChatResponseQuery request, CancellationToken cancellationToken)
        {
            // 1. Veritabanından Canlı Verileri Topla (Context Enjeksiyonu)
            var bankBalance = _bankRepository.GetAll().Where(x => !x.IsDeleted).Sum(x => x.Debit - x.Credit);
            var cashBalance = _cashRepository.GetAll().Where(x => !x.IsDeleted).Sum(x => x.Debit - x.Credit);

            // Kritik stokları hesapla (Bakiye 5 ve altı olanlar)
            var criticalStocks = _stockRepository.GetAll()
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    Balance = g.Sum(x => x.Type == StockTransactionType.Out ? -x.Quantity : x.Quantity)
                })
                .Where(x => x.Balance <= 5)
                .ToList();

            // 2. AI için "Kopya Kağıdı" (Hidden System Prompt) hazırla
            var contextInfo = $"[SİSTEM]: Banka: {bankBalance:N2}₺, Kasa: {cashBalance:N2}₺. " +
                              $"Kritik Stoklar (ID/Bakiye): {string.Join(", ", criticalStocks.Select(s => $"{s.ProductId}:{s.Balance}"))}. " +
                              $"Lütfen cevaplarını bu verilere dayandır.";

            // 3. Mesaj geçmişini hazırla ve başına context'i ekle
            var chatHistory = request.History.Select(x => new ChatMessage(x.Role, x.Content)).ToList();
            chatHistory.Insert(0, new ChatMessage("user", contextInfo));

            // 4. Servis üzerinden Gemini'ye sor
            var aiResponse = await _aiService.ChatWithAIAsync(chatHistory, cancellationToken);

            return Result<string>.Success(aiResponse,"Veriler başarıyla listelendi");
        }
    }
}
