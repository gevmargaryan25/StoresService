using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StoresService.EntityFramework.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        public Supplier? Supplier { get; set; }

        public ICollection<StoreProduct> StoreProducts { get; set; }

        public Company Company;
    }
}
