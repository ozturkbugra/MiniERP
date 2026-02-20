using Microsoft.AspNetCore.Identity;
using MiniERP.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniERP.Domain.Entities
{
    public sealed class AppRole : IdentityRole<Guid>, IEntity
    {
        // rol açıklaması
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
