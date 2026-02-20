using WebShop.Persistence;

namespace WebShop.Dto
{
    public class CategoryDto
    {
        public string categoryName {  get; set; }
        public List<Item> items { get; set; }
    }
}
