using ExpensesPlanner.Client.Models;

namespace ExpensesPlanner.Client.Interfaces
{
    public interface IExpenseService
    {
        Task<List<Expense>> GetAllExpenses();
        Task<List<Expense>> GetExpenseByNameAsync(string description);
        Task<Expense> GetExpenseByIdAsync(string id);
        Task<Expense> CreateExpenseAsync(Expense expense);
        Task<HttpResponseMessage> UpdateExpenseAsync(Expense expense);
        Task<HttpResponseMessage> DeleteExpenseAsync(string id);
    }
}
