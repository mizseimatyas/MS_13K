using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ModelTest;
using WebShop.Dto;
using WebShop.Model;
using WebShop.Persistence;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ItemModelTest
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
            Assert.Contains(result, x => x.itemName == "MacBook Pro M3");       //név = név
            Assert.Contains(result, x => x.itemName == "Laptop Táska 15\"");    //talál ilyet
            Assert.Contains(result, x => x.itemName == "RTX 4080 GPU");         //létezik
        }

        [Fact]
        public async Task AllItemsNoWhiteSpace()
        {
            var result = await _model.AllItems();
            var first = result.First();

            Assert.True(first.categoryId > 0);                                  //categoryid nem negativ
            Assert.False(string.IsNullOrWhiteSpace(first.itemName));            //nem üres név
            Assert.False(string.IsNullOrWhiteSpace(first.description));         //nem üres leírás
            Assert.True(first.price > 0);
        }

        [Fact]
        public async Task AllItems_ThrowsKeyNotFound()
        {
            using var emptydb = DbContextFactory.CreateEmpty();                 //empty db
            var emptymodel = new ItemModel(emptydb);                            //modelhez db

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => emptymodel.AllItems()); //hibauzenet 


            Assert.Contains("Nincs egyetlen", exc.Message);                     //hibauzenet egyezik a modelbe
        }
        #endregion

        #region ItemById
        [Fact]
        public async Task ItemById_Correct()
        {
            var idexist = _context.Items.First().ItemId;
            var result = await _model.ItemById(idexist);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.itemName));
            Assert.True(result.categoryId > 0);
            Assert.True(result.price > 0);
        }

        [Fact]
        public async Task ItemById_ThrowsNotFound()
        {
            var notexistId = int.MaxValue;
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ItemById(notexistId));

            Assert.Contains("Nincs", exc.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ItemById_ThrowsInvalid(int id)
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ItemById(id));

            Assert.Contains("Nincs", exc.Message);
        }
        #endregion

        #region ItemsByCategory
        [Fact]
        public async Task ItemsByCategory_Correct()
        {
            var catexist = _context.Categories.First().CategoryName;
            var result = await _model.ItemsByCategory(catexist);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.Equal(catexist.ToLower(), x.categoryNamE.ToLower()));
        }

        #region NagyKisBetuk
        #endregion

        [Fact]
        public async Task ItemsById_ThrowsNotFound()
        {
            var notexistCat = "kutyakaja";
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ItemsByCategory(notexistCat));

            Assert.Contains("Nincs", exc.Message);
            Assert.Contains(notexistCat, exc.Message);
        }

        [Fact]
        public async Task ItemsByCategory_Empty()
        {
            using var emptydb = DbContextFactory.CreateEmpty();
            var emptymodel = new ItemModel(emptydb);

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => emptymodel.ItemsByCategory("kutyakaja"));
            Assert.Contains("Nincs", exc.Message);
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
                description = "Teszt item",
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
                itemName = "KutyaKaja",
                categoryName = "kutyakaja",
                quantity = 6,
                description = "Teszt item",
                price = 676
            };
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.AddNewItem(dto));

            Assert.Contains("Nincs", exc.Message);
            Assert.Contains(dto.categoryName, exc.Message);
        }

        [Fact]
        public async Task AddNewItem_ThrowsNameTaken()
        {
            var existitem = _context.Items.First();
            var categ = _context.Categories.Single(x=> x.CategoryId == existitem.CategoryId);
            var dto = new AddNewItemDto
            {
                itemName = existitem.ItemName,
                categoryName = categ.CategoryName,
                quantity = 1,
                description = "leiras",
                price = existitem.Price
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
            var notexistId = int.MaxValue;

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteItem(notexistId));

            Assert.Contains("Nincs termék", exc.Message);
            Assert.Contains(notexistId.ToString(), exc.Message);
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
                itemName = "modositott nev",
                categoryName = categ.CategoryName,
                quantity = item.Quantity + 10,
                description = "modositott leiras",
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
                itemName = "rago",
                categoryName = "cukor",
                quantity = 1,
                description = "mentolos",
                price = 100
            };

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ModifyItem(dto));

            Assert.Contains("Nincs termék", exc.Message);
            Assert.Contains(dto.itemId.ToString(), exc.Message);
        }

        [Fact]
        public async Task ModifyItem_ThrowsNameTaken()
        {
            var item1 = _context.Items.First();                                     // item1 seedbol
            var item2 = _context.Items.Skip(1).First();                             // item2 seedbol
            var categ = _context.Categories.Single(x => x.CategoryId == item2.CategoryId);

            var dto = new ModifyItemDto
            {
                itemId = item2.ItemId,
                itemName = item1.ItemName,                                          //létezo nev
                categoryName = categ.CategoryName,
                quantity = item2.Quantity,
                description = item2.Description,
                price = item2.Price
            };

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.ModifyItem(dto));

            Assert.Contains("Már létezik", exc.Message);
        }
        #endregion


    }
}