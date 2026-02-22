using System.ComponentModel.DataAnnotations;
using WebShop.Persistence;

namespace WebShop.Dto
{
    public class CartDto
    {
        public int userId { get; set; }
        public List<CartItemDto> itemList { get; set; }
    }
}
