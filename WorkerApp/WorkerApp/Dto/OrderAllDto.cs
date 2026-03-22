using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerApp.Dto
{
    public class OrderAllDto
    {
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int TotalPrice { get; set; }
        public string TargetAddress { get; set; }
    }
}
