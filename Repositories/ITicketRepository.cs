using ITTicketingSystem.Models;

namespace ITTicketingSystem.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<Ticket> CreateWithAutoAssignmentAsync(Ticket ticket);
        Task<List<Ticket>> AssignUnassignedTicketsToEngineerAsync(int engineerId);
        Task<Ticket> UpdateAsync(Ticket ticket);
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<IEnumerable<Ticket>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Ticket>> GetByAssignedToIdAsync(int assignedToId);
        Task<IEnumerable<Ticket>> GetByStatusAsync(string status);
        Task<IEnumerable<Ticket>> GetByPriorityAsync(string priority);
    }
}
