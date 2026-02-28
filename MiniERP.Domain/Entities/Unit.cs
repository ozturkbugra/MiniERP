using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class Unit : BaseEntity
    {
        public string Code { get; set; } // Örn: AD, KG, KOLI
        public string Name { get; set; } // Örn: Adet, Kilogram, Koli

        public ICollection<Product> Products { get; set; }

        public Unit()
        {
            Products = new HashSet<Product>(); // Null Reference yememek için best practice!
        }
    }
}