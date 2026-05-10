namespace MiniERP.Application.Interfaces
{
    public interface IAIService
    {
        Task<string> AnalyzeFinancialDataAsync(string dataSummary, CancellationToken cancellationToken);
    }
}
