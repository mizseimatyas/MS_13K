namespace WebShop.Dto
{
    public class AddNewItemDto
    {
        public string categoryName { get; set; }
        public string itemName { get; set; }
        public int quantity { get; set; }
        public string description { get; set; }
        public int price { get; set; }
    }
}
