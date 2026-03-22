using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
    }
}
