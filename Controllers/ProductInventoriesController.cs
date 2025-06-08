using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdventureWorksCommons.Models;

namespace AdventureWorksCommons.Controllers
{
    [Route("[controller]")]
    public class ProductInventoriesController : Controller
    {
        private readonly AdventureWorksContext _context;

        public ProductInventoriesController(AdventureWorksContext context)
        {
            _context = context;
        }

        // GET: ProductInventories
        [HttpGet("{page?}")]
        public async Task<IActionResult> Index(int? page = 1)
        {
            const int pageSize = 50;
            var query = _context.ProductInventories
                .Include(p => p.Location)
                .Include(p => p.Product)
                .OrderBy(p => p.ProductId);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip(((int)page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (totalItems / pageSize) + 1;
            return View(items);
        }

        [HttpGet("{productID}/Location/{locationID}/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int productID,short locationID)
        {
            var productInventory = await _context.ProductInventories
                .Include(p => p.Location)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.ProductId == productID && m.LocationId==locationID);
            return View(productInventory);
        }

        [HttpPost("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([Bind("ProductId", "LocationId", "Quantity")] ProductInventory productInventory)
        {
            await _context.Database
                .ExecuteSqlInterpolatedAsync($"EXEC mms.UpdateInventoryQuantity @ProductID = {productInventory.ProductId}, @LocationID = {productInventory.LocationId}, @NewQuantity = {productInventory.Quantity}");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Replenish")]
        public IActionResult Replenish()
        {
            return View();
        }

        [HttpPost("Replenish")]
        public async Task<IActionResult> Replenish(int quantity)
        {
            await _context.Database
                .ExecuteSqlInterpolatedAsync($"EXEC mms.ReplenishInventory @ReplenishQty = {quantity}");
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductInventories/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["LocationId"] = new SelectList(_context.Locations, "LocationId", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: ProductInventories/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,LocationId,Shelf,Bin,Quantity,Rowguid,ModifiedDate")] ProductInventory productInventory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productInventory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LocationId"] = new SelectList(_context.Locations, "LocationId", "Name", productInventory.LocationId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", productInventory.ProductId);
            return View(productInventory);
        }
    }
}
