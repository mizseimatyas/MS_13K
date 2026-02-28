using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebShop.Model;
using WebShop.Persistence;
using WebShop.Utils;

namespace ModelTest
{
    public class AdminModelTest
    {
        private readonly AdminModel _model;
        private readonly DataDbContext _context;

        public AdminModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new AdminModel(_context);
        }

        #region Register
        [Fact]
        public async Task AdminRegistration_Correct()
        {
            var username = "tesztadmin";
            var password = "jelszo123";

            var before = await _context.Admins.CountAsync();

            await _model.AdminRegistration(username, password);

            var after = await _context.Admins.CountAsync();
            Assert.Equal(before + 1, after);

            var created = await _context.Admins.SingleOrDefaultAsync(x => x.AdminName == username);

            Assert.NotNull(created);
            Assert.Equal(username, created!.AdminName);
            Assert.False(string.IsNullOrWhiteSpace(created.Password));
            Assert.NotEqual(password, created.Password);
        }

        [Fact]
        public async Task AdminRegistration_ThrowsEmptyName()
        {
            var exc = await Assert.ThrowsAsync<ArgumentException>(
                () => _model.AdminRegistration("", "valami"));

            Assert.Contains("Nem lehet", exc.Message);
        }

        [Fact]
        public async Task AdminRegistration_ThrowsAlreadyExists()
        {
            var username = "letezo";

            _context.Admins.Add(new Admin
            {
                AdminName = username,
                Password = "hash"
            });
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _model.AdminRegistration(username, "valami"));

            Assert.Contains("Már", exc.Message);
        }

        #endregion

        #region Login
        [Fact]
        public async Task ValidateAdmin_Correct()
        {
            var username = "admina";
            var password = "jelszo";
            var hash = PasswordHasher.Hash(password);

            _context.Admins.Add(new Admin
            {
                AdminName = username,
                Password = hash
            });
            await _context.SaveChangesAsync();

            var result = await _model.ValidateAdmin(username, password);

            Assert.NotNull(result);
            Assert.Equal(username, result!.AdminName);
        }

        [Fact]
        public async Task ValidateAdmin_WrongPassword_ReturnsNull()
        {
            var username = "rosszadmin";
            var hash = PasswordHasher.Hash("helyesjelszo");

            _context.Admins.Add(new Admin
            {
                AdminName = username,
                Password = hash
            });
            await _context.SaveChangesAsync();

            var result = await _model.ValidateAdmin(username, "hibasjelszo");

            Assert.Null(result);
        }

        #endregion

        #region ChangePasswrod
        [Fact]
        public async Task ChangePassword_Correct()
        {
            var username = "adminchange";
            var regiJelszo = "regi123";
            var ujJelszo = "uj123";

            var regiHash = PasswordHasher.Hash(regiJelszo);
            var ujHash = PasswordHasher.Hash(ujJelszo);

            var admin = new Admin
            {
                AdminName = username,
                Password = regiHash
            };
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            await _model.ChangePassword(admin.AdminId, ujJelszo);

            var modositott = await _context.Admins.SingleAsync(x => x.AdminId == admin.AdminId);

            Assert.Equal(ujHash, modositott.Password);
            Assert.NotEqual(regiHash, modositott.Password);
        }

        [Fact]
        public async Task ChangePassword_ThrowsNotFound()
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _model.ChangePassword(int.MaxValue, "ujjelszo"));

            Assert.Contains("Nincs", exc.Message);
        }
        #endregion
    }
}
