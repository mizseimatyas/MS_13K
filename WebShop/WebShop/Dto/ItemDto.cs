using WebShop.Persistence;

namespace WebShop.Dto
{
    public class ItemDto
    {
        public int categoryId { get; set; }
        public string itemName { get; set; }
        public int quantity { get; set; }
        public string description { get; set; }
        public int price { get; set; }
    }
}
