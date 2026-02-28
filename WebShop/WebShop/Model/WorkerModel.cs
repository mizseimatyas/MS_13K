using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebShop.Dto;
using WebShop.Persistence;
using WebShop.Utils;

namespace WebShop.Model
{
    public class WorkerModel
    {
        private readonly DataDbContext _context;
        public WorkerModel(DataDbContext context)
        {
            _context = context;
        }

        public async Task WorkerRegistration(string username, string password)
        {
            if (await _context.Workers.AnyAsync(x => x.WorkerName == username))
                throw new InvalidOperationException("Már létezik ilyen dolgozónév");

            await using var trx = await _context.Database.BeginTransactionAsync();

            _context.Workers.Add(new Worker
            {
                WorkerName = username,
                Password = PasswordHasher.Hash(password),
                Role = "Worker"
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }

        public async Task<Worker?> ValidateWorker(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nem lehet üres a dolgozó neve", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nem lehet üres a jelszó", nameof(password));

            var hash = PasswordHasher.Hash(password);
            return await _context.Workers
                .FirstOrDefaultAsync(x => x.WorkerName == username && x.Password == hash);
        }

        #region Change Password
        public async Task ChangePassword(int workerId, string newPassword)
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            var worker = await _context.Workers
                .FirstOrDefaultAsync(x => x.WorkerId == workerId);

            if (worker is null)
                throw new KeyNotFoundException($"Nincs dolgozó ezzel az azonosítóval: {workerId}");

            worker.Password = PasswordHasher.Hash(newPassword);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

    }
}
