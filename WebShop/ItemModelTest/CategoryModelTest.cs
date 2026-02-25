using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class CategoryModelTest
    {
        private readonly CategoryModel _model;
        private readonly DataDbContext _context;
        
        public CategoryModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new CategoryModel(_context);
        }

        #region AddNewCategory
        [Fact]
        public async Task AddNewCategory_Correct()
        {
            var before = await _context.Categories.CountAsync();

            var ujcateg = "ujteszt";
                
            await _model.AddNewCategory(ujcateg);

            var after = await _context.Categories.CountAsync();
            Assert.Equal(before + 1, after);

            var created = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryName == ujcateg);

            Assert.NotNull(created);
        }

        [Fact]
        public async Task AddNewCategory_ThrowsNameConflict()
        {
            var existcateg = _context.Categories.First();
            var categ = _context.Categories.Single(x => x.CategoryId == existcateg.CategoryId);

            var name = existcateg.CategoryName;


            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.AddNewCategory(name));
            Assert.Contains("Már létezik", exc.Message);

        }



        #endregion



    }
}
