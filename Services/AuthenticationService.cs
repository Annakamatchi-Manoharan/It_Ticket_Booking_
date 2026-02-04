using ITTicketingSystem.Models;
using ITTicketingSystem.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace ITTicketingSystem.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(User? user, string? errorMessage)> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (null, "Email and password are required");
            }

            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                return (null, "Invalid email or password");
            }

            if (!user.IsActive)
            {
                return (null, "Account is deactivated");
            }

            // Compare plain text passwords
            if (user.PasswordHash != password)
            {
                return (null, "Invalid email or password");
            }

            await UpdateLastLoginAsync(user.Id);
            
            return (user, null);
        }

        public async Task<string> HashPasswordAsync(string password)
        {
            // Return plain text password as requested
            return password;
        }

        public async Task<bool> VerifyPasswordAsync(string password, string storedPassword)
        {
            // Compare plain text passwords
            return password == storedPassword;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }
        }
    }
}
