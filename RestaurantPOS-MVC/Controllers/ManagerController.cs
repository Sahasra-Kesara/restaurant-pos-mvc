using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS_MVC.Data;
using RestaurantPOS_MVC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ManagerController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManagerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // View all menu items
    public async Task<IActionResult> MenuManagement()
    {
        var items = await _context.Items.ToListAsync();
        return View(items);
    }

    // Add a new menu item
    public IActionResult AddMenuItem()
    {
        ViewBag.Categories = _context.Items.Select(i => i.Category).Distinct().ToList();
        ViewBag.SubCategories = _context.Items.Select(i => i.SubCategory).Distinct().ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddMenuItem(Item item, string? NewCategory, string? NewSubCategory)
    {
        // Use new category or existing one
        if (!string.IsNullOrEmpty(NewCategory))
        {
            item.Category = NewCategory;
        }

        // Use new subcategory or existing one
        if (!string.IsNullOrEmpty(NewSubCategory))
        {
            item.SubCategory = NewSubCategory;
        }

        if (ModelState.IsValid)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MenuManagement));
        }

        ViewBag.Categories = _context.Items.Select(i => i.Category).Distinct().ToList();
        ViewBag.SubCategories = _context.Items.Select(i => i.SubCategory).Distinct().ToList();
        return View(item);
    }

    // Edit an existing menu item
    public async Task<IActionResult> EditMenuItem(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> EditMenuItem(Item item)
    {
        if (ModelState.IsValid)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MenuManagement));
        }
        return View(item);
    }

    // Delete a menu item
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(MenuManagement));
    }

    // View orders
    public async Task<IActionResult> OrdersReport(DateTime? reportDate)
    {
        var ordersQuery = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Item)  // Ensure the Item is included as well
            .AsQueryable();

        // If a specific date is selected, filter by that date
        if (reportDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == reportDate.Value.Date);
            ViewData["ReportDate"] = reportDate.Value.ToString("yyyy-MM-dd");
        }
        else
        {
            // Default to today's date if no date is selected
            reportDate = DateTime.Today;
            ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == reportDate.Value.Date);
            ViewData["ReportDate"] = reportDate.Value.ToString("yyyy-MM-dd");
        }

        var orders = await ordersQuery.ToListAsync();
        return View(orders);
    }



    // Generate reports
    public async Task<IActionResult> SalesReport(DateTime? reportDate)
    {
        // If the report date is not passed, default to today
        if (!reportDate.HasValue)
        {
            reportDate = DateTime.Today;
        }

        // Query the bills based on the selected date
        var sales = await _context.Bills
            .Where(b => b.PaymentDate.Date == reportDate.Value.Date)
            .ToListAsync();

        // Calculate the total sales for the selected date
        var totalSales = sales.Sum(b => b.TotalAmount);

        // Store the report date and total sales in ViewData
        ViewData["ReportDate"] = reportDate.Value.ToString("yyyy-MM-dd");
        ViewData["TotalSales"] = totalSales;

        // Return the view with the filtered sales data
        return View(sales);
    }


    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> ItemsReport()
    {
        var items = await _context.Items.ToListAsync();  // Get all items
        return View(items);  // Pass the list of items to the view
    }




}