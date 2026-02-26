using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class UserModelTest
    {
        private readonly UserModel _model;
        private readonly DataDbContext _context;

        public UserModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new UserModel(_context);
        }

        #region Hash
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
        #endregion

        #region Register
        [Fact]
        public async Task Registration_Correct()
        {
            var email = "tesztmail@gmail.com";
            var password = "teeszt123";

            var before = await _context.Users.CountAsync();

            await _model.Registration(email, password);

            var after = await _context.Users.CountAsync();
            Assert.Equal(before + 1, after);

            var createdUser = await _context.Users.SingleOrDefaultAsync(x=> x.Email == email);

            Assert.NotNull(createdUser);
            Assert.Equal(email, createdUser.Email);
            Assert.False(string.IsNullOrWhiteSpace(createdUser.Password));
            Assert.NotEqual(password, createdUser.Password);
        }

        [Fact]
        public async Task Registration_ThrowsEmptyEmail()
        {
            var exc = await Assert.ThrowsAsync<ArgumentException>(() => _model.Registration("", "passw"));

            Assert.Contains("Nem lehet", exc.Message);
        }

        [Fact]
        public async Task Registration_ThrowsUserAlreadyExists()
        {
            var email = "letezoemail@gmail.com";

            _context.Users.Add(new User
            {
                Email = email,
                Password = "hash"
            });
            await _context.SaveChangesAsync();
            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.Registration(email, "pwd"));
            Assert.Contains("Már létezik", exc.Message);
        }

        #endregion

        #region Login
        [Fact]
        public async Task ValidateUser_Correct()
        {
            var email = "teszta@gmail.com";
            var password = "jelszo123";
            var hash = PassHash.Hash(password);
            
            _context.Users.Add(new User
            {
                Email = email,
                Password = hash
            });
            await _context.SaveChangesAsync();

            var result = await _model.ValidateUser(email, password);

            Assert.NotNull(result);
            Assert.Equal(email, result!.Email);
        }

        [Fact]
        public async Task ValidateUser_ThrowsWrongPassword()
        {
            var email = "tesztaaa@gmail.com";
            var hash = PassHash.Hash("jelszo");

            _context.Users.Add(new User
            {
                Email = email,
                Password = hash
            });
            await _context.SaveChangesAsync();

            var result = await _model.ValidateUser(email, "nemjo123");

            Assert.Null(result);
        }

        #endregion

        #region ChangePassword
        [Fact]
        public async Task ChangePassword_Correct()
        {
            var email = "valtozottjelszo@gmail.com";
            var regiJelszo = "regi123";
            var ujJelszo = "uj123";
            var regiHash = PassHash.Hash(regiJelszo);
            var ujHash = PassHash.Hash(ujJelszo);

            var user = new User
            {
                Email = email,
                Password = regiHash,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _model.ChangePassword(user.UserId, ujJelszo);

            var valtozott = await _context.Users.SingleAsync(x=> x.UserId == user.UserId);

            Assert.Equal(ujHash, valtozott.Password);
            Assert.NotEqual(regiHash, valtozott.Password);
        }

        [Fact]
        public async Task ChangePasswrod_ThrowsNotFound()
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ChangePassword(int.MaxValue, "ujvalami"));
            Assert.Contains("Nincs felhasználó", exc.Message);
        }

        #endregion

    }
}
