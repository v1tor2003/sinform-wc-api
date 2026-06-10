using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SinformWcApi.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (context.Request.Headers.TryGetValue("X-API-KEY", out var apiKeyValues))
        {
            var apiKey = apiKeyValues.ToString();
            if (!string.IsNullOrEmpty(apiKey))
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey == apiKey);
                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email)
                    }, "ApiKey");

                    context.User = new ClaimsPrincipal(identity);
                    
                    // Store user object in Items for direct endpoint access
                    context.Items["User"] = user;
                }
            }
        }

        await _next(context);
    }
}

public static class ApiKeyAuthenticationExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }

    public static RouteHandlerBuilder RequireApiKey(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }
            return await next(context);
        });
    }
}
