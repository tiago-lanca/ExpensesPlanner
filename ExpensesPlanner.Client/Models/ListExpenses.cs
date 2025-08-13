using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExpensesPlanner.Client.Models
{
    public class ListExpenses
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public List<Expense>? Expenses { get; set; } = new List<Expense>();

        [BsonRequired]
        public string UserId { get; set; } = string.Empty;



        #region Functions

        public static async Task<ListExpenses> CreateListExpenses(ApplicationUser user, ListExpensesService listExpensesService)
        {
            var newList = new ListExpenses
            {
                Expenses = new List<Expense>(),
                UserId = user.Id
            };

            // Creating a new ListExpenses for the user
            return await listExpensesService.CreateListExpensesAsync(newList);
        }

        public async Task<HttpResponseMessage> UpdateListExpenses(Expense newExpense, ListExpensesService listExpensesService)
        {
            Expenses?.Add(newExpense);

            return await listExpensesService.UpdateListExpensesAsync(this);
        }

        #endregion
    }
}
