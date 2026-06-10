using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Services.Impls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

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

    public static void MapCreateOfficialResultEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/official-results", async (Request request, IOfficialResultService officialResultService) =>
        {
            var result = await officialResultService.CreateOrUpdateAsync(request.Phase, request.FirstPlace, request.SecondPlace, request.ThirdPlace);
            return Results.Ok(result);
        })
        .WithName("CreateOfficialResult")
        .WithTags("OfficialResults");
    }
}
