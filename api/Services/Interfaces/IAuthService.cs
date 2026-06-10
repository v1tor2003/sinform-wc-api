using SinformWcApi.Entities;
using System.Threading.Tasks;

namespace SinformWcApi.Services.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(string name, string email, string password);
    Task<string?> LoginAsync(string email, string password);
}
