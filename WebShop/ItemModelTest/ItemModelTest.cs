using Microsoft.EntityFrameworkCore.Internal;
using ModelTest;
using WebShop.Dto;
using WebShop.Model;
using WebShop.Persistence;

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


        [Fact]
        public void AllItems()
        {
            var result = _model.AllItems();

            Assert.NotEmpty(result);
            Assert.Contains(result, x => x.itemName == "MacBook Pro M3");
            Assert.All(result, x => Assert.False(string.IsNullOrWhiteSpace(x.description)));
        }

        [Fact]
        public void AllItemsEmpty()
        {
            using var empty = DbContextFactory.CreateEmpty();
            var model = new ItemModel(empty);

            var exception = Assert.Throws<KeyNotFoundException>(() => model.AllItems());
            Assert.Contains("Nincs egyetlen ", exception.Message);
        }

        [Fact]
        public void ItemsByName()
        {
            var result = _model.ItemByName("RTX");
            Assert.Contains("GPU", result.itemName); //benne van a nevebe
            Assert.Equal(3, result.categoryId); //van kategoriaja
            Assert.False(string.IsNullOrWhiteSpace(result.description));  //nem ures a leiras
            Assert.NotEqual(0, result.price); //van ára
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void ItemsByIdEmpty(int id)
        {
            Assert.Throws<KeyNotFoundException>(() => _model.ItemById(id));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ItemByNameEmpty(string name)
        {
            Assert.Throws<KeyNotFoundException>(() => _model.ItemByName(name));
        }


        [Fact]
        public async Task AddNewItem()
        {
            var before_count = _context.Items.Count();
            var dto = new AddNewItemDto
            {
                itemName = "TesztName",
                categoryName = "TesztCat",
                quantity = 67,
                description = "teszt",
                price = 67,
            };
            await _model.AddNewItem(dto);
            Assert.Equal(before_count + 1, _context.Items.Count());
            Assert.True(_context.Items.Any(x => x.ItemName == dto.itemName));
        }
    }
}