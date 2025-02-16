using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;

namespace ExpensesPlanner.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        // Get user ID on HTTPContextAccessor 
        public static string GetUserID(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}
