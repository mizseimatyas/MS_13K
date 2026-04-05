using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class OrderDetailsDto
    {
        public int orderId { get; set; }
        public string targetAddress { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public int totalPrice { get; set; }
        public List<OrderItemDto> items { get; set; }
    }
}
