using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopOrder
{
    public int BrosShopOrderId { get; set; }

    public int BrosShopUserId { get; set; }

    public DateTime BrosShopDateTimeOrder { get; set; }

    public string? BrosShopTypeOrder { get; set; }
}
