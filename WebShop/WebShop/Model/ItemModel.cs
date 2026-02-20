using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class ItemModel
    {
        private readonly DataDbContext _context;
        public ItemModel(DataDbContext context)
        {
            _context = context;
        }

        //For workers
        #region AllItems
        public IEnumerable<ItemDto> AllItems()
        {
            if (!_context.Items.Any())
                throw new KeyNotFoundException("Nincs egyetlen item sem");

            return _context.Items.Include(x => x.Category)
                .Select(x => new ItemDto
                {
                    categoryId = x.Category.CategoryId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    description = x.Description,
                    price = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemById
        public ItemDto ItemById(int id)
        {
            if (!_context.Items.Any(x => x.ItemId == id))
                throw new KeyNotFoundException($"Nincs item '{id}' azonosítóval");
            return _context.Items.Include(x => x.Category)
                .Where(x => x.ItemId == id)
                .Select(x => new ItemDto
                {
                    categoryId = x.Category.CategoryId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    description = x.Description,
                    price = x.Price
                })
                .First();
        }

        #endregion

        #region ItemsByQuantity0
        #endregion

        #region ItemsByQuantity
        #endregion

        #region ModifyItemById
        #endregion

        //For users:
        #region ItemsByCategory
        public IEnumerable<SearchItemsByDto> ItemsByCategory(string category)
        {
            if (!_context.Items.Any())
                throw new KeyNotFoundException("Nincs egyetlen item sem");
            return _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemsByPriceMax
        public IEnumerable<SearchItemsByPriceDto> ItemsByPriceMax(int maxp)
        {
            if (!_context.Items.Any(x => x.Price <= maxp))
                throw new KeyNotFoundException("Nem található semmi ekkora összeg alatt");

            return _context.Items.Where(x => x.Price <= maxp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemsByPriceMin
        public IEnumerable<SearchItemsByPriceDto> ItemsByPriceMin(int minp)
        {
            if (!_context.Items.Any(x => x.Price >= minp))
                throw new KeyNotFoundException("Nem található semmi ekkora összeg felett");

            return _context.Items.Where(x => x.Price >= minp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemsByMaxMinPrice
        public IEnumerable<SearchItemsByPriceDto> ItemsByPriceMinMax(int minp, int maxp)
        {
            if (!_context.Items.Any(x => x.Price >= minp && x.Price <= maxp))
                throw new KeyNotFoundException("Nem található semmi ebben az árkategóriában");

            return _context.Items.Where(x => x.Price >= minp && x.Price <= maxp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemsByCategoryPriceAsc
        public IEnumerable<SearchItemsByDto> ItemsByCategoryPriceAsc(string category)
        {
            if (!_context.Items.Include(x => x.Category)
                .Any(x => x.Category.CategoryName.ToLower() == category.ToLower()))
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return _context.Items.Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderBy(x => x.Price)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region ItemsByCategoryPriceDesc
        public IEnumerable<SearchItemsByDto> ItemsByCategoryPriceDesc(string category)
        {
            if (!_context.Items.Include(x => x.Category)
                .Any(x => x.Category.CategoryName.ToLower() == category.ToLower()))
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return _context.Items.Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderByDescending(x => x.Price)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region SearchItemByNameExact(WIP)
        #endregion

        #region SearchItemByNameSnipet
        public IEnumerable<SearchItemsByDto> ItemsByName(string sname)
        {
            if (!_context.Items.Any(x => x.ItemName.ToLower().Contains(sname.ToLower())))
                throw new KeyNotFoundException($"Nincs termék erre a keresésre: {sname}");

            return _context.Items.Include(x => x.Category)
                .Where(x => x.ItemName.ToLower().Contains(sname.ToLower()))
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToList();
        }
        #endregion

        #region SearchByParameter(WIP)
        #endregion

        #region CompareItems(WIP)
        #endregion
    }
}
