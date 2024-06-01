using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StoresService.EntityFramework.Entities
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public ICollection<Supplier> Suppliers { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
