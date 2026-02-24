using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class CartModel
    {
        private readonly DataDbContext _context;
        public CartModel(DataDbContext context)
        {
            _context = context;
        }

        #region CartInventoryByUserId
        public async Task<CartDto> CartInventoryByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");

            var items = await _context.Carts
                .Include(x => x.Item)
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.ItemId)
                .Select(x => new CartItemDto
                {
                    itemId = x.Item.ItemId,
                    itemName = x.Item.ItemName,
                    quantity = x.Quantity,
                    price = x.Price
                })
                .ToListAsync();

            if (items.Count == 0)
                throw new KeyNotFoundException("Üres a kosár");

            return new CartDto
            {
                userId = userId,
                itemList = items
            };
        }
        #endregion

        #region CartInventoryTotalPriceByUserId
        public async Task<int> CartInventoryTotalPrice(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");

            var totalp = await _context.Carts
                .Where(x => x.UserId == userId)
                .SumAsync(x => x.Quantity * x.Price);

            if (totalp == 0)
                throw new KeyNotFoundException("Üres a kosarad");

            return totalp;
        }
        #endregion


        #region ModifyCartItems
        public async Task ModifyCartItems(ModifyCartItemDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.userId), "Felhasználó azonosító csak pozitív lehet");
            if (dto.itemId <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.itemId), "Termék azonosító csak pozitív lehet");
            if (dto.quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(dto.quantity), "Mennyiség nem lehet negatív");

            var cartItem = await _context.Carts
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.UserId == dto.userId && x.ItemId == dto.itemId);

            if (cartItem is null)
                throw new KeyNotFoundException($"Nem található termék ezzel az azonosítóval a kosárban: {dto.itemId}");

            if (dto.quantity == 0)
            {
                _context.Carts.Remove(cartItem);
            }
            else
            {
                if (dto.quantity > cartItem.Item.Quantity)
                    throw new InvalidOperationException("Nincs készleten elegendő mennyiség");

                cartItem.Quantity = dto.quantity;
                cartItem.Price = dto.price > 0 ? dto.price : cartItem.Item.Price;
            }

            await _context.SaveChangesAsync();
        }
        #endregion

        #region ModifyCartStatus(WIP)
        #endregion

    }
}
