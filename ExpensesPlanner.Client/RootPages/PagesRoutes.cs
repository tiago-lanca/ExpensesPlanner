﻿namespace ExpensesPlanner.Client.RootPages
{
    public static class PagesRoutes
    {
        // Users
        public const string AllUsers = "/account/users";
        public const string RegisterUser = "/account/register";
        public const string EditUser = "/account/user/edit/{Id}";        

        // Expenses
        public const string AllExpenses = "expenses";
        public const string CreateExpense = "expenses/create";
        public const string EditExpense = "expenses/{Id}";
    }
}
