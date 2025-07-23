using System;
using System.Collections.Generic;

namespace CafeManagement.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int OrderId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public decimal? FinalAmount { get; set; }

    public DateTime PaymentTime { get; set; }

    public virtual Order Order { get; set; } = null!;
}
