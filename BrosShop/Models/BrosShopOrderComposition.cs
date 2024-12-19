using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopOrderComposition
{
    public int BrosShopOrderId { get; set; }

    public int BrosShopAttributesId { get; set; }

    public sbyte BrosShopQuantity { get; set; }

    public decimal BrosShopCost { get; set; }

    public virtual BrosShopProductAttribute BrosShopAttributes { get; set; } = null!;

    public virtual BrosShopOrder BrosShopOrder { get; set; } = null!;
}
