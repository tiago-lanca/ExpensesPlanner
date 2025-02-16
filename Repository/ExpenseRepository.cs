using ExpensesPlanner.Data;
using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;

namespace ExpensesPlanner.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ExpenseRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool Add(Expense expense)
        {
            _context.Add(expense);
            
            return Save();
        }

        public bool Delete(Expense expense)
        {
            _context.Remove(expense);
            return Save();
        }

        public async Task<IEnumerable<Expense>> GetAllExpenses()
        {
            var currentUser = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (currentUser != null)
            {
                var userExpenses = _context.Expenses.Where(x => x.UserId.ToString() == currentUser.ToString());

                return userExpenses.ToList();
            }

            return null;
        }

        public async Task<Expense> GetByIdAsync(int id)
        {
            return await _context.Expenses.FirstOrDefaultAsync(expense => expense.Id == id);
        }

        public async Task<IEnumerable<Expense>> GetByNameAsync(string description)
        {
            return await _context.Expenses.Where(expense => expense.Description.Contains(description)).ToListAsync();
        }

        public decimal GetTotalAmount()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var userExpenses = _context.Expenses.Where(expense => expense.UserId.ToString() == currentUser.ToString());
            return userExpenses.Sum(expense => expense.Amount);
        }

        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Expense expense)
        {
            _context.Update(expense);
            return Save();
        }
    }
}
