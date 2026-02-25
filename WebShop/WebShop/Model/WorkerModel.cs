using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class WorkerModel
    {
        private readonly DataDbContext _context;
        public WorkerModel(DataDbContext context)
        {
            _context = context;
        }

        public async Task WorkerRegistration(string username, string password, string role = "Worker")
        {
            if (await _context.Workers.AnyAsync(x => x.WorkerName == username))
                throw new InvalidOperationException("Worker already exists");

            await using var trx = await _context.Database.BeginTransactionAsync();

            _context.Workers.Add(new Worker
            {
                WorkerName = username,
                Password = HashPassword(password),
                Role = role
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }

        public async Task<Worker?> ValidateWorker(string username, string password)
        {
            var hash = HashPassword(password);

            return await _context.Workers
                .Where(x => x.WorkerName == username && x.Password == hash)
                .FirstOrDefaultAsync();
        }

        #region Change Password
        public async Task ChangePassword(int workerId, string newPassword)
        {
            await using var trx = await _context.Database.BeginTransactionAsync();

            var worker = await _context.Workers
                .FirstOrDefaultAsync(x => x.WorkerId == workerId);

            if (worker is null)
                throw new KeyNotFoundException($"Nincs dolgozó ezzel az azonosítóval: {workerId}");

            worker.Password = HashPassword(newPassword);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
