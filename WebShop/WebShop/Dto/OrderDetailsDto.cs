using WebShop.Persistence;

namespace WebShop.Dto
{
    public class OrderDetailsDto
    {
        public int orderId { get; set; }
        public string targetAddress { get; set; }
        public DateTimeOffset date { get; set; }
        public string status { get; set; }
        public int totalPrice { get; set; }
        public List<OrderItemDto> items { get; set; }
    }
}
