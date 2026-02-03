using ITTicketingSystem.Models;

namespace ITTicketingSystem.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<User>> GetAvailableEngineersAsync();
        Task<User?> GetNextAvailableEngineerAsync();
        Task<User?> GetNextAvailableEngineerRoundRobinAsync();
        Task UpdateAvailabilityAsync(int userId, bool isAvailable);
    }
}
