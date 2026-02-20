using System.Security.Cryptography;
using System.Text;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class AdminModel
    {
        private readonly DataDbContext _context;
        public AdminModel(DataDbContext context)
        {
            _context = context;
        }

        public void AdminRegistration(string username, string password, string role = "Admin")
        {
            if (_context.Admins.Any(x => x.AdminName == username))
            {
                throw new InvalidOperationException("Admin already exists");
            }
            using var trx = _context.Database.BeginTransaction();
            {
                _context.Admins.Add(new Admin { AdminName = username, Password = HashPassword(password), Role = role });
                _context.SaveChanges();
                trx.Commit();
            }
        }

        public Admin? ValidateAdmin(string username, string password)
        {
            var hash = HashPassword(password);
            var admin = _context.Admins.Where(x => x.AdminName == username);
            return admin.Where(x => x.Password == hash).FirstOrDefault();
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
