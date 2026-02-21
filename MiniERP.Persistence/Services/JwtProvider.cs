using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniERP.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniERP.Persistence.Services;

public sealed class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateTokenAsync(string userId, string email, IList<string> roles)
    {
        // 1. Tokenın içine koyacağımız bilgiler
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Kullanıcının rollerini de Token'ın içine ekliyoruz
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 2. appsettings.json dosyasından ayarları çekiyoruz
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expireMinutes = Convert.ToInt32(_configuration["Jwt:ExpireMinutes"]);

        // 3. Şifreleme Anahtarını hazırlıyoruz
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 4. Tokenı oluşturuyoruz
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        // 5. Tokenı string formatına çevirip geri dönüyoruz
        var handler = new JwtSecurityTokenHandler();
        return await Task.FromResult(handler.WriteToken(token));
    }
}