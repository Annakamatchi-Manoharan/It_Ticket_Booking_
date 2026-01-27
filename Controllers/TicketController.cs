using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;

namespace ITTicketingSystem.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public TicketController(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTicketViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var ticket = new Ticket
                {
                    Subject = model.Subject,
                    Description = model.Description,
                    Priority = model.Priority,
                    Department = model.Department,
                    Category = model.Category,
                    Status = "Open",
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle attachments if any
                if (model.Attachments != null && model.Attachments.Any())
                {
                    var attachmentPaths = new List<string>();
                    foreach (var file in model.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            // For now, just store file names. In production, you'd save files to disk or cloud storage
                            attachmentPaths.Add(file.FileName);
                        }
                    }
                    ticket.Attachments = string.Join(",", attachmentPaths);
                }

                await _ticketRepository.CreateAsync(ticket);

                TempData["Success"] = "Ticket created successfully!";
                return RedirectToAction("MyTickets");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "An error occurred while creating the ticket. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var tickets = await _ticketRepository.GetByUserIdAsync(userId);
            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Check if user has permission to view this ticket
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (ticket.CreatedById != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> AssignedTickets()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Support"))
            {
                return Forbid();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var tickets = await _ticketRepository.GetByAssignedToIdAsync(userId);
            return View(tickets);
        }
    }
}
