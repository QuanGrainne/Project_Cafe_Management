using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafeManagement.Models;

namespace CafeManagement
{
    public class TempOrderData
    {
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}