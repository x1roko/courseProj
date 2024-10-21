using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopProductAttribute
{
    public int BrosShopProductId { get; set; }

    public int BrosShopAttributesId { get; set; }

    public int BrosShopCount { get; set; }

    public virtual BrosShopAttribute BrosShopAttributes { get; set; } = null!;

    public virtual BrosShopProduct BrosShopProduct { get; set; } = null!;
}
