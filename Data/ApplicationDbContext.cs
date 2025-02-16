using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ExpensesPlanner.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpensesPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Expense> Expenses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>()
                .Property(u => u.NormalizedEmail)
                .HasColumnName("NormalizedEmail")
                .HasMaxLength(256);
        }
    }
}
