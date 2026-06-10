using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Entities;
using System;
using SinformWcApi.Services.Impls;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Features.Auth;

public static class RegisterEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Name is required.")]
        string Name,
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password);
    public record Response(string Message);

    public static void MapRegisterEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/register", async (Request request, IAuthService authService) =>
        {
            var newUser = await authService.RegisterAsync(request.Name, request.Email, request.Password);
            return Results.Created($"/api/v1/users/{newUser.Id}", new Response("User registered successfully."));
        })
        .WithName("RegisterUser")
        .WithTags("Auth");
    }
}
