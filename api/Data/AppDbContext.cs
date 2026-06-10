using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;

namespace SinformWcApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Sweepstakes> Sweepstakes => Set<Sweepstakes>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Guess> Guesses => Set<Guess>();
    public DbSet<OfficialPhaseResult> OfficialPhaseResults => Set<OfficialPhaseResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.ApiKey).IsUnique();
        });

        modelBuilder.Entity<Sweepstakes>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Phase).IsRequired().HasMaxLength(50);
            entity.Property(e => e.InviteCode).IsRequired().HasMaxLength(6);
            entity.HasIndex(e => e.InviteCode).IsUnique();

            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.SweepstakesId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Sweepstakes)
                  .WithMany(s => s.Participants)
                  .HasForeignKey(e => e.SweepstakesId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Guess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.First).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Second).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Third).HasMaxLength(100);

            entity.HasIndex(e => e.ParticipantId).IsUnique();

            entity.HasOne(e => e.Participant)
                  .WithOne()
                  .HasForeignKey<Guess>(e => e.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfficialPhaseResult>(entity =>
        {
            entity.HasKey(e => e.Phase);
            entity.Property(e => e.Phase).HasMaxLength(50);
            entity.Property(e => e.FirstPlace).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SecondPlace).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ThirdPlace).HasMaxLength(100);
        });
    }
}
