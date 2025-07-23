using System;
using System.Collections.Generic;

namespace CafeManagement.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int TableId { get; set; }

    public int StaffId { get; set; }

    public DateTime OrderTime { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User Staff { get; set; } = null!;

    public virtual CafeTable Table { get; set; } = null!;
}
