using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class CategoryModel
    {
        private readonly DataDbContext _context;
        public CategoryModel(DataDbContext context)
        {
            _context = context;
        }


        #region AddNewCategory
        public async Task AddNewCategory(string categ)
        {
            if (string.IsNullOrWhiteSpace(categ))
                throw new ArgumentException("Nem lehet üres a kategória neve");

            var verifycat = await _context.Categories.AnyAsync(x=> x.CategoryName.ToLower() == categ.ToLower());
            if (verifycat)
                throw new InvalidOperationException($"Már létezik kategória ezzel a névvel {nameof(categ)}");

            await using var trx = await _context.Database.BeginTransactionAsync();
            _context.Categories.Add(new Category
            {
                CategoryName = categ,
            });
            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region ModifyCategory
        public async Task ModifyNewCategory(ModifyCategoryDto dto)
        {
            if(dto is null)
                throw new ArgumentNullException(nameof(dto));
            if(dto.categId <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.categId), "Kategória azonosító csak pozitív lehet");

            if (string.IsNullOrWhiteSpace(dto.categName))
                throw new ArgumentException("Nem lehet üres a kategória neve");

            await using var trx = await _context.Database.BeginTransactionAsync();

            var category = await _context.Categories.SingleOrDefaultAsync(x=> x.CategoryId == dto.categId);
            if (category is null)
                throw new KeyNotFoundException($"Nincs kategória ezzel az azonosítóval: {dto.categId}");

            var verifycat = await _context.Categories.AnyAsync(x=> x.CategoryName.ToLower() == dto.categName.ToLower() && x.CategoryId != dto.categId);
            if (verifycat)
                throw new InvalidOperationException($"Már léteztik ilyen kategórianév: {dto.categName}");

            category.CategoryName = dto.categName;

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region DeleteCategory
        public async Task DeleteCategory(int categid)
        {
            if (categid <= 0)
                throw new ArgumentOutOfRangeException(nameof(categid), "A kategória azonosoító csak pozitív lehet");

            await using var trx = await _context.Database.BeginTransactionAsync();

            var categ = await _context.Categories.SingleOrDefaultAsync(x => x.CategoryId == categid);
            if (categ is null)
                throw new KeyNotFoundException($"Nincs kategória ezzel az azonosítóval: {categid}");

            _context.Categories.Remove(categ);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region AllCategories
        public IEnumerable<CategoryDto> AllCategories()
        {
            if (!_context.Categories.Any())
                throw new KeyNotFoundException("Nincs egyetlen kategória sem");

            return _context.Categories.Select(x => new CategoryDto
            {
                categoryName = x.CategoryName,
            }).ToList();
        }
        #endregion

        #region ModifyCategorySpecifics(WIP)
        #endregion

    }
}
