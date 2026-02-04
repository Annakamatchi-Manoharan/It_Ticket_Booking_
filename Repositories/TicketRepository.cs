using ITTicketingSystem.Data;
using ITTicketingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTicketingSystem.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        public TicketRepository(ApplicationDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> CreateWithAutoAssignmentAsync(Ticket ticket)
        {
            // Auto-assign to available engineer using round-robin based on workload
            var nextAvailableEngineer = await _userRepository.GetNextAvailableEngineerRoundRobinAsync();
            
            if (nextAvailableEngineer != null)
            {
                ticket.AssignedToId = nextAvailableEngineer.Id;
                ticket.Status = "Assigned";
                
                // Debug: Log successful assignment
                System.Diagnostics.Debug.WriteLine($"Ticket {ticket.Id} assigned to engineer {nextAvailableEngineer.Id} ({nextAvailableEngineer.Email})");
            }
            else
            {
                // No engineers available, keep as Open
                ticket.Status = "Open";
                
                // Debug: Log no available engineers
                System.Diagnostics.Debug.WriteLine($"Ticket {ticket.Id} created but no engineers available - status set to Open");
            }
            
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<List<Ticket>> AssignUnassignedTicketsToEngineerAsync(int engineerId)
        {
            // Get all tickets that are not assigned to any engineer (either null AssignedToId or assigned to non-engineers)
            var unassignedTickets = await _context.Tickets
                .Where(t => (t.Status == "Open" || t.Status == "In-Progress") && 
                           (t.AssignedToId == null || 
                            !_context.Users.Any(u => u.Id == t.AssignedToId && u.Role == "Engineer")))
                .OrderBy(t => t.CreatedAt) // Assign oldest tickets first
                .ToListAsync();

            var assignedTickets = new List<Ticket>();

            foreach (var ticket in unassignedTickets)
            {
                ticket.AssignedToId = engineerId;
                ticket.Status = "Assigned";
                ticket.UpdatedAt = DateTime.Now;
                assignedTickets.Add(ticket);
            }

            await _context.SaveChangesAsync();
            return assignedTickets;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.CreatedById == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(int assignedToId)
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.AssignedToId == assignedToId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByStatusAsync(string status)
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByPriorityAsync(string priority)
        {
            return await _context.Tickets
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
