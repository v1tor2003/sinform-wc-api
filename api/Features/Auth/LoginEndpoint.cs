using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Auth;

public static class LoginEndpoint
{
    public record Request(string Email, string Password);
    public record Response(string ApiKey);

    public static void MapLoginEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", async (Request request, AppDbContext dbContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Email and Password are required.");
            }

            var emailNormalized = request.Email.Trim().ToLower();

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new Response(user.ApiKey));
        })
        .WithName("LoginUser")
        .WithTags("Auth");
    }
}
