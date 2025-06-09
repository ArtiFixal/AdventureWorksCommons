using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdventureWorksCommons.Models;

namespace AdventureWorksCommons.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AdventureWorksContext _context;

        public EmployeesController(AdventureWorksContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 50;

            var adventureWorksContext = _context.Employees
                .Include(e => e.BusinessEntity)
                .OrderBy(e => e.BusinessEntityId);

            var pagedData = await adventureWorksContext
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalCount = await _context.Employees.CountAsync();
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (totalCount / pageSize)+1;

            return View(pagedData);
        }


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.BusinessEntity)
                .FirstOrDefaultAsync(m => m.BusinessEntityId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            ViewData["BusinessEntityId"] = new SelectList(_context.People, "BusinessEntityId", "BusinessEntityId");
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusinessEntityId,NationalIdnumber,LoginId,OrganizationLevel,JobTitle,BirthDate,MaritalStatus,Gender,HireDate,SalariedFlag,VacationHours,SickLeaveHours,CurrentFlag,Rowguid,ModifiedDate")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusinessEntityId"] = new SelectList(_context.People, "BusinessEntityId", "BusinessEntityId", employee.BusinessEntityId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["BusinessEntityId"] = new SelectList(_context.People, "BusinessEntityId", "BusinessEntityId", employee.BusinessEntityId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BusinessEntityId,NationalIdnumber,LoginId,OrganizationLevel,JobTitle,BirthDate,MaritalStatus,Gender,HireDate,SalariedFlag,VacationHours,SickLeaveHours,CurrentFlag,Rowguid,ModifiedDate")] Employee employee)
        {
            if (id != employee.BusinessEntityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.BusinessEntityId))
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
            ViewData["BusinessEntityId"] = new SelectList(_context.People, "BusinessEntityId", "BusinessEntityId", employee.BusinessEntityId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.BusinessEntity)
                .FirstOrDefaultAsync(m => m.BusinessEntityId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.BusinessEntityId == id);
        }
    }
}
