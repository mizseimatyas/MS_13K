namespace WebShop.Dto
{
    public class ModifyCartItemDto
    {
        public int userId { get; set; }
        public int itemId { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }
}
