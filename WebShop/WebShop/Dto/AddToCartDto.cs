namespace WebShop.Dto
{
    public class AddToCartDto
    {
        public int userId { get; set; }
        public int itemId { get; set; }
        public int quantity { get; set; }
    }
}
