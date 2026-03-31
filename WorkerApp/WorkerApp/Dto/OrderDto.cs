using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class OrderDto
    {
        public int UserId { get; set; }
        public string TargetAddress { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int TotalPrice { get; set; }
        public List<OrderItemDto> items { get; set; }
    }
}
