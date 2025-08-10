using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Models;

namespace ExpensesPlanner.Client.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDetails>> GetAllUsers();
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetApplicationUserByIdAsync(string userId);
        Task<HttpResponseMessage> CreateUserAsync(RegisterUser user);
        Task<HttpResponseMessage> UpdateUserAsync(ApplicationUser user);
        Task<HttpResponseMessage> DeleteUserAsync(string id);
    }
}
