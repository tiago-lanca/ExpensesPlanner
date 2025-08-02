using Blazored.LocalStorage;
using DevExpress.Blazor;
using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Threading.Tasks;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class Finances
    {
        [Inject] private CategoryLimitService _categoryLimitService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private AuthService authService { get; set; } = default!;
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        private readonly string dropdownCategoryDefaultMessage = "Select Category";
        private string userId = string.Empty;
        private int sliderValue;
        private bool hasChanges = false;
        private int sliderValuePercentage;
        public  bool isXSmallScreen { get; set; }
        private string? selectedAddCategory = string.Empty;
        private string? progressBarClass = "progressbar-blue";

        private List<string> availableCategories = new();
        private ListExpenses listExpenses = new();
        private List<CategoryLimit> initialCategoryLimitList = new();
        private Dictionary<string, decimal> expensesByCategory = new();
        private List<CategoryLimit> categoryLimitsList = new();

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            selectedAddCategory = dropdownCategoryDefaultMessage;
                                   
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            var user = await authService.GetCurrentUserAsync(token);
            userId = user.Id;

            listExpenses = await _listExpensesService.GetListByUserIdAsync(userId);
            await InitializeCategoryLimits();
            
            GetAvailableCategories();
        }

        private async Task AddCategoryLimit(string category)
        {
            if (category.Equals(dropdownCategoryDefaultMessage))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = "The selected category is invalid.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
                return;
            }            

            categoryLimitsList.Add(new CategoryLimit
            {
                Category = category,
                Limit = 10 // Default limit, can be adjusted later
            });

            await GetExpensesAmountByCategory();

            selectedAddCategory = dropdownCategoryDefaultMessage;
            GetAvailableCategories();

            CheckHasChanges();
        }

        private void RemoveCategoryLimitItem(CategoryLimit categoryLimit)
        {
            categoryLimitsList.Remove(categoryLimit);
            GetAvailableCategories();
            CheckHasChanges();
        }

        private void OnLimitValueChanged(CategoryLimit changedLimit, int value)
        {
            changedLimit.Limit = value;
            CheckHasChanges();
        }

        private async Task UpdateCategoryLimits()
        {
            try
            {
                UserCategoryLimit userCategoryLimit = new UserCategoryLimit
                {
                    UserId = userId,
                    CategoryLimits = categoryLimitsList
                };

                var response = await _categoryLimitService.UpdateUserCategoryLimitsAsync(userCategoryLimit);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Category limits updated successfully.",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });

                    // Refresh data to a new initialCategoryLimitList
                    initialCategoryLimitList = categoryLimitsList
                        .Select(catlimit => new CategoryLimit { Category = catlimit.Category, Limit = catlimit.Limit })
                        .ToList();
                    hasChanges = false;
                }

                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = $"Failed response to update category limits: {errorMessage}",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });
                }
            }

            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed to update category limits: {ex.Message}",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });

                Console.WriteLine($"Error updating category limits: {ex.Message}");
                return;
            }
        }

        private async Task InitializeCategoryLimits()
        {
            var userCategoryLimit = await _categoryLimitService.GetUserCategoryLimitsAsync(userId);
            categoryLimitsList = userCategoryLimit.CategoryLimits ?? new List<CategoryLimit>();

            initialCategoryLimitList = categoryLimitsList
                .Select(catlimit => new CategoryLimit { Category = catlimit.Category, Limit = catlimit.Limit })
                .ToList();

            await GetExpensesAmountByCategory();
        }

        private async Task GetExpensesAmountByCategory()
        {    
            foreach (var category in categoryLimitsList)
            {
                expensesByCategory[category.Category] = listExpenses.Expenses
                    .Where(expense => expense.Category == category.Category)
                    .Sum(expense => expense.Amount);
            }
        }

        private void GetAvailableCategories()
        {
            availableCategories = Enum.GetNames(typeof(ExpenseCategory)).ToList();

            foreach (var category in categoryLimitsList)
            {
                availableCategories.Remove(category.Category);
            }
        }

        private int GetLimitPercentage(int limit)
        {
            return (limit * 100) / 1000;
        }

        private bool CheckHasChanges()
        {
            return hasChanges = !initialCategoryLimitList.SequenceEqual(categoryLimitsList, new CategoryLimitComparer());
        }

        private void OnSelectedCategory(DropDownButtonItemClickEventArgs args)
        {
            selectedAddCategory = args.ItemInfo.Text;
        }

        private bool IsCategoryLimitListEmpty => categoryLimitsList.Count == 0;

    }
}
