using BlockChain.Models;
using Microsoft.EntityFrameworkCore;

namespace BlockChain.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Block> Blocks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Block>()
            .HasIndex(b => b.Index);
    }
}