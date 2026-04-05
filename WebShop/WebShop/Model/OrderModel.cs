using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class OrderModel
    {
        private readonly DataDbContext _context;
        public OrderModel(DataDbContext context)
        {
            _context = context;
        }

        //For users

        #region PlaceOrder
        public async Task<OrderDto> PlaceOrder(int userId, string targetAddress)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");
            if (string.IsNullOrWhiteSpace(targetAddress))
                throw new ArgumentException("Szállítási cím nem lehet üres", nameof(targetAddress));

            var cartItems = await _context.Carts
                .Include(x => x.Item)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                throw new KeyNotFoundException("Üres a kosár, nem lehet rendelést leadni");

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Item.Quantity < cartItem.Quantity)
                    throw new InvalidOperationException(
                        $"'{cartItem.Item.ItemName}' termékből nincs elegendő készlet " +
                        $"(kért: {cartItem.Quantity}, elérhető: {cartItem.Item.Quantity})");
            }

            var order = new Order
            {
                UserId = userId,
                TargetAddress = targetAddress,
                Date = DateTime.UtcNow,
                Status = OrderStatus.PaymentSuccess,
                TotalPrice = cartItems.Sum(x => x.Quantity * x.Price),
                OrderItems = cartItems.Select(x => new OrderItem
                {
                    ItemId = x.ItemId,
                    ItemName = x.Item.ItemName,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            };

            foreach (var cartItem in cartItems)
                cartItem.Item.Quantity -= cartItem.Quantity;

            _context.Orders.Add(order);
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return new OrderDto
            {
                userId = userId,
                orderId = order.OrderId,
                targetAddress = order.TargetAddress,
                date = order.Date,
                status = order.Status.ToString(),
                totalPrice = order.TotalPrice,
                items = order.OrderItems.Select(x => new OrderItemDto
                {
                    itemId = x.ItemId,
                    itemName = x.ItemName,
                    quantity = x.Quantity,
                    price = x.Price
                }).ToList()
            };
        }
        #endregion

        #region OrderHistoryByUserId
        public async Task<List<OrderAllDto>> OrderHistoryByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");

            var orders = await _context.Orders.Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .Select(x => new OrderAllDto
                {
                    orderId = x.OrderId,
                    date = x.Date,
                    status = x.Status.ToString(),
                    totalPrice = x.TotalPrice,
                    targetAddress = x.TargetAddress
                })
                .ToListAsync();
            if (!orders.Any())
                throw new KeyNotFoundException("Nem volt még rendelésed");
            return orders;
        }

        #endregion

        #region OrderDetailsByUserWithOrderId
        public async Task<OrderDetailsDto> OrderDetailsByUserId(int userId, int orderId)
        {
            if(userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");
            if(orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId), "Rendelés azonosító csak pozitív lehet");

            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userId);

            if (order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{orderId} azonosítóval");

            return new OrderDetailsDto
            {
                orderId = order.OrderId,
                targetAddress = order.TargetAddress,
                date = order.Date,
                status = order.Status.ToString(),
                totalPrice = order.TotalPrice,
                items = order.OrderItems.Select(y => new OrderItemDto
                {
                    itemId = y.ItemId,
                    itemName = y.ItemName,
                    quantity = y.Quantity,
                    price = y.Price
                }).ToList()
            };
        }
        #endregion

        #region CancelOrderByUser
        public async Task CancelOrderByUserWithOrderId(int orderId, int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "Felhasználó azonosító csak pozitív lehet");
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId), "Rendelés azonosító csak pozitív lehet");

            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userId);

            if (order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{orderId} azonosítóval");

            if (order.Status != OrderStatus.PendingPayment)
                throw new InvalidOperationException("Csak függőben levő rendelés törölhető");

            foreach(var item in order.OrderItems)
            {
                var product = await _context.Items.FindAsync(item.ItemId);
                if (product != null)
                    product.Quantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();
        }
        #endregion

        //For workers

        #region AllOrdersNewestFirst
        public async Task<List<OrderAllDto>> GetAllOrders()
        {
            var orders = await _context.Orders.OrderByDescending(x => x.Date).Select(y => new OrderAllDto
            {
                orderId = y.OrderId,
                date = y.Date,
                status = y.Status.ToString(),
                totalPrice = y.TotalPrice,
                targetAddress = y.TargetAddress
            }).ToListAsync();

            if (!orders.Any())
                throw new KeyNotFoundException("Nincs rendelés");
            return orders;
        }
        #endregion

        #region UpdateOrderStatus
        public async Task UpdateOrderStatus(UpdateOrderStatusDto dto)
        {
            if(dto.orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(dto.orderId), "Rendelés azonosító csak pozitív lehet");
            if(string.IsNullOrWhiteSpace(dto.orderStatus))
                throw new ArgumentException("Státusz nem lehet üres", nameof(dto.orderStatus));

            var order = await _context.Orders.FindAsync(dto.orderId);
            if (order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{dto.orderId} azonosítóval");

            if (!Enum.TryParse<OrderStatus>(dto.orderStatus, true, out var newStatus))
                throw new ArgumentException($"Érvénytelen státusz: {dto.orderStatus}");

            var allowedTransitions = new Dictionary<OrderStatus, OrderStatus[]>
                {
                    { OrderStatus.PaymentSuccess, new[] { OrderStatus.Delivering } },
                    { OrderStatus.Delivering, new[] { OrderStatus.OrderCompleted } }
                };

            if (!allowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
                throw new InvalidOperationException(
                    $"Érvénytelen státuszváltás: '{order.Status}' → '{newStatus}'");

            order.Status = newStatus;
            await _context.SaveChangesAsync();
        }
        #endregion

        #region OrderDetailsById
        public async Task<OrderDetailsDto> OrderDetailsByOrderId(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId), "Rendelés azonosító csak pozitív lehet");

            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{orderId} azonosítóval");

            return new OrderDetailsDto
            {
                orderId = order.OrderId,
                targetAddress = order.TargetAddress,
                date = order.Date,
                status = order.Status.ToString(),
                totalPrice = order.TotalPrice,
                items = order.OrderItems.Select(y => new OrderItemDto
                {
                    itemId = y.ItemId,
                    itemName = y.ItemName,
                    quantity = y.Quantity,
                    price = y.Price
                }).ToList()
            };
        }

        #endregion

        #region CompleteOrder
        public async Task CompleteOrder(int orderId)
        {
            if(orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId), "Rendelés azonosító csak pozitív lehet");

            var order = await _context.Orders.FirstOrDefaultAsync(y => y.OrderId == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{orderId} azonosítóval");

            if (order.Status == OrderStatus.OrderCompleted)
                throw new InvalidOperationException("Teljesített rendelés nem törölhető");

            order.Status = OrderStatus.OrderCompleted;
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
