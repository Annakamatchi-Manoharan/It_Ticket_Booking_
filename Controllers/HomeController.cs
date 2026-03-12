using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;
using System.Diagnostics;
using System.Security.Claims;
using ITTicketingSystem.Services;

namespace ITTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;
        
        public HomeController(ITicketRepository ticketRepository, IUserRepository userRepository, IAuthenticationService authenticationService)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _authenticationService = authenticationService;
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
        [HttpGet]
        public async Task<IActionResult> TeamOverview(int? editId = null)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = (await _userRepository.GetAllAsync())
                .Where(u => u.IsActive)
                .ToList();

            var model = new TeamOverviewViewModel
            {
                Users = users
            };

            if (editId.HasValue)
            {
                var editUser = await _userRepository.GetByIdAsync(editId.Value);
                if (editUser != null && editUser.IsActive)
                {
                    model.EditUserId = editUser.Id;
                    model.EditName = editUser.FullName.Replace(" -", "").Trim();
                    model.EditEmail = editUser.Email;
                    model.EditRole = editUser.Role ?? "User";
                }
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeamOverview(TeamOverviewViewModel model)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove(nameof(model.EditName));
            ModelState.Remove(nameof(model.EditEmail));
            ModelState.Remove(nameof(model.EditPassword));
            ModelState.Remove(nameof(model.EditConfirmPassword));
            ModelState.Remove(nameof(model.EditRole));

            var normalizedEmail = model.Email.Trim();
            var activeUsers = (await _userRepository.GetAllAsync()).ToList();
            var duplicateUser = activeUsers.FirstOrDefault(u =>
                u.IsActive &&
                string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (duplicateUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "This email already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.Users = activeUsers
                    .Where(u => u.IsActive)
                    .ToList();
                return View(model);
            }

            var nameParts = model.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.FirstOrDefault() ?? model.Name.Trim();
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var user = new User
            {
                FirstName = firstName,
                LastName = string.IsNullOrWhiteSpace(lastName) ? "-" : lastName,
                Email = normalizedEmail,
                PasswordHash = await _authenticationService.HashPasswordAsync(model.Password),
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _userRepository.CreateAsync(user);

            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(TeamOverview));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeamUser(TeamOverviewViewModel model)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove(nameof(model.Name));
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));
            ModelState.Remove(nameof(model.Role));

            if (!model.EditUserId.HasValue || model.EditUserId.Value <= 0)
            {
                return RedirectToAction(nameof(TeamOverview));
            }

            var existingRecord = await _userRepository.GetByIdAsync(model.EditUserId.Value);
            if (existingRecord == null || !existingRecord.IsActive)
            {
                TempData["SuccessMessage"] = "User not found.";
                return RedirectToAction(nameof(TeamOverview));
            }

            var activeUsers = (await _userRepository.GetAllAsync())
                .Where(u => u.IsActive)
                .ToList();

            var normalizedEmail = string.IsNullOrWhiteSpace(model.EditEmail)
                ? existingRecord.Email
                : model.EditEmail.Trim();

            var duplicateUser = activeUsers.FirstOrDefault(u =>
                string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
                u.Id != existingRecord.Id);

            if (duplicateUser != null)
            {
                ModelState.AddModelError(nameof(model.EditEmail), "This email already exists.");
            }

            var isChangingPassword = !string.IsNullOrWhiteSpace(model.EditPassword) || !string.IsNullOrWhiteSpace(model.EditConfirmPassword);
            if (!isChangingPassword)
            {
                ModelState.Remove(nameof(model.EditPassword));
                ModelState.Remove(nameof(model.EditConfirmPassword));
            }

            if (isChangingPassword)
            {
                if (string.IsNullOrWhiteSpace(model.EditPassword))
                {
                    ModelState.AddModelError(nameof(model.EditPassword), "Password is required.");
                }

                if (string.IsNullOrWhiteSpace(model.EditConfirmPassword))
                {
                    ModelState.AddModelError(nameof(model.EditConfirmPassword), "Confirm password is required.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Users = activeUsers;
                return View("TeamOverview", model);
            }

            var nameParts = model.EditName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            existingRecord.FirstName = nameParts.FirstOrDefault() ?? model.EditName.Trim();
            existingRecord.LastName = nameParts.Length > 1 ? nameParts[1] : "-";
            existingRecord.Email = normalizedEmail;
            existingRecord.Role = model.EditRole;

            if (isChangingPassword)
            {
                existingRecord.PasswordHash = await _authenticationService.HashPasswordAsync(model.EditPassword);
            }

            await _userRepository.UpdateAsync(existingRecord);

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(TeamOverview));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeamUser(int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole == "User")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var deleted = await _userRepository.DeleteAsync(id);
            if (deleted)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }

            return RedirectToAction(nameof(TeamOverview));
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
