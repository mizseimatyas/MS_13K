using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Dto;
using WebShop.Model;
using WebShop.Persistence;
using Xunit.Abstractions;

namespace ModelTest
{
    public class CartModelTest
    {
        private readonly CartModel _model;
        private readonly DataDbContext _context;

        public CartModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new CartModel(_context);
        }

        #region CartInventory
        [Fact]
        public async Task CartInventoryByUserId_Correct()
        {
            var user = new User { Email = "teszt@gmail.com", Password = "password" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "teszt" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item1 = new Item
            {
                ItemName = "termek1",
                Quantity = 5,
                Price = 67000,
                Description = "teszt",
                CategoryId = cat.CategoryId
            };
            var item2 = new Item
            {
                ItemName = "termek2",
                Quantity = 6,
                Price = 57000,
                Description = "teszt2",
                CategoryId = cat.CategoryId
            };

            _context.Items.AddRange(item1, item2);
            await _context.SaveChangesAsync();

            _context.Carts.AddRange(
                new Cart { UserId = user.UserId, ItemId = item1.ItemId, Quantity = 2, Price = item1.Price },
                new Cart { UserId = user.UserId, ItemId = item2.ItemId, Quantity = 1, Price = item2.Price }
                );
            await _context.SaveChangesAsync();

            var result = await _model.CartInventoryByUserId(user.UserId);

            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.userId);
            Assert.Equal(2, result.itemList.Count);
            Assert.Equal(item1.ItemId, result.itemList[0].itemId);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CartInventoryByUserId_ThrowsOutOfRange(int id)
        {
            var exc = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.CartInventoryByUserId(id));
            Assert.Contains("pozitív", exc.Message);

        }

        [Fact]
        public async Task CartInventoryByUserId_ThrowsCartEmpty()
        {
            var user = new User { Email = "teszt2@gmail.com", Password = "password2" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.CartInventoryByUserId(user.UserId));

            Assert.Contains("Üres", exc.Message);

        }
        #endregion

        #region CartInventoryTotalPrice
        [Fact]
        public async Task CartInventoryTotalPrice_Correct()
        {
            var user = new User { Email = "teszt@gmail.com", Password = "password" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "teszt" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item1 = new Item
            {
                ItemName = "termek1",
                Quantity = 1,
                Price = 1000,
                Description = "teszt",
                CategoryId = cat.CategoryId
            };
            var item2 = new Item
            {
                ItemName = "termek2",
                Quantity = 2,
                Price = 1000,
                Description = "teszt2",
                CategoryId = cat.CategoryId
            };

            _context.Items.AddRange(item1, item2);
            await _context.SaveChangesAsync();

            _context.Carts.AddRange(
                new Cart { UserId = user.UserId, ItemId = item1.ItemId, Quantity = 2, Price = item1.Price },
                new Cart { UserId = user.UserId, ItemId = item2.ItemId, Quantity = 1, Price = item2.Price }
                );
            await _context.SaveChangesAsync();

            var totalp = await _model.CartInventoryTotalPrice(user.UserId);

            Assert.Equal(3000, totalp);

        }

        [Fact]
        public async Task CartInventoryTotalPrice_ThrowsEmpty()
        {
            var user = new User { Email = "teszt@gmail.com", Password = "password" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _model.CartInventoryTotalPrice(user.UserId));
            Assert.Contains("Üres", exc.Message);
        }


        #endregion

        #region ModifyCartItems
        [Fact]
        public async Task ModifyCartItems_Correct()
        {
            var user = new User { Email = "teszta@gmail.com", Password = "pwd" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "teszt" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item = new Item
            {
                ItemName = "moditem",
                Quantity = 10,
                Price = 1000,
                Description = "leiras",
                CategoryId = cat.CategoryId
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.UserId,
                ItemId = item.ItemId,
                Quantity = 1,
                Price = item.Price
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var dto = new ModifyCartItemDto
            {
                userId = user.UserId,
                itemId = item.ItemId,
                quantity = 5,
                price = 1500
            };

            await _model.ModifyCartItems(dto);

            var modified = await _context.Carts
                .Include(x => x.Item)
                .SingleAsync(x => x.UserId == user.UserId && x.ItemId == item.ItemId);

            Assert.Equal(5, modified.Quantity);
            Assert.Equal(1500, modified.Price);
        }

        [Fact]
        public async Task ModifyCartItems_RemoveQuantityZero()
        {
            var user = new User { Email = "tesz@gmail.com", Password = "pwd" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "teszt" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item = new Item
            {
                ItemName = "moditem",
                Quantity = 10,
                Price = 1000,
                Description = "leiras",
                CategoryId = cat.CategoryId
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.UserId,
                ItemId = item.ItemId,
                Quantity = 2,
                Price = item.Price
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var dto = new ModifyCartItemDto
            {
                userId = user.UserId,
                itemId = item.ItemId,
                quantity = 0,
                price = 0
            };

            await _model.ModifyCartItems(dto);

            var deleted = await _context.Carts.SingleOrDefaultAsync(x => x.UserId == user.UserId && x.ItemId == item.ItemId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task ModifyCartItems_ThrowsNotFound()
        {
            var dto = new ModifyCartItemDto
            {
                userId = 200,
                itemId = 1,
                quantity = 1,
                price = 100
            };

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ModifyCartItems(dto));
            Assert.Contains("Nem található", exc.Message);
        }

        [Fact]
        public async Task ModifyCartItems_ThrowsNotEnough()
        {
            var user = new User { Email = "tesz@gmail.com", Password = "pwd" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "teszt" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item = new Item
            {
                ItemName = "moditem",
                Quantity = 3,
                Price = 1000,
                Description = "leiras",
                CategoryId = cat.CategoryId
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.UserId,
                ItemId = item.ItemId,
                Quantity = 2,
                Price = item.Price
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var dto = new ModifyCartItemDto
            {
                userId = user.UserId,
                itemId = item.ItemId,
                quantity = 10,
                price = 0
            };

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.ModifyCartItems(dto));
            Assert.Contains("Nincs készleten", exc.Message);
        }
        #endregion
    }
}
