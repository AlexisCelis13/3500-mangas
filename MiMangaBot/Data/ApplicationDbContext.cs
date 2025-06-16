using Microsoft.EntityFrameworkCore;
using MiMangaBot.Domain.Models;

namespace MiMangaBot.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Manga> Mangas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Manga>()
            .HasIndex(m => m.Titulo)
            .IsUnique();
    }
} 
