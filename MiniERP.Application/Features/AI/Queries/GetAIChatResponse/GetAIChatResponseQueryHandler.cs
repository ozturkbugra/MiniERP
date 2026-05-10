using MediatR;
using MiniERP.Application.Interfaces;
using MiniERP.Domain.Common;
using MiniERP.Domain.Entities;
using MiniERP.Domain.Enums;
using System.Text;
using CustomerEntity = MiniERP.Domain.Entities.Customer;

namespace MiniERP.Application.Features.AI.Queries.GetAIChatResponse
{
    public sealed class GetAIChatResponseQueryHandler : IRequestHandler<GetAIChatResponseQuery, Result<string>>
    {
        private readonly IAIService _aiService;
        private readonly IRepository<StockTransaction> _stockRepository;
        private readonly IRepository<BankTransaction> _bankRepository;
        private readonly IRepository<CashTransaction> _cashRepository;
        private readonly IRepository<CustomerTransaction> _customerTransactionRepository;
        private readonly IRepository<CustomerEntity> _customerRepository;

        public GetAIChatResponseQueryHandler(
            IAIService aiService,
            IRepository<StockTransaction> stockRepository,
            IRepository<BankTransaction> bankRepository,
            IRepository<CashTransaction> cashRepository,
            IRepository<CustomerTransaction> customerTransactionRepository,
            IRepository<CustomerEntity> customerRepository)
        {
            _aiService = aiService;
            _stockRepository = stockRepository;
            _bankRepository = bankRepository;
            _cashRepository = cashRepository;
            _customerTransactionRepository = customerTransactionRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Result<string>> Handle(GetAIChatResponseQuery request, CancellationToken cancellationToken)
        {
            // --- 1. FİNANSAL DURUM (KASA & BANKA) ---
            var bankBalance = _bankRepository.GetAll().Where(x => !x.IsDeleted).Sum(x => x.Debit - x.Credit);
            var cashBalance = _cashRepository.GetAll().Where(x => !x.IsDeleted).Sum(x => x.Debit - x.Credit);

            // --- 2. CARİ DURUMU (ÖZGÜRLEŞTİRİLDİ) ---
            var customerTxs = _customerTransactionRepository.GetAll().Where(x => !x.IsDeleted).ToList();
            var customerBalances = customerTxs
                .GroupBy(x => x.CustomerId)
                .Select(g => new {
                    CustomerId = g.Key,
                    Balance = g.Sum(x => x.Debit - x.Credit)
                }).ToList();

            decimal totalReceivables = customerBalances.Where(x => x.Balance > 0).Sum(x => x.Balance);
            decimal totalPayables = customerBalances.Where(x => x.Balance < 0).Sum(x => x.Balance);

            var activeCustomers = _customerRepository.GetAll().Where(x => !x.IsDeleted).ToList();

            // LİMİT YOK: Bakiyesi sıfır olmayan herkesi gönderiyoruz
            var allActiveCaris = customerBalances
                .Where(x => x.Balance != 0)
                .Select(cb => new {
                    Name = activeCustomers.FirstOrDefault(c => c.Id == cb.CustomerId)?.Name ?? "Bilinmeyen Cari",
                    Balance = cb.Balance
                })
                .OrderByDescending(x => x.Balance)
                .ToList();

            // --- 3. STOK DURUMU (ÖZGÜRLEŞTİRİLDİ) ---
            var stockMoves = await _stockRepository.GetAllAsync(x => !x.IsDeleted, cancellationToken, x => x.Product);

            // LİMİT YOK: Eldeki tüm mevcut stokları gönderiyoruz (Sıfır olanlar kalabalık yapmasın diye hariç)
            var allActiveStocks = stockMoves
                .GroupBy(x => new { x.ProductId, ProductName = x.Product?.Name ?? "Bilinmeyen Ürün" })
                .Select(g => new {
                    ProductName = g.Key.ProductName,
                    Balance = g.Sum(x => ((int)x.Type == (int)StockTransactionType.In || (int)x.Type == 3) ? x.Quantity : -x.Quantity)
                })
                .Where(x => x.Balance != 0)
                .OrderBy(x => x.ProductName)
                .ToList();

            // --- 4. AI İÇİN KOPYA KAĞIDI (SYSTEM PROMPT) OLUŞTURMA ---
            var sb = new StringBuilder();
            sb.AppendLine("[GİZLİ SİSTEM BİLGİSİ - MİNİ ERP GÜNCEL DURUM ÖZETİ]");
            sb.AppendLine($"💰 Kasa: {cashBalance:N2} TL | 🏦 Banka: {bankBalance:N2} TL");
            sb.AppendLine($"📈 Piyasadaki Toplam Alacak (Tahsil Edilecek): {totalReceivables:N2} TL");
            sb.AppendLine($"📉 Piyasaya Toplam Borç (Ödenecek): {Math.Abs(totalPayables):N2} TL\n");

            if (allActiveCaris.Any())
            {
                sb.AppendLine("📊 TÜM CARİ BAKİYELER (Pozitif: Alacağımız var, Negatif: Borcumuz var):");
                foreach (var c in allActiveCaris) sb.AppendLine($"- {c.Name}: {c.Balance:N2} TL");
            }

            if (allActiveStocks.Any())
            {
                sb.AppendLine("\n📦 GÜNCEL STOK DURUMU (Eldeki Tüm Ürünler):");
                foreach (var s in allActiveStocks) sb.AppendLine($"- {s.ProductName}: {s.Balance} adet");
            }

            sb.AppendLine("\nLütfen cevaplarını sadece bu gerçek verilere dayandır. Kullanıcı sana verileri analiz etmeni gerektiren sorular soracak (Örn: 'En borçlu 5 kişi?', 'Hangi stoklar 5'in altına düştü?'). Sen bu listeleri inceleyip soruyu cevapla. Türkçe, profesyonel ve kısa konuş.");

            // --- 5. MESAJ GEÇMİŞİNİ HAZIRLA VE AI'YA FIRLAT ---
            var chatHistory = request.History.Select(x => new ChatMessage(x.Role, x.Content)).ToList();
            chatHistory.Insert(0, new ChatMessage("user", sb.ToString()));

            var aiResponse = await _aiService.ChatWithAIAsync(chatHistory, cancellationToken);

            return Result<string>.Success(aiResponse, "AI Chat yanıtı başarıyla oluşturuldu.");
        }
    }
}