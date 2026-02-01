using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;
using System.Security.Claims;
using System.Linq;

namespace ITTicketingSystem.Controllers
{
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public TicketController(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        public IActionResult Create()
        {
            return View(new CreateTicketViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = new Ticket
            {
                Subject = model.Subject,
                Description = model.Description,
                Priority = model.Priority,
                Department = model.Department,
                Category = model.Category,
                WorkLocation = model.WorkLocation,
                TeamViewerId = model.TeamViewerId,
                TeamViewerPassword = model.TeamViewerPassword,
                ContactNumber = model.ContactNumber,
                ContactEmail = model.ContactEmail,
                CreatedById = userId,
                Status = "Open"
            };

            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                ticket.Attachments = string.Join(";", model.Attachments.Select(a => a.FileName));
            }

            await _ticketRepository.CreateAsync(ticket);

            TempData["SuccessMessage"] = "Ticket completed successfully! Your ticket has been submitted and will be processed shortly.";

            return RedirectToAction("MyTickets", "Ticket");
        }

        [Authorize]
        public async Task<IActionResult> MyTickets(int page = 1, int pageSize = 10, string search = "", string status = "All", string category = "All")
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var allUserTickets = await _ticketRepository.GetByUserIdAsync(userId);
            
            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                allUserTickets = allUserTickets.Where(t => 
                    t.Subject.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Department.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Category.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            // Apply status filter
            if (status != "All")
            {
                allUserTickets = allUserTickets.Where(t => t.Status == status).ToList();
            }
            
            // Apply category filter
            if (category != "All")
            {
                allUserTickets = allUserTickets.Where(t => t.Category == category).ToList();
            }
            
            var totalCount = allUserTickets.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            
            var pagedTickets = allUserTickets
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Category = category;

            return View(pagedTickets);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            return RedirectToAction("Dashboard", "Home");
        }

        [Authorize]
        public async Task<IActionResult> AssignedTickets()
        {
            return RedirectToAction("Dashboard", "Home");
        }
    }
}
