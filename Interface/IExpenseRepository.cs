using ExpensesPlanner.Models;

namespace ExpensesPlanner.Interface
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetAllExpenses();
        Task<Expense> GetByIdAsync(int id);
        Task<IEnumerable<Expense>> GetByNameAsync(string name);
        decimal GetTotalAmount();
        bool Add(Expense expense);
        bool Update(Expense expense);
        bool Delete(Expense expense);
        bool Save();
    }
}
