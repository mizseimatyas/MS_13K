using Microsoft.EntityFrameworkCore;
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

        public async Task Registration(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Nem lehet üres az email", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nem lehet üres a jelszó", nameof(password));

            if (await _context.Users.AnyAsync(x => x.Email == email))
                throw new InvalidOperationException("Már létezik felhasználó ezzel az emailel");

            await using var trx = await _context.Database.BeginTransactionAsync();

            _context.Users.Add(new User
            {
                Email = email,
                Password = HashPassword(password),
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }

        public async Task<User?> ValidateUser(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Nem lehet üres az email", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nem lehet üres a jelszó", nameof(password));

            var hash = HashPassword(password);

            return await _context.Users
                .Where(x => x.Email == email && x.Password == hash)
                .FirstOrDefaultAsync();
        }

        public async Task ChangePassword(int userid, string newpassword)
        {
            if (userid <= 0)
                throw new ArgumentOutOfRangeException(nameof(userid), "Felhasználó azonosító csak pozitív lehet");

            if (string.IsNullOrWhiteSpace(newpassword))
                throw new ArgumentException("Nem lehet üres az új jelszó", nameof(newpassword));

            await using var trx = await _context.Database.BeginTransactionAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userid);

            if (user is null)
                throw new KeyNotFoundException($"Nincs felhasználó ezzel az azonosítóval: {userid}");

            user.Password = HashPassword(newpassword);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
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