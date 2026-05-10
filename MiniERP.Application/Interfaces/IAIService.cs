namespace MiniERP.Application.Interfaces
{
    public record ChatMessage(string Role, string Content);
    public interface IAIService
    {
        Task<string> ChatWithAIAsync(List<ChatMessage> history, CancellationToken cancellationToken);
    }
}
