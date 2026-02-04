using ITTicketingSystem.Data;
using ITTicketingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTicketingSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            
            // If user is an engineer and just logged in, set them to available
            if (user != null && user.Role == "Engineer")
            {
                user.IsAvailable = true;
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            else if (user != null)
            {
                // Update last login for other roles
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetAvailableEngineersAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Engineer" && u.IsActive && u.IsAvailable)
                .OrderBy(u => u.CreatedAt) // FIFO order
                .ToListAsync();
        }

        public async Task<User?> GetNextAvailableEngineerAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Engineer" && u.IsActive && u.IsAvailable)
                .OrderBy(u => u.CreatedAt) // FIFO order
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetNextAvailableEngineerRoundRobinAsync()
        {
            var availableEngineers = await _context.Users
                .Where(u => u.Role == "Engineer" && u.IsActive && u.IsAvailable)
                .OrderBy(u => u.Id) // Consistent ordering
                .ToListAsync();

            if (!availableEngineers.Any())
                return null;

            // Get ticket counts for each available engineer
            var engineerTicketCounts = await _context.Tickets
                .Where(t => t.AssignedToId.HasValue && availableEngineers.Select(e => e.Id).Contains(t.AssignedToId.Value))
                .Where(t => t.Status != "Resolved" && t.Status != "Closed")
                .GroupBy(t => t.AssignedToId)
                .Select(g => new { EngineerId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Create dictionary of ticket counts
            var ticketCountDict = engineerTicketCounts.ToDictionary(x => x.EngineerId, x => x.Count);

            // Find engineer with minimum tickets (round-robin based on workload)
            var selectedEngineer = availableEngineers
                .OrderBy(e => ticketCountDict.GetValueOrDefault(e.Id, 0))
                .ThenBy(e => e.Id) // Tie-breaker by ID
                .First();

            return selectedEngineer;
        }

        public async Task UpdateAvailabilityAsync(int userId, bool isAvailable)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsAvailable = isAvailable;
                await UpdateAsync(user);
            }
        }
    }
}
