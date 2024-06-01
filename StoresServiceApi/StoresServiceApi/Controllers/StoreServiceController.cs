using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoresService.EntityFramework.Context;
using StoresService.EntityFramework.Entities;
using StoresServiceApi.DTOs;

namespace StoresServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreServiceController : ControllerBase
    {
        private readonly StoresServiceDbContext _context;

        public StoreServiceController(StoresServiceDbContext context)
        {
            _context = context;
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
    }
}
