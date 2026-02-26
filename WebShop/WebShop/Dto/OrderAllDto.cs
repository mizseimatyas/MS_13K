using WebShop.Persistence;

namespace WebShop.Dto
{
    public class OrderAllDto
    {
        public int orderId { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public int totalPrice { get; set; }
        public string targetAddress { get; set; }
    }
}
