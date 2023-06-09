namespace OnlineBeratungstermin.Helpers
{
    using Microsoft.EntityFrameworkCore;
    using OnlineBeratungstermin.Models;
    using System;

    public class OnlineTermineDbContext : DbContext
    {
        // DbSet properties for each entity in the model
        public DbSet<Termin> Termine { get; set; }
        public DbSet<Terminart> Terminarten { get; set; }
        public DbSet<Berater> Beraters { get; set; }

        public OnlineTermineDbContext()
        { }

        // Add constructor to accept DbContextOptions
        public OnlineTermineDbContext(DbContextOptions<OnlineTermineDbContext> options) : base(options)
        {
        }

        // Override OnModelCreating to configure the model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entity relationships, constraints, etc.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Seed();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("OnlineTermineDatabase");
            }
        }
    }
}
