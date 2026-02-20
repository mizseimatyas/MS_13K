using WebShop.Persistence;

namespace WebShop.Dto
{
    public class OrderDto
    {
        public int cartId { get; set; }
        public int userId { get; set; }
        public string targetAddress { get; set; }
        public int targetPhone { get; set; }
        public DateTimeOffset date { get; set; }
        public OrderStatus status { get; set; }
    }
}
