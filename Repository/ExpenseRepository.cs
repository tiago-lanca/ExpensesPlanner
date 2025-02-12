using ExpensesPlanner.Data;
using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpensesPlanner.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;
        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
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
            return await _context.Expenses.ToListAsync();
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
            return _context.Expenses.Sum(expense => expense.Amount);
        }

        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(Expense expense)
        {
            throw new NotImplementedException();
        }
    }
}
