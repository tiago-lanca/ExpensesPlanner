using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ExpensesPlanner.Models;

namespace ExpensesPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Expense> Expenses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
    }
}
