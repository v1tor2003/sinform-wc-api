using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Repositories;
using System;
using System.Threading.Tasks;
using SinformWcApi.Repositories.Interfaces;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Services.Impls;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterAsync(string name, string email, string password)
    {
        var emailNormalized = email.Trim().ToLower();

        var exists = await _userRepository.ExistsByEmailAsync(emailNormalized);
        if (exists)
        {
            throw new DomainException("Email is already registered.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var apiKey = "usr_live_" + Guid.NewGuid().ToString("N");

        var newUser = new User
        {
            Name = name,
            Email = emailNormalized,
            PasswordHash = passwordHash,
            ApiKey = apiKey,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return newUser;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var emailNormalized = email.Trim().ToLower();

        var user = await _userRepository.GetByEmailAsync(emailNormalized);
        if (user == null)
        {
            return null;
        }

        var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!isValid)
        {
            return null;
        }

        return user.ApiKey;
    }
}
