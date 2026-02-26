using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
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

            var letrehozott = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryName == ujcateg);

            Assert.NotNull(letrehozott);
            Assert.Equal(ujcateg, letrehozott!.CategoryName);
        }

        [Fact]
        public async Task AddNewCategory_ThrowsNameConflict()
        {
            var name = "Laptop";

            _context.Categories.Add(new Category
            {
                CategoryName = name
            });
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.AddNewCategory(name));

            Assert.Contains("Már létezik", exc.Message);

        }
        #endregion

        #region ModifyCategory

        [Fact]
        public async Task ModifyCategory_Correct()
        {
            var categ = new Category
            {
                CategoryName = "regi"
            };
            _context.Categories.Add(categ);

            await _context.SaveChangesAsync();

            var dto = new ModifyCategoryDto
            {
                categId = categ.CategoryId,
                categName = "ujnev"
            };

            await _model.ModifyCategory(dto);

            var modified = await _context.Categories.SingleAsync(X=> X.CategoryId == categ.CategoryId);

            Assert.Equal("ujnev", modified.CategoryName);
        }

        [Fact]
        public async Task ModifyCategory_ThrowsNotFound()
        {
            var dto = new ModifyCategoryDto
            {
                categId = int.MaxValue,
                categName = "teszetlo"
            };

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.ModifyCategory(dto));

            Assert.Contains("Nincs", exc.Message);
        }

        [Fact]
        public async Task ModifyCategory_ThrowsNullDto()
        {
            var exc = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _model.ModifyCategory(null!));

            Assert.Contains("dto", exc.ParamName); //paraméter dto tartalmazza "dto" részletet
        }

        [Fact]
        public async Task ModifyCategory_ThrowsNameTaken()
        {
            var cat1 = new Category { CategoryName = "hat" };
            var cat2 = new Category { CategoryName = "het" };
            _context.Categories.AddRange(cat1, cat2);
            await _context.SaveChangesAsync();

            var dto = new ModifyCategoryDto
            {
                categId = cat2.CategoryId,
                categName = "hat"
            };

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.ModifyCategory(dto));

            Assert.Contains("Már létezik", exc.Message);
        }

        #endregion

        #region DeleteCategory
        [Fact]
        public async Task DeleteCategory_Correct()
        {
            var categ = new Category { CategoryName = "torol" };
            _context.Categories.Add(categ);
            await _context.SaveChangesAsync();

            var before = await _context.Categories.CountAsync();

            await _model.DeleteCategory(categ.CategoryId);

            var after = await _context.Categories.CountAsync();

            Assert.Equal(before - 1, after);

            var deleted = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryId == categ.CategoryId);
            Assert.Null(deleted);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteCategory_ThrowsOutOfRange(int id)
        {
            var exc = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _model.DeleteCategory(id));

            Assert.Contains("pozitív", exc.Message);
        }

        [Fact]
        public async Task DeleteCategory_ThrowsNotFound()
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.DeleteCategory(int.MaxValue));

            Assert.Contains("Nincs", exc.Message);
        }
        #endregion

        #region AllCategories
        [Fact]
        public async Task AllCategories_Correct()
        {
            _context.Categories.AddRange(new Category { CategoryName = "hat" }, new Category { CategoryName = "het"});
            await _context.SaveChangesAsync();

            var result = await _model.AllCategories();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains(result, x => x.categoryName == "hat");
            Assert.Contains(result, x => x.categoryName == "het");

        }

        [Fact]
        public async Task AllCategories_ThrowsEmpty()
        {
            using var empty = DbContextFactory.CreateEmpty();
            var model = new CategoryModel(empty);

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            model.AllCategories());

            Assert.Contains("Nincs", exc.Message);
        }
        #endregion
    }
}
