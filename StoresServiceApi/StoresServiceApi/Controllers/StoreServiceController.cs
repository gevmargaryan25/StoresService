using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoresService.EntityFramework.Context;
using StoresService.EntityFramework.Entities;
using StoresServiceApi.DTOs;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Text;
using StoresServiceApi.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace StoresServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreServiceController : Controller
    {
        private readonly StoresServiceDbContext _context;
        private readonly IPdfCreator _pdfCreator;

        public StoreServiceController(StoresServiceDbContext context, IPdfCreator pdfCreator)
        {
            _context = context;
            _pdfCreator = pdfCreator;
        }

        [HttpPost("{storeId}/products")]
        public async Task<ActionResult> AddProductsToStore(int storeId, [FromBody] List<ProductDto> productInputs)
        {
            var store = await _context.Stores
            .Include(s => s.StoreProducts)
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            foreach (var productInput in productInputs)
            {
                var product = await _context.Products.FindAsync(productInput.ProductId);

                if (product == null)
                {
                    product = new Product
                    {
                        ProductName = productInput.ProductName
                    };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                var storeProduct = store.StoreProducts.FirstOrDefault(sp => sp.ProductId == productInput.ProductId);

                if (storeProduct != null)
                {
                    storeProduct.Quantity = productInput.Quantity;
                }
                else
                {
                    store.StoreProducts.Add(new StoreProduct
                    {
                        StoreId = store.StoreId,
                        ProductId = product.ProductId,
                        Quantity = productInput.Quantity
                    });
                }
            }

            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost]
        public async Task<ActionResult> AddStore([FromBody] StoreInputDto storeInput)
        {
            if (string.IsNullOrWhiteSpace(storeInput.StoreName))
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var store = new Store
            {
                StoreName = storeInput.StoreName
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("{storeId}/products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetStoreProducts(int storeId)
        {
            var store = await _context.Stores
                .Include(s => s.StoreProducts)
                .ThenInclude(sp => sp.Product)
                .FirstOrDefaultAsync(s => s.StoreId == storeId);

            if (store == null)
            {
                return NotFound();
            }

            var products = store.StoreProducts.Select(sp => new ProductDto
            {
                ProductId = sp.ProductId,
                ProductName = sp.Product.ProductName,
                Quantity = sp.Quantity
            }).ToList();

            return StatusCode(StatusCodes.Status200OK, products);
        }

        [HttpGet(nameof(GetProductMetaData))]
        public async Task<IActionResult> GetProductMetaData(int productId)
        {
            var prod = await _context.Products
            .Include(p => p.Company)
            .ThenInclude(p => p.Suppliers)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

            var dto = new ProductOwnerDto()
            {
                CompanyName = prod.Company.CompanyName,
                SupplierName = prod.Supplier.SupplierName
            };
            return StatusCode(StatusCodes.Status200OK, dto);
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GeneratePdf()
        {
            // Fetch data from the database
            var companies = await _context.Companies
                .Include(c => c.Suppliers)
                .Include(c => c.Products)
                .ToListAsync();

            var stores = await _context.Stores
                .Include(s => s.StoreProducts)
                .ThenInclude(sp => sp.Product)
                .ToListAsync();

            var contentBuilder = new StringBuilder();
            contentBuilder.Append("Ընկերություններ\n");
            foreach (var company in companies)
            {
                contentBuilder.Append($"{company.CompanyName}\n");

                if (company.Suppliers.Any())
                {
                    contentBuilder.Append("Մատակարար\n");
                    foreach (var supplier in company.Suppliers)
                    {
                        contentBuilder.Append($"{supplier.SupplierName}\t{supplier.SupplierId}\n");
                    }
                    contentBuilder.Append("\n");
                }
                else
                {
                    contentBuilder.Append("Մատակարար չի գտնվել։\n\n");
                }
            }
            contentBuilder.Append("Խանութներ\n");
            foreach (var store in stores)
            {
                contentBuilder.Append($"{store.StoreName}\n");

                if (store.StoreProducts.Any())
                {
                    contentBuilder.Append("Ապրանքի անուն\tՔանակ\tՀամար\n");
                    foreach (var prod in store.StoreProducts)
                    {
                        contentBuilder.Append($"{prod.Product.ProductName}\t{prod.Quantity}\t{prod.ProductId}\n");
                    }
                    contentBuilder.Append("\n");
                }
                else
                {
                    contentBuilder.Append("Ապրանք չի գտնվել։\n");
                }
            }
            var pdfContent = contentBuilder.ToString();
            var pdf = await _pdfCreator.Create(pdfContent);

            return File(pdf, "application/pdf", "Stores_Data.pdf");
        }
    }
}
