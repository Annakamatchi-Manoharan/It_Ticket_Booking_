using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ITTicketingSystem.Repositories;
using System.Security.Claims;

namespace ITTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        
        public HomeController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public IActionResult Index()
        {
            // Redirect to login page
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // Get ticket statistics
            var allTickets = _ticketRepository.GetAllAsync().Result;
            var openTicketsCount = allTickets.Count(t => t.Status == "Open");
            var pendingTicketsCount = allTickets.Count(t => t.Status == "In-Progress");
            var resolvedTodayCount = allTickets.Count(t => t.Status == "Resolved" && t.CreatedAt.Date == DateTime.Today);
            
            ViewBag.OpenTicketsCount = openTicketsCount;
            ViewBag.PendingTicketsCount = pendingTicketsCount;
            ViewBag.ResolvedTodayCount = resolvedTodayCount;
            
            // Redirect to appropriate dashboard based on role
            if (userRole == "Manager")
            {
                return RedirectToAction("ManagerDashboard");
            }
            else if (userRole == "Engineer")
            {
                return RedirectToAction("EngineerDashboard");
            }
            else if (userRole == "User")
            {
                // For User role, show simplified dashboard
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userAllTickets = _ticketRepository.GetAllAsync().Result;
                
                // Get user-specific tickets (for demo, we'll use all tickets)
                var userTickets = userAllTickets.ToList(); // In real app: .Where(t => t.CreatedById == userId)
                var userPendingCount = userTickets.Count(t => t.Status == "Open" || t.Status == "In-Progress");
                var userResolvedCount = userTickets.Count(t => t.Status == "Resolved");
                
                ViewBag.UserTicketsCount = userTickets.Count;
                ViewBag.UserPendingCount = userPendingCount;
                ViewBag.UserResolvedCount = userResolvedCount;
                
                return View("UserDashboard");
            }
            
            // Default dashboard for other roles
            return View();
        }

        [Authorize]
        public IActionResult ManagerDashboard()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return View();
        }

        [Authorize]
        public IActionResult EngineerDashboard()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            // Get engineer-specific statistics
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allTickets = _ticketRepository.GetAllAsync().Result;
            
            // For demo purposes, assign some tickets to the current user
            // In a real app, you'd have an AssignedTo field in tickets
            var assignedTickets = allTickets.Where(t => t.Status == "Open" || t.Status == "In-Progress").Take(5).ToList();
            var assignedTicketsCount = assignedTickets.Count;
            var pendingResponseCount = assignedTickets.Count(t => t.Status == "Open");
            var slaBreachedCount = 0; // Would calculate based on SLA rules
            var avgResolutionTime = "2.4h"; // Would calculate from actual data
            
            ViewBag.AssignedTickets = assignedTickets;
            ViewBag.AssignedTicketsCount = assignedTicketsCount;
            ViewBag.PendingResponseCount = pendingResponseCount;
            ViewBag.SlaBreachedCount = slaBreachedCount;
            ViewBag.AvgResolutionTime = avgResolutionTime;
            
            return View("EngineerDashboard");
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
