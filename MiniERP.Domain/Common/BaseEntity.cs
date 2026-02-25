namespace MiniERP.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Soft Delete 
        public bool IsDeleted { get; set; } = false;

        public string? CreatedBy { get; set; } // Kaydı oluşturan kullanıcının ID'si
        public string? UpdatedBy { get; set; } // Son güncelleyen kullanıcının ID'si
    }
}
