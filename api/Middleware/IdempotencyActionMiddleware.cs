using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using SinformWcApi.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SinformWcApi.Middleware;

public class IdempotencyActionMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyActionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
    {
        var endpoint = context.GetEndpoint();
        var idempotentAttr = endpoint?.Metadata.GetMetadata<IdempotentAttribute>();

        if (idempotentAttr == null)
        {
            await _next(context);
            return;
        }

        // 1. Idempotency validation
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKeyValues) || 
            string.IsNullOrWhiteSpace(idempotencyKeyValues.ToString()))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "Idempotency-Key header is required." });
            return;
        }

        var idempotencyKey = idempotencyKeyValues.ToString();
        var cacheKey = $"idemp:{idempotencyKey}";

        // Check cache
        var cachedJson = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedJson);
            return;
        }

        // Intercept response body
        var originalResponseBodyStream = context.Response.Body;
        using var responseBodyTracker = new MemoryStream();
        context.Response.Body = responseBodyTracker;

        try
        {
            await _next(context);

            // If success (200 OK or 201 Created), cache it
            if (context.Response.StatusCode == StatusCodes.Status200OK || 
                context.Response.StatusCode == StatusCodes.Status201Created)
            {
                responseBodyTracker.Seek(0, SeekOrigin.Begin);
                var responseJson = await new StreamReader(responseBodyTracker).ReadToEndAsync();
                
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                await cache.SetStringAsync(cacheKey, responseJson, cacheOptions);
            }
        }
        finally
        {
            responseBodyTracker.Seek(0, SeekOrigin.Begin);
            await responseBodyTracker.CopyToAsync(originalResponseBodyStream);
            context.Response.Body = originalResponseBodyStream;
        }
    }
}
