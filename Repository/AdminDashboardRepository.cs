using ExpensesPlanner.Data;
using ExpensesPlanner.Extensions;
using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;

namespace ExpensesPlanner.Repository
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AdminDashboardRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public bool Add(User user)
        {
            _context.Add(user);
            return Save();
        }

        public bool Delete(User user)
        {
            _context.Remove(user);
            return Save();
        }

        public async Task<List<User>> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = _context.Users.Where(user => user.Email == email).FirstOrDefault();
            return user;
        }

        public async Task<User> GetUserById(string id)
        {
            User user = _context.Users.FirstOrDefault(user => user.Id == id);
            return user;
        }

        public async Task<User> GetUserByFirstName(string firstName)
        {
            User user = _context.Users.FirstOrDefault(user => user.FirstName.Contains(firstName));
            return user;
        }

        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool Update(User user)
        {
            _context.Update(user);
            return Save();
        }
    }
}
