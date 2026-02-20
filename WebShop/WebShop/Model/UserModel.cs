using System.Security.Cryptography;
using System.Text;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class UserModel
    {
        private readonly DataDbContext _context;
        public UserModel(DataDbContext context)
        {
            _context = context;
        }

        public void Registration(string email, string password, string role = "User")
        {
            if (_context.Users.Any(x => x.Email == email))
            {
                throw new InvalidOperationException("User already exists");
            }
            using var trx = _context.Database.BeginTransaction();
            {
                _context.Users.Add(new User { Email = email, Password = HashPassword(password), Role = role });
                _context.SaveChanges();
                trx.Commit();
            }
        }

        public User? ValidateUser(string email, string password)
        {
            var hash = HashPassword(password);
            var user = _context.Users.Where(x => x.Email == email);
            return user.Where(x => x.Password == hash).FirstOrDefault();
        }

        public void ChangePassword(int userid, string newpassword)
        {
            var trx = _context.Database.BeginTransaction();
            {
                var user = _context.Users.Where(x => x.UserId == userid).First().Password = HashPassword(newpassword);
                _context.SaveChanges();
                trx.Commit();
            }
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
