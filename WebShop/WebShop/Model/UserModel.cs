using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebShop.Dto;
using WebShop.Persistence;
using WebShop.Utils;

namespace WebShop.Model
{
    public class UserModel
    {
        private readonly DataDbContext _context;
        public UserModel(DataDbContext context)
        {
            _context = context;
        }

        public async Task Registration(string email, string password, string? address, string? phone)
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
                Password = PasswordHasher.Hash(password),
                Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim()
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

            var hash = PasswordHasher.Hash(password);

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

            user.Password = PasswordHasher.Hash(newpassword);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }

        public async Task<UserDto> GetMe(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var user = await _context.Users
                .Where(x => x.UserId == userId)
                .Select(x => new UserDto
                {
                    userid = x.UserId,
                    email = x.Email,
                    address = x.Address ?? "",
                    phone = x.Phone ?? ""
                })
                .FirstOrDefaultAsync();

            if (user is null)
                throw new KeyNotFoundException("Nincs ilyen felhasználó");

            return user;
        }
    }
}