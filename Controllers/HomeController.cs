using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ITTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Redirect to login page
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Redirect to appropriate dashboard based on role
            if (userRole == "Manager")
            {
                return RedirectToAction("ManagerDashboard");
            }
            
            // Default dashboard for other roles
            return View();
        }

        [Authorize]
        public IActionResult ManagerDashboard()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}
