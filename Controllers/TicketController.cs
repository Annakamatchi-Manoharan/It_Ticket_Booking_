using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
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
                Status = "Open",
                CreatedAt = DateTime.Now
            };

            if (model.Attachments != null && model.Attachments.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");
                
                // Ensure the directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                
                var savedFiles = new List<string>();
                
                foreach (var file in model.Attachments)
                {
                    if (file.Length > 0)
                    {
                        // Create a unique filename to avoid conflicts
                        var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        
                        savedFiles.Add(uniqueFileName);
                    }
                }
                
                ticket.Attachments = string.Join(";", savedFiles);
            }

            // Debug: Check available engineers before assignment
            var availableEngineers = await _userRepository.GetAvailableEngineersAsync();
            var availableEngineersList = availableEngineers.ToList();
            var nextEngineer = await _userRepository.GetNextAvailableEngineerRoundRobinAsync();

            await _ticketRepository.CreateWithAutoAssignmentAsync(ticket);

            // Debug: Check ticket after assignment
            var createdTicket = await _ticketRepository.GetByIdAsync(ticket.Id);

            TempData["SuccessMessage"] = $"Ticket created successfully! Your ticket #{ticket.Id} has been submitted and will be processed shortly. " +
                $"Available engineers: {availableEngineersList.Count}, Assigned to: {createdTicket?.AssignedToId}";

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateTicket(int ticketId, string status, string priority, string department, string category, string subject, string description, string teamViewerId, string teamViewerPassword, string contactEmail, string contactNumber)
        {
            try
            {
                // Get the current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Find the ticket
                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null)
                {
                    return Json(new { success = false, message = "Ticket not found" });
                }
                
                // Update ticket properties
                ticket.Status = status;
                ticket.Priority = priority;
                ticket.Department = department;
                ticket.Category = category;
                ticket.Subject = subject;
                ticket.Description = description;
                ticket.TeamViewerId = teamViewerId;
                ticket.TeamViewerPassword = teamViewerPassword;
                ticket.ContactEmail = contactEmail;
                ticket.ContactNumber = contactNumber;
                
                // Update the ticket
                await _ticketRepository.UpdateAsync(ticket);
                
                return Json(new { success = true, message = "Ticket updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating ticket: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(bool isAvailable)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Debug: Log the received value
                System.Diagnostics.Debug.WriteLine($"ToggleAvailability called with isAvailable: {isAvailable} for user: {userId}");

                await _userRepository.UpdateAvailabilityAsync(userId, isAvailable);
                
                // Debug: Verify the update
                var updatedUser = await _userRepository.GetByIdAsync(userId);
                System.Diagnostics.Debug.WriteLine($"After update - User {userId} IsAvailable: {updatedUser?.IsAvailable}");
                
                // If engineer is becoming available, assign existing unassigned tickets
                int assignedCount = 0;
                if (isAvailable)
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null && user.Role == "Engineer")
                    {
                        // Get all unassigned tickets for debugging
                        var allTickets = await _ticketRepository.GetAllAsync();
                        var unassignedCount = allTickets.Count(t => 
                            (t.Status == "Open" || t.Status == "In-Progress") && 
                            (t.AssignedToId == null || 
                             t.AssignedTo == null || 
                             t.AssignedTo?.Role != "Engineer"));

                        var assignedTickets = await _ticketRepository.AssignUnassignedTicketsToEngineerAsync(userId);
                        assignedCount = assignedTickets.Count;
                    }
                }
                
                return Json(new { 
                    success = true, 
                    message = isAvailable ? 
                        (assignedCount > 0 ? 
                            $"You are now available and {assignedCount} ticket(s) have been assigned to you!" : 
                            "You are now available (no pending tickets to assign)") : 
                        "You are now off duty",
                    isAvailable = isAvailable,
                    assignedTicketsCount = assignedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating availability: " + ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> DownloadAttachment(string fileName)
        {
            try
            {
                // For now, we'll return a placeholder response
                // In a real application, you would:
                // 1. Validate the user has access to this ticket
                // 2. Find the file in your storage system (wwwroot/uploads/tickets/)
                // 3. Return the actual file

                // For demonstration, return a simple file response
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    var contentType = GetContentType(fileName);
                    return File(fileBytes, contentType, fileName);
                }
                else
                {
                    // Return a placeholder file if the actual file doesn't exist
                    var placeholderContent = $"Attachment file '{fileName}' would be downloaded here.\n\nIn a real application, this would be the actual file content.";
                    var placeholderBytes = System.Text.Encoding.UTF8.GetBytes(placeholderContent);
                    return File(placeholderBytes, "text/plain", $"{fileName}.txt");
                }
            }
            catch (Exception ex)
            {
                // Log the error and return a user-friendly message
                return BadRequest("Unable to download attachment. Please contact support.");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".svg" => "image/svg+xml",
                ".log" => "text/plain",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".ppt" => "application/vnd.ms-powerpoint",
                _ => "application/octet-stream"
            };
        }
    }
}
