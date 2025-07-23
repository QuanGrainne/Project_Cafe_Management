using System;
using System.Collections.Generic;

namespace CafeManagement.Models;

public partial class CafeTable
{
    public int TableId { get; set; }

    public string TableName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
