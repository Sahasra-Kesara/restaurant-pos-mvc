using Microsoft.AspNetCore.Mvc;
using RestaurantPOS_MVC.Models;
using RestaurantPOS_MVC.Data;
using System.Linq;

namespace RestaurantPOS_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    // Redirect based on user role
                    if (user.Role == "Manager")
                    {
                        return RedirectToAction("Index", "Manager");
                    }
                    else if (user.Role == "Cashier")
                    {
                        return RedirectToAction("Index", "POS");
                    }
                }
                else
                {
                    // Invalid login attempt
                    ViewData["ErrorMessage"] = "Invalid username or password.";
                }
            }
            return View(model);
        }

        // GET: Logout
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}
