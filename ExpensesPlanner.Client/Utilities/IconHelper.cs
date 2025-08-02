namespace ExpensesPlanner.Client.Utilities
{
    public static class IconHelper
    {
        public static string GetIconForCategory(string category)
        {
            return category switch
            {
                "Food" => "flatware",
                "Transport" => "directions_car",
                "Entertainment" => "movie",
                "Health" => "health_and_safety",
                "Gym" => "exercise",
                "Shopping" => "shopping_bag",
                "Bills" => "receipt",
                "Travel" => "travel",
                "Education" => "school",
                "Investment" => "finance_mode",
                "Savings" => "savings",
                "Other" => "search",
                _ => throw new NotImplementedException()
            };
        }
    }
}
