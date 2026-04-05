using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class OrderItemDto
    {
        public int itemId { get; set; }
        public string itemName { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }
}
