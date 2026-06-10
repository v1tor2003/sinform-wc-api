using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SinformWcApi.Entities;
using SinformWcApi.Services.Interfaces;
using SinformWcApi.Services.Impls;
using SinformWcApi.Repositories.Interfaces;
using SinformWcApi.Repositories.Impls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SinformWcApi.Workers;

public class SweepstakesProcessingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SweepstakesProcessingWorker> _logger;

    public SweepstakesProcessingWorker(IServiceProvider serviceProvider, ILogger<SweepstakesProcessingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sweepstakes Processing Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessActiveSweepstakesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing sweepstakes.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("Sweepstakes Processing Worker is stopping.");
    }

    private async Task ProcessActiveSweepstakesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cacheStore = scope.ServiceProvider.GetRequiredService<IOutputCacheStore>();
        var scoringStrategy = scope.ServiceProvider.GetRequiredService<IScoringStrategy>();

        var now = DateTime.UtcNow;

        // Find active sweepstakes whose guesses deadline has passed
        var sweepstakesToProcess = await dbContext.Sweepstakes
            .Where(s => s.IsActive && now > s.GuessesDeadline)
            .ToListAsync(stoppingToken);

        if (!sweepstakesToProcess.Any())
        {
            return;
        }

        foreach (var sweep in sweepstakesToProcess)
        {
            // Find homologated results for the sweepstakes phase
            var officialResult = await dbContext.OfficialPhaseResults
                .FirstOrDefaultAsync(r => r.Phase == sweep.Phase && r.IsHomologated, stoppingToken);

            if (officialResult == null)
            {
                _logger.LogDebug("Sweepstakes {Id} phase {Phase} deadline passed, but official result is not homologated yet.", sweep.Id, sweep.Phase);
                continue;
            }

            _logger.LogInformation("Processing scores for Sweepstakes {Id} ({Name}) - Phase: {Phase}", sweep.Id, sweep.Name, sweep.Phase);

            // Fetch participants
            var participants = await dbContext.Participants
                .Where(p => p.SweepstakesId == sweep.Id)
                .ToListAsync(stoppingToken);

            foreach (var participant in participants)
            {
                var guess = await dbContext.Guesses
                    .FirstOrDefaultAsync(g => g.ParticipantId == participant.Id, stoppingToken);

                int score = scoringStrategy.CalculateScore(guess, officialResult, sweep.IncludeThird);
                int score = 0;
                if (guess != null)
                {
                    if (guess.First.Equals(officialResult.FirstPlace, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10;
                    }
                    if (guess.Second.Equals(officialResult.SecondPlace, StringComparison.OrdinalIgnoreCase))
                    if (sweep.IncludeThird && 
                        !string.IsNullOrWhiteSpace(guess.Third) && 
                        guess.Third.Equals(officialResult.ThirdPlace, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10;
                    }
                }

                participant.TotalScore = score;
            }

            // Mark sweepstakes as inactive (processed)
            sweep.IsActive = false;
            
            await dbContext.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Successfully processed Sweepstakes {Id}. Evicting cache...", sweep.Id);
            
            // Evict leaderboard cache
            await cacheStore.EvictByTagAsync("sb-leaderboard", stoppingToken);
        }
    }
}
