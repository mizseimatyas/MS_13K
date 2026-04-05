using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Dto;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class ItemModelTest
    {
        private readonly ItemModel _model;
        private readonly DataDbContext _context;

        public ItemModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new ItemModel(_context);
        }


        #region AllItems
        [Fact]
        public async Task AllItems_Correct()
        {
            var result = await _model.AllItems();

            Assert.NotEmpty(result);
            Assert.Contains(result, x => x.itemName == "MacBook Pro M3");
            Assert.Contains(result, x => x.itemName == "RTX 4080 GPU");
            Assert.All(result, x => Assert.False(string.IsNullOrWhiteSpace(x.itemName)));
            Assert.All(result, x => Assert.True(x.price > 0));
        }

        [Fact]
        public async Task AllItems_ThrowsKeyNotFound()
        {
            using var emptydb = DbContextFactory.CreateEmpty();
            var emptymodel = new ItemModel(emptydb);

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => emptymodel.AllItems());
            Assert.Contains("Nincs egyetlen", exc.Message);
        }
        #endregion

        #region ItemById
        [Fact]
        public async Task ItemById_Correct()
        {
            var existingId = _context.Items.First().ItemId;
            var result = await _model.ItemById(existingId);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.itemName));
            Assert.False(string.IsNullOrWhiteSpace(result.categoryName));
            Assert.True(result.price > 0);
        }

        [Fact]
        public async Task ItemById_ThrowsNotFound()
        {
            var notExistId = int.MaxValue;
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ItemById(notExistId));

            Assert.Contains("Nincs", exc.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ItemById_ThrowsNotFound_InvalidId(int id)
        {
            // Model does not range-check id, queries DB and returns null → KeyNotFoundException
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ItemById(id));
            Assert.Contains("Nincs", exc.Message);
        }
        #endregion

        #region AdmItemByName
        [Fact]
        public async Task AdmItemByName_Correct()
        {
            var result = await _model.AdmItemByName("Laptop");

            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Contains("laptop", x.itemName.ToLower()));
        }

        [Fact]
        public async Task AdmItemByName_CaseInsensitive()
        {
            var lower = await _model.AdmItemByName("rtx");
            var upper = await _model.AdmItemByName("RTX");

            Assert.Equal(lower.Count(), upper.Count());
        }

        [Fact]
        public async Task AdmItemByName_ReturnsEmpty_WhenNoMatch()
        {
            // Model returns empty list (not exception) for no match
            var result = await _model.AdmItemByName("kutyakaja_nemletezik_xyz");
            Assert.Empty(result);
        }
        #endregion

        #region AddNewItem
        [Fact]
        public async Task AddNewItem_Correct()
        {
            var before = await _context.Items.CountAsync();
            var categ = _context.Categories.First();
            var dto = new AddNewItemDto
            {
                itemName = "TesztItem",
                categoryName = categ.CategoryName,
                quantity = 67,
                description = "Teszt item leírása",
                price = 6767
            };

            await _model.AddNewItem(dto);

            var after = await _context.Items.CountAsync();
            Assert.Equal(before + 1, after);

            var created = await _context.Items.SingleOrDefaultAsync(x => x.ItemName == dto.itemName);
            Assert.NotNull(created);
            Assert.Equal(categ.CategoryId, created.CategoryId);
            Assert.Equal(dto.quantity, created.Quantity);
            Assert.Equal(dto.description, created.Description);
            Assert.Equal(dto.price, created.Price);
        }

        [Fact]
        public async Task AddNewItem_ThrowsNullDto()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _model.AddNewItem(null!));
        }

        [Fact]
        public async Task AddNewItem_ThrowsCategoryNotFound()
        {
            var dto = new AddNewItemDto
            {
                itemName = "UjTermek",
                categoryName = "nemletezokategoria",
                quantity = 6,
                description = "Teszt leírás",
                price = 676
            };

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.AddNewItem(dto));
            Assert.Contains("Nincs", exc.Message);
            Assert.Contains(dto.categoryName, exc.Message);
        }

        [Fact]
        public async Task AddNewItem_ThrowsNameTaken()
        {
            var existItem = _context.Items.First();
            var categ = _context.Categories.Single(x => x.CategoryId == existItem.CategoryId);
            var dto = new AddNewItemDto
            {
                itemName = existItem.ItemName,
                categoryName = categ.CategoryName,
                quantity = 1,
                description = "leiras",
                price = existItem.Price
            };

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.AddNewItem(dto));
            Assert.Contains("Már létezik", exc.Message);
        }
        #endregion

        #region DeleteItem
        [Fact]
        public async Task DeleteItem_Correct()
        {
            var before = await _context.Items.CountAsync();
            var item = _context.Items.First();

            await _model.DeleteItem(item.ItemId);

            var after = await _context.Items.CountAsync();
            Assert.Equal(before - 1, after);

            var deleted = await _context.Items.SingleOrDefaultAsync(x => x.ItemId == item.ItemId);
            Assert.Null(deleted);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteItem_ThrowsOutOfRange(int id)
        {
            var exc = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.DeleteItem(id));
            Assert.Contains("pozitív", exc.Message);
        }

        [Fact]
        public async Task DeleteItem_ThrowsNotFound()
        {
            var notExistId = int.MaxValue;
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteItem(notExistId));

            Assert.Contains("Nincs termék", exc.Message);
            Assert.Contains(notExistId.ToString(), exc.Message);
        }
        #endregion

        #region ModifyItem
        [Fact]
        public async Task ModifyItem_Correct()
        {
            var item = _context.Items.First();
            var categ = _context.Categories.First();
            var dto = new ModifyItemDto
            {
                itemId = item.ItemId,
                itemName = "Módosított Név",
                categoryName = categ.CategoryName,
                quantity = item.Quantity + 10,
                description = "módosított leírás",
                price = item.Price + 1000
            };

            await _model.ModifyItem(dto);

            var modified = await _context.Items.SingleAsync(x => x.ItemId == item.ItemId);
            Assert.Equal(dto.itemName, modified.ItemName);
            Assert.Equal(categ.CategoryId, modified.CategoryId);
            Assert.Equal(dto.quantity, modified.Quantity);
            Assert.Equal(dto.description, modified.Description);
            Assert.Equal(dto.price, modified.Price);
        }

        [Fact]
        public async Task ModifyItem_ThrowsNullDto()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _model.ModifyItem(null!));
        }

        [Fact]
        public async Task ModifyItem_ThrowsNotFoundItem()
        {
            var dto = new ModifyItemDto
            {
                itemId = int.MaxValue,
                itemName = "nemletezik",
                categoryName = "Számítógépek",
                quantity = 1,
                description = "leírás",
                price = 100
            };

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ModifyItem(dto));
            Assert.Contains("Nincs termék", exc.Message);
            Assert.Contains(dto.itemId.ToString(), exc.Message);
        }

        [Fact]
        public async Task ModifyItem_ThrowsNameTaken()
        {
            var item1 = _context.Items.First();
            var item2 = _context.Items.Skip(1).First();
            var categ = _context.Categories.Single(x => x.CategoryId == item2.CategoryId);
            var dto = new ModifyItemDto
            {
                itemId = item2.ItemId,
                itemName = item1.ItemName,           // existing name → conflict
                categoryName = categ.CategoryName,
                quantity = item2.Quantity,
                description = item2.Description,
                price = item2.Price
            };

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.ModifyItem(dto));
            Assert.Contains("Már létezik", exc.Message);
        }
        #endregion

        #region ItemsByMaxMinPrice
        [Fact]
        public async Task ItemsByMaxMinPrice_Correct()
        {
            var max = 2000000;
            var min = 5000;
            var result = await _model.ItemsByPriceMinMax(min, max);

            Assert.NotEmpty(result);
            Assert.Contains(result, x => x.pricE >= min);
            Assert.Contains(result, x => x.pricE <= max);
        }

        [Fact]
        public async Task ItemsByMaxMinPrice_Empty()
        {
            using var emptydb = DbContextFactory.CreateEmpty();
            var emptymodel = new ItemModel(emptydb);

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => emptymodel.ItemsByPriceMinMax(0, 1000));

            Assert.Contains("Nem található", exc.Message);
        }
        #endregion

    }
}
