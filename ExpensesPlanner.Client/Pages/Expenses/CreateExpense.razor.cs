using Blazored.LocalStorage;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using System.IdentityModel.Tokens.Jwt;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class CreateExpense
    {
        private Expense newExpense { get; set; } = new();
        private RadzenTemplateForm<Expense> form;
        [Inject] private IExpenseService _expenseService { get; set; } = default!;
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private IUserService _userService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        private string Id = string.Empty;
        private bool busy;


        protected override async Task OnInitializedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (token is null) { Navigation.NavigateTo("/"); return; }


            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                Id = jwtToken.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier")).Value;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error parsing token: " + ex.Message);
            }
        }
        private async Task SubmitForm()
        {
            try
            {
                busy = true;

                var user = await _userService.GetUserByIdAsync(Id); // Replace with actual session user ID

                // Check if the user has an existing List of Expenses, otherwise creates a new one
                if (user.IsListExpensesEmpty())
                {
                    var newList = await ListExpenses.CreateListExpenses(user, _listExpensesService);

                    if(newList != null)
                    {
                        user.ListExpensesId = newList.Id;
                        // Update the user with the new ListExpensesId
                        await _userService.UpdateUserAsync(user);
                    }
                    else throw new Exception("Failed to create a new ListExpenses for the user.");
                }

                // Creating the expense
                newExpense = await Expense.CreateExpense(newExpense, user.ListExpensesId, _expenseService);

                // Getting the user's ListExpenses by ID
                var userListExpenses = await _listExpensesService.GetListByIdAsync(user.ListExpensesId);

                // Adding the new expense to the user's ListExpenses and Updating it
                var response = await userListExpenses.UpdateListExpenses(newExpense, _listExpensesService);

                if (response.IsSuccessStatusCode)
                {
                    await Task.Delay(2000);
                    busy = false;
                    Navigation.NavigateTo("/expenses");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error creating expense: " + errorMessage);
                    busy = false;
                }
                
            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception error: " + ex.Message);
            }
        }

        private async Task ValidateAndSubmit()
        {
            if (form.EditContext.Validate())
            {
                await form.Submit.InvokeAsync(null);
            }
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/expenses");
        }

    }
}
