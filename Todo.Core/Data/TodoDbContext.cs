using Microsoft.EntityFrameworkCore;
using Todo.Core.Models;

namespace Todo.Core.Data;

public class TodoDbContext : DbContext
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();

    private readonly string _dbPath;

    public TodoDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Status).HasConversion<string>();
            e.HasMany(t => t.Comments).WithOne(c => c.Ticket).HasForeignKey(c => c.TicketId);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
        });
    }
}
