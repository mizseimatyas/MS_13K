using System.Security.Cryptography;
using System.Text;
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

        public void WorkerRegistration(string username, string password, string role = "Worker")
        {
            if (_context.Workers.Any(x => x.WorkerName == username))
            {
                throw new InvalidOperationException("Worker already exists");
            }
            using var trx = _context.Database.BeginTransaction();
            {
                _context.Workers.Add(new Worker { WorkerName = username, Password = HashPassword(password), Role = role });
                _context.SaveChanges();
                trx.Commit();
            }
        }

        public Worker? ValidateWorker(string username, string password)
        {
            var hash = HashPassword(password);
            var worker = _context.Workers.Where(x => x.WorkerName == username);
            return worker.Where(x => x.Password == hash).FirstOrDefault();
        }

        #region Change Password

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
