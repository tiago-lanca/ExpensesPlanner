using ExpensesPlanner.Client.DTO;

namespace ExpensesPlanner.Client.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDetails>> GetAllUsers();
        Task<UserDetails> GetUserByIdAsync(string userId);
        Task<HttpResponseMessage> CreateUserAsync(RegisterUser user);
        Task<HttpResponseMessage> UpdateUserAsync(UserDetails user);
        Task<HttpResponseMessage> DeleteUserAsync(string id);
    }
}
