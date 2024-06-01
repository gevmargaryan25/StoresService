using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StoresService.EntityFramework.Entities
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        public string SupplierName { get; set; }

        public ICollection<Store> Stores { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<Company> Companies { get; set; }
    }
}
