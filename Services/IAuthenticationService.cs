using ITTicketingSystem.Models;

namespace ITTicketingSystem.Services
{
    public interface IAuthenticationService
    {
        Task<(User? user, string? errorMessage)> AuthenticateAsync(string email, string password);
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hash);
        Task UpdateLastLoginAsync(int userId);
    }
}
