using Microsoft.EntityFrameworkCore;
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

        #region Register New Admin
        public async Task AdminRegistrationAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nem lehet üres az admin neve", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nem lehet üres a jelszó", nameof(password));

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Nem lehet üres a szerepkör", nameof(role));

            if (await _context.Admins.AnyAsync(x => x.AdminName == username))
                throw new InvalidOperationException("Admin already exists");

            await using var trx = await _context.Database.BeginTransactionAsync();

            _context.Admins.Add(new Admin
            {
                AdminName = username,
                Password = HashPassword(password)
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region Validate Admin
        public async Task<Admin?> ValidateAdminAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nem lehet üres az admin neve", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nem lehet üres a jelszó", nameof(password));

            var hash = HashPassword(password);

            return await _context.Admins
                .Where(x => x.AdminName == username && x.Password == hash)
                .FirstOrDefaultAsync();
        }
        #endregion

        #region Change Password
        public async Task ChangePasswordAsync(int adminId, string newPassword)
        {
            if (adminId <= 0)
                throw new ArgumentOutOfRangeException(nameof(adminId), "Admin azonosító csak pozitív lehet");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Nem lehet üres az új jelszó", nameof(newPassword));

            await using var trx = await _context.Database.BeginTransactionAsync();

            var admin = await _context.Admins
                .FirstOrDefaultAsync(x => x.AdminId == adminId);

            if (admin is null)
                throw new KeyNotFoundException($"Nincs admin ezzel az azonosítóval: {adminId}");

            admin.Password = HashPassword(newPassword);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region Encrypt Password
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        #endregion
    }
}