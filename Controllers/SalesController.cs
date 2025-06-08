using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdventureWorksCommons.Models;

namespace AdventureWorksCommons.Controllers
{
    [Route("[controller]")]
    public class SalesController : Controller
    {
        private readonly AdventureWorksContext _context;

        public SalesController(AdventureWorksContext context)
        {
            _context = context;
        }

        // GET: Sales
        [HttpGet("{page?}")]
        public async Task<IActionResult> Index(int? page = 1)
        {
            int pageSize = 50;
            int pageNumber = page.GetValueOrDefault(1);
            if (pageNumber < 1)
                pageNumber = 1;

            var query = _context.SalesOrderHeaders
                .Include(s => s.BillToAddress)
                .Include(s => s.CreditCard)
                .Include(s => s.CurrencyRate)
                .Include(s => s.Customer)
                .Include(s => s.SalesPerson)
                .Include(s => s.ShipMethod)
                .Include(s => s.ShipToAddress)
                .Include(s => s.Territory)
                .OrderBy(s => s.SalesOrderId);

            var totalItems = await query.CountAsync();

            var salesOrders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = (totalItems / pageSize) + 1;

            return View(salesOrders);
        }

        [HttpGet("{saleID}/Invoice")]
        public async Task<IActionResult> Invoice(int saleID)
        {
            if (!SaleHasInvoice(saleID))
            {
                await _context.Database
                    .ExecuteSqlInterpolatedAsync($"EXEC igs.GenerateInvoice @SalesOrderID = {saleID}");
            }
            var invoice = _context.Invoices.Include(invoice=>invoice.InvoiceLines)
                .FirstOrDefault(invoice => invoice.SalesOrderId == saleID);
            return View(invoice);
        }

        // GET: Sales/5/Details
        [HttpGet("{id?}/Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderHeader = await _context.SalesOrderHeaders
                .Include(s => s.BillToAddress)
                .Include(s => s.CreditCard)
                .Include(s => s.CurrencyRate)
                .Include(s => s.Customer)
                .Include(s => s.SalesPerson)
                .Include(s => s.ShipMethod)
                .Include(s => s.ShipToAddress)
                .Include(s => s.Territory)
                .FirstOrDefaultAsync(m => m.SalesOrderId == id);
            if (salesOrderHeader == null)
            {
                return NotFound();
            }

            return View(salesOrderHeader);
        }

        // GET: Sales/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["BillToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId");
            ViewData["CreditCardId"] = new SelectList(_context.CreditCards, "CreditCardId", "CreditCardId");
            ViewData["CurrencyRateId"] = new SelectList(_context.CurrencyRates, "CurrencyRateId", "CurrencyRateId");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["SalesPersonId"] = new SelectList(_context.SalesPeople, "BusinessEntityId", "BusinessEntityId");
            ViewData["ShipMethodId"] = new SelectList(_context.ShipMethods, "ShipMethodId", "ShipMethodId");
            ViewData["ShipToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId");
            ViewData["TerritoryId"] = new SelectList(_context.SalesTerritories, "TerritoryId", "TerritoryId");
            return View();
        }

