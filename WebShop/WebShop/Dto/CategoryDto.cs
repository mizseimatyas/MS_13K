using WebShop.Persistence;

namespace WebShop.Dto
{
    public class CategoryDto
    {
        public int categoryId { get; set; }
        public string categoryName {  get; set; }
        public int itemCount { get; set; }
    }
}
