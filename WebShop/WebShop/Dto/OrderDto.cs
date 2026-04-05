using WebShop.Persistence;

namespace WebShop.Dto
{
    public class OrderDto
    {
        public int userId { get; set; }
        public int orderId { get; set; }
        public string targetAddress { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public int totalPrice { get; set; }
        public List<OrderItemDto> items { get; set; }
    }
}
