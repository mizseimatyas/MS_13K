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

        //validate data
        #region ConfirmData(WIP)
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

            var oldStatus = order.Status;
            order.Status = newStatus;

            if(newStatus == OrderStatus.Delivering && oldStatus == OrderStatus.PaymentSuccess)
            {
                foreach(var item in order.OrderItems)
                {
                    var product = await _context.Items.FindAsync(item.ItemId);
                    if (product != null)
                        product.Quantity -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();
        }

        #endregion

        #region CompleteOrder
        public async Task CompleteOrder(int orderId)
        {
            if(orderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(orderId), "Rendelés azonosító csak pozitív lehet");

            var order = await _context.Orders.Include(x=> x.OrderItems).FirstOrDefaultAsync(y=> y.OrderId == orderId);

            if(order == null)
                throw new KeyNotFoundException($"Nem található rendelés #{orderId} azonosítóval");

            if (order.Status == OrderStatus.OrderCompleted)
                throw new InvalidOperationException("Teljesített rendelés nem törölhető");

            if(order.Status != OrderStatus.Cancelled)
            {
                foreach(var item in order.OrderItems)
                {
                    var product = await _context.Items.FindAsync(item.ItemId);
                    if (product != null)
                        product.Quantity += item.Quantity;
                }
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
