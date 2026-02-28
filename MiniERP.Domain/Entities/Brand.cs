using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class Brand : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }

        public Brand()
        {
            Products = new HashSet<Product>(); // Null Reference yememek için best practice!
        }
    }
}