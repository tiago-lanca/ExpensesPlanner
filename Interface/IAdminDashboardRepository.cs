using ExpensesPlanner.Models;

namespace ExpensesPlanner.Interface
{
    public interface IAdminDashboardRepository
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByFirstName(string username);
        Task<string> GetUserRole(string id);
        bool Add(User user);
        bool Delete(User user);
        bool Update(User user);
        bool Save();
    }
}
