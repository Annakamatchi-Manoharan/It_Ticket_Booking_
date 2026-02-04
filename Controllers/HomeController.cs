using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;
using System.Diagnostics;
using System.Security.Claims;

namespace ITTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        
        public HomeController(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
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
                // For User role, show user-specific dashboard
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userId, out int currentUserId))
                {
                    // Get tickets created by this specific user
                    var userTickets = _ticketRepository.GetByUserIdAsync(currentUserId).Result.ToList();
                    var totalTicketsCount = userTickets.Count;
                    var userPendingTicketsCount = userTickets.Count(t => t.Status == "Open" || t.Status == "In-Progress");
                    var resolvedTicketsCount = userTickets.Count(t => t.Status == "Resolved");
                    
                    ViewBag.TotalTicketsCount = totalTicketsCount;
                    ViewBag.PendingTicketsCount = userPendingTicketsCount;
                    ViewBag.ResolvedTicketsCount = resolvedTicketsCount;
                    ViewBag.UserTickets = userTickets.Take(5).ToList(); // Show recent 5 tickets
                }
                else
                {
                    ViewBag.TotalTicketsCount = 0;
                    ViewBag.PendingTicketsCount = 0;
                    ViewBag.ResolvedTicketsCount = 0;
                    ViewBag.UserTickets = new List<Ticket>();
                }
                
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
            if (int.TryParse(userId, out int currentUserId))
            {
                // Get user info including availability
                var currentUser = _userRepository.GetByIdAsync(currentUserId).Result;
                
                // Debug: Log the loaded availability
                System.Diagnostics.Debug.WriteLine($"EngineerDashboard - User {currentUserId} loaded with IsAvailable: {currentUser?.IsAvailable}");
                
                // Get only tickets assigned to this engineer
                var assignedTickets = _ticketRepository.GetByAssignedToIdAsync(currentUserId).Result.ToList();
                var assignedTicketsCount = assignedTickets.Count;
                var pendingResponseCount = assignedTickets.Count(t => t.Status == "Open");
                var slaBreachedCount = 0; // Would calculate based on SLA rules
                var avgResolutionTime = "2.4h"; // Would calculate from actual data
                
                ViewBag.AssignedTickets = assignedTickets;
                ViewBag.AssignedTicketsCount = assignedTicketsCount;
                ViewBag.PendingResponseCount = pendingResponseCount;
                ViewBag.SlaBreachedCount = slaBreachedCount;
                ViewBag.AvgResolutionTime = avgResolutionTime;
                ViewBag.UserIsAvailable = currentUser?.IsAvailable ?? false;
                
                // Debug: Log what's being sent to view
                System.Diagnostics.Debug.WriteLine($"EngineerDashboard - ViewBag.UserIsAvailable: {ViewBag.UserIsAvailable}");
            }
            else
            {
                ViewBag.AssignedTickets = new List<Ticket>();
                ViewBag.AssignedTicketsCount = 0;
                ViewBag.PendingResponseCount = 0;
                ViewBag.SlaBreachedCount = 0;
                ViewBag.AvgResolutionTime = "0h";
                ViewBag.UserIsAvailable = false;
            }
            
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
