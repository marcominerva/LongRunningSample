using LongRunningSample.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace LongRunningSample.DataAccessLayer;

public partial class ApplicationDbContext : DbContext
{
    public DbSet<Execution> Executions { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Execution>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.JobId).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasConversion<string>();
        });
    }
}
