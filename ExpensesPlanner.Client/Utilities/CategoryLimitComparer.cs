using ExpensesPlanner.Client.Models;
using System.Diagnostics.CodeAnalysis;

namespace ExpensesPlanner.Client.Utilities
{
    public class CategoryLimitComparer : IEqualityComparer<CategoryLimit>
    {
        public bool Equals(CategoryLimit? x, CategoryLimit? y)
        {
            return x?.Category == y?.Category && x?.Limit == y?.Limit;
        }

        public int GetHashCode([DisallowNull] CategoryLimit obj)
        {
            return HashCode.Combine(obj.Category, obj.Limit);
        }
    }
}
