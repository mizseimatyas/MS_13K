using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
}
