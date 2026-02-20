using System.ComponentModel.DataAnnotations;
using WebShop.Persistence;

namespace WebShop.Dto
{
    public class CartDto
    {
        public int userId { get; set; }
        public int itemId { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public List<Item> itemList { get; set; }
    }
}
