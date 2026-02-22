using Microsoft.AspNetCore.Identity;

namespace MiniERP.Persistence.IdentityModels;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;
}