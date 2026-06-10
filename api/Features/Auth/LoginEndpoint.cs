using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Services.Impls;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Features.Auth;

public static class LoginEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password);
    public record Response(string ApiKey);

    public static void MapLoginEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", async (Request request, IAuthService authService) =>
        {
            var apiKey = await authService.LoginAsync(request.Email, request.Password);
            if (apiKey == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new Response(apiKey));
        })
        .WithName("LoginUser")
        .WithTags("Auth");
    }
}
