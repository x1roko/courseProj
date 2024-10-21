using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopAttribute
{
    public int BrosShopAttributesId { get; set; }

    public string? BrosShopSize { get; set; }

    public string? BrosShopColor { get; set; }

    public virtual ICollection<BrosShopProductAttribute> BrosShopProductAttributes { get; set; } = new List<BrosShopProductAttribute>();
}
