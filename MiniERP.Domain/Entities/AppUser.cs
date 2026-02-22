using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class AppUser : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        // Identity olmadığı için giriş bilgilerini biz tanımlıyoruz
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // JWT ve oturum yönetimi için
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
