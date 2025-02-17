using ExpensesPlanner.Data;
using ExpensesPlanner.Extensions;
using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExpensesPlanner.Repository
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        public AdminDashboardRepository(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
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

        public async Task<string> GetUserRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return "No Roles";

            var roleName = await _userManager.GetRolesAsync(user);
            return roleName.FirstOrDefault("No Role");
            
        }
    }
}
