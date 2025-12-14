using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PlanerPutovanja.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Trip> Trips { get; set; } = null!;
        public DbSet<TripActivity> Activities { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Trip>()
                .HasMany(t => t.Activities)
                .WithOne(a => a.Trip)
                .HasForeignKey(a => a.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Trip>()
                .HasMany(t => t.Expenses)
                .WithOne(e => e.Trip)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
