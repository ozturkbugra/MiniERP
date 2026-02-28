using MiniERP.Domain.Common;

namespace MiniERP.Domain.Entities
{
    public sealed class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }

        public Category()
        {
            Products = new HashSet<Product>(); // Null Reference yememek için best practice!
        }
    }
}