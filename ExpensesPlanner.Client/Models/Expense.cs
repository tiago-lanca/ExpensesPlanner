using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.Client.Models
{
    public class Expense
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = default!;

        [Required]
        public decimal Amount { get; set; } = 0.0m;

        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [Required]
        public string? Category { get; set; }
        public string? ListExpensesId { get; set; } = string.Empty;



        public static async Task<Expense> CreateExpense(Expense newExpense, string listId, IExpenseService expenseService)
        {
            // Giving the ID of the existing ListExpenses to the new expense
            newExpense.ListExpensesId = listId;

            return await expenseService.CreateExpenseAsync(newExpense);
        }
        
    }
}
