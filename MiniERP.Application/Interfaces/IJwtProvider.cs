namespace MiniERP.Application.Interfaces;

public interface IJwtProvider
{
    Task<string> CreateTokenAsync(string userId, string email, IList<string> roles);
}