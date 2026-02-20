using Microsoft.EntityFrameworkCore;

namespace WebShop.Dto
{
    public class SearchItemsByDto
    {
        public string categoryNamE { get; set; }
        public string itemNamE { get; set; }
        public int pricE { get; set; }

    }
}