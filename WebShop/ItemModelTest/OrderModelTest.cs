using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Dto;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class OrderModelTest
    {
        private readonly OrderModel _model;
        private readonly DataDbContext _context;

        public OrderModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new OrderModel(_context);
        }

        #region Helper
        private async Task<(User user, Item item, Order order)> SeederAsync(
            OrderStatus status = OrderStatus.PendingPayment,
            int quantity = 2,
            int price = 1000)
        {
            var user = new User { Email = "teszta@gmail.com", Password = "pwd" };
            _context.Users.Add(user);

            var cat = new Category { CategoryName = "OrderCat" };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();

            var item = new Item
            {
                ItemName = "OrderItem",
                Quantity = 10,
                Price = price,
                Description = "leiras",
                CategoryId = cat.CategoryId
            };
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            var order = new Order
            {
                UserId = user.UserId,
                TargetAddress = "cim 1",
                Date = DateTime.UtcNow,
                Status = status,
                TotalPrice = quantity * price,
                OrderItems = new List<OrderItem> { new OrderItem
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Quantity = quantity,
                    Price = price,
                }
                }
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return (user, item, order);
        }
        #endregion

        #region OrderHistory
        [Fact]
        public async Task OrderHistoryByUserId_Correct()
        {
            var (user, _, order) = await SeederAsync();
            var result = await _model.OrderHistoryByUserId(user.UserId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(order.OrderId, result[0].orderId);
            Assert.Equal(order.TotalPrice, result[0].totalPrice);
        }

        [Fact]
        public async Task OrderHistoryByUserId_ThrowsEmpty()
        {
            var user = new User { Email = "noorder@gmail.com", Password = "pwd" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => _model.OrderHistoryByUserId(user.UserId));

            Assert.Contains("Nem volt", exc.Message);
        }
        #endregion

        #region OrderDetailsByUserId
        [Fact]
        public async Task OrderDetailsByUserId_Correct()
        {
            var (user, item, order) = await SeederAsync();
            var result = await _model.OrderDetailsByUserId(user.UserId, order.OrderId);

            Assert.NotNull(result);
            Assert.Equal(order.OrderId, result.orderId);
            Assert.Equal(order.TotalPrice, result.totalPrice);
            Assert.Single(result.items);
            Assert.Equal(item.ItemId, result.items[0].itemId);
            Assert.Equal(item.ItemName, result.items[0].itemName);
        }

        #endregion

        #region CancelOrder
        [Fact]
        public async Task CancelOrderByUser_Correct()
        {
            var (user, item, order) = await SeederAsync(
                status: OrderStatus.PendingPayment,
                quantity: 2,
                price: 1000);
            var beforeQuantity = item.Quantity;

            await _model.CancelOrderByUserWithOrderId(order.OrderId, user.UserId);

            var updatedOrder = await _context.Orders.Include(x=> x.OrderItems).SingleAsync(x=> x.OrderId == order.OrderId);

            var updatedItem = await _context.Items.SingleAsync(x => x.ItemId == item.ItemId);

            Assert.Equal(OrderStatus.Cancelled, updatedOrder.Status);
            Assert.Equal(beforeQuantity + 2, updatedItem.Quantity);
        }

        [Fact]
        public async Task CancelOrderByUser_ThrowsWrongStatus()
        {
            var (user, _, order) = await SeederAsync(status: OrderStatus.Delivering);

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _model.CancelOrderByUserWithOrderId(order.OrderId, user.UserId));

            Assert.Contains("Csak függőben", exc.Message);
        }
        #endregion

        #region GetAllOrders
        [Fact]
        public async Task GetAllOrders_Correct()
        {
            await SeederAsync();
            await SeederAsync();

            var result = await _model.GetAllOrders();

            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }


        [Fact]
        public async Task GetAllOrders_ThrowsEmpty()
        {
            using var empty = DbContextFactory.CreateEmpty();
            var model = new OrderModel(empty);

            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() => model.GetAllOrders());

            Assert.Contains("Nincs", exc.Message);
        }

        #endregion

        #region UpdateOrderStatus
        [Fact]
        public async Task UpdateOrderStatus_Correct()
        {
            var (user, item, order) = await SeederAsync(
                status: OrderStatus.PaymentSuccess,
                quantity: 2,
                price: 1000);
            var beforeQuantity = item.Quantity;

            var dto = new UpdateOrderStatusDto
            {
                orderId = order.OrderId,
                orderStatus = "Delivering"
            };

            await _model.UpdateOrderStatus(dto);

            var updatedOrder = await _context.Orders.Include(x=> x.OrderItems).SingleAsync(x=> x.OrderId == order.OrderId);
            var updatedItem = await _context.Items.SingleAsync(x => x.ItemId == item.ItemId);

            Assert.Equal(OrderStatus.Delivering, updatedOrder.Status);
            Assert.Equal(beforeQuantity - 2, updatedItem.Quantity);
        }

        [Fact]
        public async Task UpdateOrderStatus_ThrowsInvalidStatus()
        {
            var (_, _, order) = await SeederAsync();
            var dto = new UpdateOrderStatusDto
            {
                orderId = order.OrderId,
                orderStatus = "scammed"
            };

            var exc = await Assert.ThrowsAsync<ArgumentException>(() => _model.UpdateOrderStatus(dto));

            Assert.Contains("Érvénytelen", exc.Message);
        }
        #endregion

        #region CompleteOrder
        [Fact]
        public async Task CompleteOrder_Correct()
        {
            var (_, item, order) = await SeederAsync(
                status: OrderStatus.Delivering,
                quantity: 2,
                price: 1000);

            await _model.CompleteOrder(order.OrderId);

            var updated = await _context.Orders.SingleAsync(x => x.OrderId == order.OrderId);

            Assert.Equal(OrderStatus.OrderCompleted, updated.Status);


        }

        [Fact]
        public async Task CompleteOrder_PutOrderInHistory()
        {
            var (_, _, order) = await SeederAsync(
                status: OrderStatus.Delivering
                );
            await _model.CompleteOrder(order.OrderId);

            var exists = await _context.Orders.AnyAsync(x=> x.OrderId == order.OrderId);
            Assert.True(exists);
        }

        [Fact]
        public async Task CompleteOrder_ThrowsAlreadyCompleted()
        {
            var (_, _, order) = await SeederAsync(
                status: OrderStatus.OrderCompleted
                );

            var exc = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _model.CompleteOrder(order.OrderId));

            Assert.Contains("Teljesített", exc.Message);
        }

        [Fact]
        public async Task CompleteOrder_ThrowsNotFound()
        {
            var exc = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _model.CompleteOrder(int.MaxValue));

            Assert.Contains("Nem található", exc.Message);
        }

        #endregion
    }
}
