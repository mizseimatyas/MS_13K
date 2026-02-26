using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class WorkerModelTest
    {
        private readonly WorkerModel _model;
        private readonly DataDbContext _context;

        public WorkerModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new WorkerModel(_context);
        }

        public static class PassHash
        {
            public static string Hash(string password)
            {
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }


        #region Register
        [Fact]
        public async Task WorkerRegistration_Correct()
        {
            var username = "tesztworker";
            var password = "jelszo123";

            var before = await _context.Workers.CountAsync();

            await _model.WorkerRegistration(username, password);

            var after = await _context.Workers.CountAsync();
            Assert.Equal(before + 1, after);

            var created = await _context.Workers.SingleOrDefaultAsync(x => x.WorkerName == username);

            Assert.NotNull(created);
            Assert.Equal(username, created.WorkerName);
            Assert.False(string.IsNullOrWhiteSpace(created.Password));
            Assert.NotEqual(password, created.Password);
            Assert.Equal("Worker", created.Role);
        }

        [Fact]
        public async Task WorkerRegistration_ThrowsAlreadyExists()
        {
            var username = "pista";

            _context.Workers.Add(new Worker
            {
                WorkerName = username,
                Password = "ja",
                Role = "Worker"
            });
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.WorkerRegistration(username, "valami"));

            Assert.Contains("Már", exc.Message);
        }

        #endregion

        #region Login
        [Fact]
        public async Task ValidateWorker_Correct()
        {
            var username = "dolgozo1";
            var password = "jelszo";
            var hash = PassHash.Hash(password);
           
            _context.Workers.Add(new Worker
            {
                WorkerName = username,
                Password = hash
            });

            await _context.SaveChangesAsync();

            var result = await _model.ValidateWorker(username, password);
            
            Assert.NotNull(result);
            Assert.Equal(username, result!.WorkerName);
        }


        [Fact]
        public async Task ValidateWorker_ThrowsWrongPassword()
        {
            var username = "nemjo";
            var hash = PassHash.Hash("jojelszo");

            _context.Workers.Add(new Worker
            {
                WorkerName = username,
                Password = hash
            });

            await _context.SaveChangesAsync();

            var result = await _model.ValidateWorker(username, "nemjojelszo");
            Assert.Null(result);
        }
        #endregion

        #region ChangePassword
        [Fact]
        public async Task ChangePasswrod_Correct()
        {
            var username = "dolgozik";
            var regiJelszo = "regi67";
            var ujJelszo = "uj67";

            var regiHash = PassHash.Hash(regiJelszo);
            var ujHash = PassHash.Hash(ujJelszo);

            var dolgozo = new Worker
            {
                WorkerName = username,
                Password = regiHash,

            };
            _context.Workers.Add(dolgozo);
            await _context.SaveChangesAsync();

            await _model.ChangePassword(dolgozo.WorkerId, ujJelszo);

            var modositott = await _context.Workers.SingleAsync(x=> x.WorkerId == dolgozo.WorkerId);

            Assert.Equal(ujHash, modositott.Password);
            Assert.NotEqual(regiHash, modositott.Password);
        }


        [Fact]
        public async Task ChangePassword_ThrosNotFound()
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ChangePassword(int.MaxValue, "ujjelszo"));

            Assert.Contains("Nincs", exc.Message);
        }

        #endregion
    }
}
