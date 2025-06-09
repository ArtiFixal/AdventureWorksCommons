using AdventureWorksCommons.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdventureWorksCommons.Controllers
{
    public class EmployeeBenefitsController : Controller
    {
        private readonly AdventureWorksContext _context;

        public EmployeeBenefitsController(AdventureWorksContext context)
        {
            _context = context;
        }

        // GET: /EmployeeBenefits/
        public async Task<IActionResult> Index(int page = 1)
        {
            const int PageSize = 50;

            var query = _context.Set<BenefitProduct>()
                .FromSqlRaw("SELECT * FROM ebs.GetUnpopularProducts()")
                .AsNoTracking();

            var totalItems = await query.CountAsync();
            var totalPages = (totalItems/PageSize) + 1;

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }

        private async Task MapEmployeeDataAsync()
        {
            ViewData["Employees"] = await _context.Employees
                .Select(e => new SelectListItem
                {
                    Value = e.BusinessEntityId.ToString(),
                    Text = $"{e.BusinessEntity.FirstName} {e.BusinessEntity.LastName} ID: {e.BusinessEntity.BusinessEntityId}"
                }).ToListAsync();
        }

        private async Task MapViewDataAsync()
        {
            await MapEmployeeDataAsync();
            ViewData["Products"] = await _context.Set<BenefitProduct>()
                .FromSqlRaw("SELECT * FROM ebs.GetUnpopularProducts()")
                .Select(p => new SelectListItem
                {
                    Value = p.ProductID.ToString(),
                    Text = p.Name
                }).ToListAsync();
        }


        // GET: /EmployeeBenefits/Assign
        public async Task<IActionResult> Assign()
        {
            await MapViewDataAsync();
            return View();
        }

        // POST: /EmployeeBenefits/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignBenefitModel model)
        {
            if (!ModelState.IsValid)
            {
                await MapViewDataAsync();
                return View(model);
            }

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC ebs.AssignEmployeeBenefit @p0, @p1, @p2",
                model.EmployeeID, model.ProductID, model.DiscountPercent
            );

            return RedirectToAction("Index");
        }

        // GET: /EmployeeBenefits/Redeem
        public async Task<IActionResult> Redeem(int employeeID)
        {
            await MapEmployeeDataAsync();
            ViewData["Benefits"] = await _context.EmployeeBenefits
                .Where(b => b.EmployeeID == employeeID && !b.Redeemed)
                .Select(b => new SelectListItem
                {
                    Value = b.BenefitID.ToString(),
                    Text = $"Benefit #{b.BenefitID} – Product {b.Product.Name}, {b.DiscountPercent}%"
                }).ToListAsync();
            ViewData["SelectedEmployeeID"] = employeeID;
            return View();
        }

        // POST: /EmployeeBenefits/Redeem
        [HttpPost]
        public async Task<IActionResult> Redeem(RedeemBenefitModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            try
            {
                var sql = "EXEC ebs.RedeemBenefit @p0, @p1";
                await _context.Database.ExecuteSqlRawAsync(sql, model.BenefitID, model.EmployeeID);

                TempData["Success"] = "Benefit redeemed successfully.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = ex.InnerException?.Message ?? ex.Message;
                return View(model);
            }
        }
    }
}
