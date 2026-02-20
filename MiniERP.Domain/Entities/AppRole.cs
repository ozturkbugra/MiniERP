using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Domain.Entities
{
    public sealed class AppRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: "Admin", "User", "Manager"
        public string? Description { get; set; }
    }
}