        // POST: Sales/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SalesOrderId,RevisionNumber,OrderDate,DueDate,ShipDate,Status,OnlineOrderFlag,SalesOrderNumber,PurchaseOrderNumber,AccountNumber,CustomerId,SalesPersonId,TerritoryId,BillToAddressId,ShipToAddressId,ShipMethodId,CreditCardId,CreditCardApprovalCode,CurrencyRateId,SubTotal,TaxAmt,Freight,TotalDue,Comment,Rowguid,ModifiedDate")] SalesOrderHeader salesOrderHeader)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salesOrderHeader);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.BillToAddressId);
            ViewData["CreditCardId"] = new SelectList(_context.CreditCards, "CreditCardId", "CreditCardId", salesOrderHeader.CreditCardId);
            ViewData["CurrencyRateId"] = new SelectList(_context.CurrencyRates, "CurrencyRateId", "CurrencyRateId", salesOrderHeader.CurrencyRateId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", salesOrderHeader.CustomerId);
            ViewData["SalesPersonId"] = new SelectList(_context.SalesPeople, "BusinessEntityId", "BusinessEntityId", salesOrderHeader.SalesPersonId);
            ViewData["ShipMethodId"] = new SelectList(_context.ShipMethods, "ShipMethodId", "ShipMethodId", salesOrderHeader.ShipMethodId);
            ViewData["ShipToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.ShipToAddressId);
            ViewData["TerritoryId"] = new SelectList(_context.SalesTerritories, "TerritoryId", "TerritoryId", salesOrderHeader.TerritoryId);
            return View(salesOrderHeader);
        }

        // GET: Sales/5/Edit
        [HttpGet("{id?}/Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderHeader = await _context.SalesOrderHeaders.FindAsync(id);
            if (salesOrderHeader == null)
            {
                return NotFound();
            }
            ViewData["BillToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.BillToAddressId);
            ViewData["CreditCardId"] = new SelectList(_context.CreditCards, "CreditCardId", "CreditCardId", salesOrderHeader.CreditCardId);
            ViewData["CurrencyRateId"] = new SelectList(_context.CurrencyRates, "CurrencyRateId", "CurrencyRateId", salesOrderHeader.CurrencyRateId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", salesOrderHeader.CustomerId);
            ViewData["SalesPersonId"] = new SelectList(_context.SalesPeople, "BusinessEntityId", "BusinessEntityId", salesOrderHeader.SalesPersonId);
            ViewData["ShipMethodId"] = new SelectList(_context.ShipMethods, "ShipMethodId", "ShipMethodId", salesOrderHeader.ShipMethodId);
            ViewData["ShipToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.ShipToAddressId);
            ViewData["TerritoryId"] = new SelectList(_context.SalesTerritories, "TerritoryId", "TerritoryId", salesOrderHeader.TerritoryId);
            return View(salesOrderHeader);
        }

        // POST: Sales/5/Edit
        [HttpPost("{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SalesOrderId,RevisionNumber,OrderDate,DueDate,ShipDate,Status,OnlineOrderFlag,SalesOrderNumber,PurchaseOrderNumber,AccountNumber,CustomerId,SalesPersonId,TerritoryId,BillToAddressId,ShipToAddressId,ShipMethodId,CreditCardId,CreditCardApprovalCode,CurrencyRateId,SubTotal,TaxAmt,Freight,TotalDue,Comment,Rowguid,ModifiedDate")] SalesOrderHeader salesOrderHeader)
        {
            if (id != salesOrderHeader.SalesOrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salesOrderHeader);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderHeaderExists(salesOrderHeader.SalesOrderId))
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
            ViewData["BillToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.BillToAddressId);
            ViewData["CreditCardId"] = new SelectList(_context.CreditCards, "CreditCardId", "CreditCardId", salesOrderHeader.CreditCardId);
            ViewData["CurrencyRateId"] = new SelectList(_context.CurrencyRates, "CurrencyRateId", "CurrencyRateId", salesOrderHeader.CurrencyRateId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", salesOrderHeader.CustomerId);
            ViewData["SalesPersonId"] = new SelectList(_context.SalesPeople, "BusinessEntityId", "BusinessEntityId", salesOrderHeader.SalesPersonId);
            ViewData["ShipMethodId"] = new SelectList(_context.ShipMethods, "ShipMethodId", "ShipMethodId", salesOrderHeader.ShipMethodId);
            ViewData["ShipToAddressId"] = new SelectList(_context.Addresses, "AddressId", "AddressId", salesOrderHeader.ShipToAddressId);
            ViewData["TerritoryId"] = new SelectList(_context.SalesTerritories, "TerritoryId", "TerritoryId", salesOrderHeader.TerritoryId);
            return View(salesOrderHeader);
        }

        // GET: Sales/5/Delete
        [HttpGet("{id?}/Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesOrderHeader = await _context.SalesOrderHeaders
                .Include(s => s.BillToAddress)
                .Include(s => s.CreditCard)
                .Include(s => s.CurrencyRate)
                .Include(s => s.Customer)
                .Include(s => s.SalesPerson)
                .Include(s => s.ShipMethod)
                .Include(s => s.ShipToAddress)
                .Include(s => s.Territory)
                .FirstOrDefaultAsync(m => m.SalesOrderId == id);
            if (salesOrderHeader == null)
            {
                return NotFound();
            }

            return View(salesOrderHeader);
        }

        // POST: Sales/Delete/5
        [HttpPost("{id}/Edit"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesOrderHeader = await _context.SalesOrderHeaders.FindAsync(id);
            if (salesOrderHeader != null)
            {
                _context.SalesOrderHeaders.Remove(salesOrderHeader);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SalesOrderHeaderExists(int id)
        {
            return _context.SalesOrderHeaders.Any(e => e.SalesOrderId == id);
        }

        private bool SaleHasInvoice(int saleID)
        {
            return _context.Invoices.Any(invoice => invoice.SalesOrderId == saleID);
        }
    }
}
