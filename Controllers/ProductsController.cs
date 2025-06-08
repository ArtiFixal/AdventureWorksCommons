using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdventureWorksCommons.Models;

namespace AdventureWorksCommons.Controllers
{
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly AdventureWorksContext _context;

        public ProductsController(AdventureWorksContext context)
        {
            _context = context;
        }

        // GET: Products/5
        [HttpGet("{page?}")]
        public async Task<IActionResult> Index(int? page = 1)
        {
            const int pageSize = 50;

            var query = _context.Products
                .Include(p => p.ProductModel)
                .Include(p => p.ProductSubcategory)
                .Include(p => p.SizeUnitMeasureCodeNavigation)
                .Include(p => p.WeightUnitMeasureCodeNavigation);

            int totalItems = await query.CountAsync();
            int totalPages = (totalItems / pageSize) + 1;

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((int)((page - 1) * pageSize))
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(products);
        }


        // GET: Products/Details/5
        [HttpGet("{id?}/Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductModel)
                .Include(p => p.ProductSubcategory)
                .Include(p => p.SizeUnitMeasureCodeNavigation)
                .Include(p => p.WeightUnitMeasureCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        protected void BindViewData(NewProductStaging? product = null)
        {
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", product?.ProductModelId);
            ViewData["ProductSubcategoryId"] = new SelectList(_context.ProductSubcategories, "ProductSubcategoryId", "Name", product?.ProductSubcategoryId);
            ViewData["SizeUnitMeasureCode"] = new SelectList(_context.UnitMeasures, "UnitMeasureCode", "Name", product?.SizeUnitMeasureCode);
            ViewData["WeightUnitMeasureCode"] = new SelectList(_context.UnitMeasures, "UnitMeasureCode", "Name", product?.WeightUnitMeasureCode);
        }

        protected void BindViewData(Product product)
        {
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name",product?.ProductModelId);
            ViewData["ProductSubcategoryId"] = new SelectList(_context.ProductSubcategories, "ProductSubcategoryId", "Name", product?.ProductSubcategoryId);
            ViewData["SizeUnitMeasureCode"] = new SelectList(_context.UnitMeasures, "UnitMeasureCode", "Name", product?.SizeUnitMeasureCode);
            ViewData["WeightUnitMeasureCode"] = new SelectList(_context.UnitMeasures, "UnitMeasureCode", "Name", product?.WeightUnitMeasureCode);
        }

        // GET: Products/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            BindViewData();
            return View();
        }

        // POST: Products/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,ProductNumber,MakeFlag,FinishedGoodsFlag,Color,SafetyStockLevel,ReorderPoint,StandardCost,ListPrice,Size,SizeUnitMeasureCode,WeightUnitMeasureCode,Weight,DaysToManufacture,ProductLine,Class,Style,ProductSubcategoryId,ProductModelId,SellStartDate,SellEndDate,DiscontinuedDate,Rowguid,ModifiedDate")] NewProductStaging product)
        {
            if (ModelState.IsValid)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO prs.NewProductStaging
                (Name, ProductNumber, MakeFlag, FinishedGoodsFlag, Color, SafetyStockLevel, ReorderPoint,
                 StandardCost, ListPrice, Size, SizeUnitMeasureCode, WeightUnitMeasureCode, Weight,
                 DaysToManufacture, ProductLine, Class, Style, ProductSubcategoryID, ProductModelID,
                 SellStartDate, SellEndDate, DiscontinuedDate)
                VALUES
                (
                    {product.Name}, {product.ProductNumber}, {product.MakeFlag}, {product.FinishedGoodsFlag},
                    {product.Color}, {product.SafetyStockLevel}, {product.ReorderPoint}, {product.StandardCost},
                    {product.ListPrice}, {product.Size}, {product.SizeUnitMeasureCode}, {product.WeightUnitMeasureCode},
                    {product.Weight}, {product.DaysToManufacture}, {product.ProductLine}, {product.Class},
                    {product.Style}, {product.ProductSubcategoryId}, {product.ProductModelId},
                    {product.SellStartDate}, {product.SellEndDate}, {product.DiscontinuedDate}
                )");
                return RedirectToAction(nameof(Index));
            }
            BindViewData(product);
            return View(product);
        }

        // GET: Products/Edit/5
        [HttpGet("{id?}/Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            BindViewData(product);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost("{id?}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,ProductNumber,MakeFlag,FinishedGoodsFlag,Color,SafetyStockLevel,ReorderPoint,StandardCost,ListPrice,Size,SizeUnitMeasureCode,WeightUnitMeasureCode,Weight,DaysToManufacture,ProductLine,Class,Style,ProductSubcategoryId,ProductModelId,SellStartDate,SellEndDate,DiscontinuedDate,Rowguid,ModifiedDate")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            BindViewData(product);
            return View(product);
        }

        // GET: Products/Delete/5
        [HttpGet("{id?}/Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductModel)
                .Include(p => p.ProductSubcategory)
                .Include(p => p.SizeUnitMeasureCodeNavigation)
                .Include(p => p.WeightUnitMeasureCodeNavigation)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost("{id?}/Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
