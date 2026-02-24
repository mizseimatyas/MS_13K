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
        public async Task AllItems()
        {
            var result = await _model.AllItems();

            Assert.NotEmpty(result);
            Assert.Contains(result, x => x.itemName == "MacBook Pro M3");
            Assert.All(result, x => Assert.False(string.IsNullOrWhiteSpace(x.description)));
        }

        
    }
}