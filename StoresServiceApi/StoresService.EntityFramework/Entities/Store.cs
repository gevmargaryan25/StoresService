using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StoresService.EntityFramework.Entities
{
    public class Store
    {
        [Key]
        public int StoreId { get; set; }

        [Required]
        public string StoreName { get; set; }

        [Required]
        public string Address { get; set; }

        public ICollection<StoreProduct> StoreProducts { get; set; }

        public ICollection<Supplier> Suppliers { get; set; }
    }
}
