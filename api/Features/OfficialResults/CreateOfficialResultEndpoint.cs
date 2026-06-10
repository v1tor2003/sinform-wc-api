using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Services.Impls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;

namespace SinformWcApi.Features.OfficialResults;

public static class CreateOfficialResultEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Phase is required.")]
        string Phase,
        [Required(ErrorMessage = "FirstPlace is required.")]
        string FirstPlace,
        [Required(ErrorMessage = "SecondPlace is required.")]
        string SecondPlace,
        string? ThirdPlace);
    public record Request(string Phase, string FirstPlace, string SecondPlace, string? ThirdPlace);

    public static void MapCreateOfficialResultEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/official-results", async (Request request, IOfficialResultService officialResultService) =>
        {
            var result = await officialResultService.CreateOrUpdateAsync(request.Phase, request.FirstPlace, request.SecondPlace, request.ThirdPlace);
        endpoints.MapPost("/official-results", async (Request request, AppDbContext dbContext) =>
            if (string.IsNullOrWhiteSpace(request.Phase) || 
                string.IsNullOrWhiteSpace(request.FirstPlace) || 
                string.IsNullOrWhiteSpace(request.SecondPlace))
            {
                return Results.BadRequest("Phase, FirstPlace, and SecondPlace are required.");
            }

            var result = await dbContext.OfficialPhaseResults.FirstOrDefaultAsync(r => r.Phase == request.Phase);
            if (result == null)
                result = new OfficialPhaseResult
                {
                    Phase = request.Phase,
                    FirstPlace = request.FirstPlace.Trim(),
                    SecondPlace = request.SecondPlace.Trim(),
                    ThirdPlace = request.ThirdPlace?.Trim() ?? string.Empty,
                    IsHomologated = true,
                    HomologatedAt = DateTime.UtcNow
                };
                dbContext.OfficialPhaseResults.Add(result);
            else
                result.FirstPlace = request.FirstPlace.Trim();
                result.SecondPlace = request.SecondPlace.Trim();
                result.ThirdPlace = request.ThirdPlace?.Trim() ?? string.Empty;
                result.IsHomologated = true;
                result.HomologatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return Results.Ok(result);
        })
        .WithName("CreateOfficialResult")
        .WithTags("OfficialResults");
    }
}
