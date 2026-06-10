using Microsoft.AspNetCore.Http;
using SinformWcApi.Contexts;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SinformWcApi.Middleware;

public class AttachUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public AttachUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                userContext.UserId = userId;
                userContext.Name = context.User.Identity.Name;
                userContext.Email = context.User.FindFirst(ClaimTypes.Email)?.Value;
            }
        }

        await _next(context);
    }
}
