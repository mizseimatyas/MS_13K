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
        public async Task<IEnumerable<ItemDto>> AllItems()
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Select(x => new ItemDto
                {
                    categoryId = x.Category.CategoryId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    description = x.Description,
                    price = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Nincs egyetlen item sem");

            return items;
        }
        #endregion

        #region ItemById
        public async Task<ItemDto> ItemById(int id)
        {
            var item = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.ItemId == id)
                .Select(x => new ItemDto
                {
                    categoryId = x.Category.CategoryId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    description = x.Description,
                    price = x.Price
                })
                .FirstOrDefaultAsync();

            if (item is null)
                throw new KeyNotFoundException($"Nincs item '{id}' azonosítóval");

            return item;
        }
        #endregion

        #region ItemsWithQuantity0
        public async Task<IEnumerable<SearchItemsByQuantityDto>> ItemsWithQunatity0()
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Quantity == 0)
                .Select(x => new SearchItemsByQuantityDto
                {
                    id = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    categoryName = x.Category.CategoryName
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Nincs kifogyott termék");

            return items;
        }
        #endregion

        #region Items Quantity Order By Lowest First
        public async Task<IEnumerable<SearchItemsByQuantityDto>> ItemsWithQuantityOrderByAsc()
        {
            return await _context.Items
                .Include(x => x.Category)
                .OrderBy(x => x.Quantity)
                .Select(x => new SearchItemsByQuantityDto
                {
                    id = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    categoryName = x.Category.CategoryName
                })
                .ToListAsync();
        }
        #endregion

        #region Items Quantity Order By Highest First
        public async Task<IEnumerable<SearchItemsByQuantityDto>> ItemsWithQuantityOrderByDesc()
        {
            return await _context.Items
                .Include(x => x.Category)
                .OrderByDescending(x => x.Quantity)
                .Select(x => new SearchItemsByQuantityDto
                {
                    id = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    categoryName = x.Category.CategoryName
                })
                .ToListAsync();
        }
        #endregion

        #region Items Quantity By Category Lowest First
        public async Task<IEnumerable<SearchItemsByQuantityDto>> CategoryItemsQuantityOrderByAsc(string category)
        {
            return await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderBy(x => x.Quantity)
                .Select(x => new SearchItemsByQuantityDto
                {
                    id = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    categoryName = x.Category.CategoryName
                })
                .ToListAsync();
        }
        #endregion

        #region Items Quantity By Category Highest First
        public async Task<IEnumerable<SearchItemsByQuantityDto>> CategoryItemsQuantityOrderByDesc(string category)
        {
            return await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderByDescending(x => x.Quantity)
                .Select(x => new SearchItemsByQuantityDto
                {
                    id = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    categoryName = x.Category.CategoryName
                })
                .ToListAsync();
        }
        #endregion

        #region AddItem
        public async Task AddNewItem(AddNewItemDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.itemName))
                throw new ArgumentException("Nem lehet üres a termék neve", nameof(dto.itemName));

            if (string.IsNullOrWhiteSpace(dto.categoryName))
                throw new ArgumentException("Nem lehet üres a termék kategóriája", nameof(dto.categoryName));

            if (dto.quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.quantity), "Termék mennyisége csak pozitív lehet");

            if (string.IsNullOrWhiteSpace(dto.description))
                throw new ArgumentException("Nem lehet üres a termék leírása", nameof(dto.description));

            if (dto.price <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.price), "Termék ára csak pozitív lehet");

            await using var trx = await _context.Database.BeginTransactionAsync();

            var category = await _context.Categories
                .SingleOrDefaultAsync(x => x.CategoryName.ToLower() == dto.categoryName.ToLower());

            if (category is null)
                throw new KeyNotFoundException($"Nincs '{dto.categoryName}' kategória");

            var nameTaken = await _context.Items
                .AnyAsync(x => x.ItemName.ToLower() == dto.itemName.ToLower());

            if (nameTaken)
                throw new InvalidOperationException($"Már létezik termék ezzel a névvel {dto.itemName}");

            _context.Items.Add(new Item
            {
                ItemName = dto.itemName,
                CategoryId = category.CategoryId,
                Quantity = dto.quantity,
                Description = dto.description,
                Price = dto.price,
            });

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region ModifyItem
        public async Task ModifyItem(ModifyItemDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.itemId <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.itemId), "Termékazonosító csak pozitív lehet, ez nem megfelelő.");

            if (string.IsNullOrWhiteSpace(dto.itemName))
                throw new ArgumentException("Nem lehet üres a termék neve", nameof(dto.itemName));

            if (string.IsNullOrWhiteSpace(dto.categoryName))
                throw new ArgumentException("Nem lehet üres a termék kategóriája", nameof(dto.categoryName));

            if (dto.quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.quantity), "Termék mennyisége csak pozitív lehet");

            if (string.IsNullOrWhiteSpace(dto.description))
                throw new ArgumentException("Nem lehet üres a termék leírása", nameof(dto.description));

            if (dto.price <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.price), "Termék ára csak pozitív lehet");

            await using var trx = await _context.Database.BeginTransactionAsync();

            var item = await _context.Items
                .SingleOrDefaultAsync(x => x.ItemId == dto.itemId);
            if (item is null)
                throw new KeyNotFoundException($"Nincs termék ilyen azonosítóval: {dto.itemId}");

            var itemTaken = await _context.Items
                .AnyAsync(x => x.ItemName.ToLower() == dto.itemName.ToLower() && x.ItemId != dto.itemId);
            if (itemTaken)
                throw new InvalidOperationException($"Már létezik ilyen terméknév: {dto.itemName}");

            var category = await _context.Categories
                .SingleOrDefaultAsync(x => x.CategoryName.ToLower() == dto.categoryName.ToLower());
            if (category is null)
                throw new KeyNotFoundException($"Nincs '{dto.categoryName}' kategória");

            item.ItemName = dto.itemName;
            item.CategoryId = category.CategoryId;
            item.Quantity = dto.quantity;
            item.Description = dto.description;
            item.Price = dto.price;

            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        #region DeleteItem
        public async Task DeleteItem(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Termék id csak pozitív lehet");

            await using var trx = await _context.Database.BeginTransactionAsync();

            var item = await _context.Items
                .SingleOrDefaultAsync(x => x.ItemId == id);
            if (item is null)
                throw new KeyNotFoundException($"Nincs termék ezzel az azonosítóval: {id}");

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        #endregion

        //For users:

        #region ItemByName
        public async Task<ItemDto> ItemByName(string iname)
        {
            var item = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.ItemName.ToLower() == iname.ToLower())
                .Select(x => new ItemDto
                {
                    categoryId = x.Category.CategoryId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    description = x.Description,
                    price = x.Price
                })
                .FirstOrDefaultAsync();

            if (item is null)
                throw new KeyNotFoundException($"Nincs termék erre a keresésre: {iname}");

            return item;
        }
        #endregion

        #region SearchItemByNameSnipet
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByNameSnipet(string sname)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.ItemName.ToLower().Contains(sname.ToLower()))
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék erre a keresésre: {sname}");

            return items;
        }
        #endregion

        #region ItemsByCategoryNameAsc
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByCategoryNameAsc(string category)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderBy(x => x.ItemName)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return items;
        }
        #endregion

        #region ItemsByCategoryNameDesc
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByCategoryNameDesc(string category)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderByDescending(x => x.ItemName)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return items;
        }
        #endregion

        #region ItemsByCategory
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByCategory(string category)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return items;
        }
        #endregion

        #region ItemsByPriceMax
        public async Task<IEnumerable<SearchItemsByPriceDto>> ItemsByPriceMax(int maxp)
        {
            var items = await _context.Items
                .Where(x => x.Price <= maxp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Nem található semmi ekkora összeg alatt");

            return items;
        }
        #endregion

        #region ItemsByPriceMin
        public async Task<IEnumerable<SearchItemsByPriceDto>> ItemsByPriceMin(int minp)
        {
            var items = await _context.Items
                .Where(x => x.Price >= minp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Nem található semmi ekkora összeg felett");

            return items;
        }
        #endregion

        #region ItemsByMaxMinPrice
        public async Task<IEnumerable<SearchItemsByPriceDto>> ItemsByPriceMinMax(int minp, int maxp)
        {
            var items = await _context.Items
                .Where(x => x.Price >= minp && x.Price <= maxp)
                .Select(x => new SearchItemsByPriceDto
                {
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Nem található semmi ebben az árkategóriában");

            return items;
        }
        #endregion

        #region ItemsByCategoryPriceAsc
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByCategoryPriceAsc(string category)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderBy(x => x.Price)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return items;
        }
        #endregion

        #region ItemsByCategoryPriceDesc
        public async Task<IEnumerable<SearchItemsByDto>> ItemsByCategoryPriceDesc(string category)
        {
            var items = await _context.Items
                .Include(x => x.Category)
                .Where(x => x.Category.CategoryName.ToLower() == category.ToLower())
                .OrderByDescending(x => x.Price)
                .Select(x => new SearchItemsByDto
                {
                    categoryNamE = x.Category.CategoryName,
                    itemNamE = x.ItemName,
                    pricE = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException($"Nincs termék '{category}' kategóriában");

            return items;
        }
        #endregion

        #region SearchItemByNameExact(WIP)
        #endregion

        #region SearchByParameter(WIP)
        #endregion

        #region CompareItems(WIP)
        #endregion
    }
}
