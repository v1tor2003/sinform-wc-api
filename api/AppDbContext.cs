using Microsoft.EntityFrameworkCore;

namespace SinformWcApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
