using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopProduct
{
    public int BrosShopProductId { get; set; }

    public decimal BrosShopPrice { get; set; }

    public string BrosShopTitle { get; set; } = null!;

    public int? BrosShopDiscountPercent { get; set; }

    public int? BrosShopWbarticul { get; set; }

    public string? BrosShopDescription { get; set; }

    public int? BrosShopCategoryId { get; set; }

    public decimal BrosShopPurcharesePrice { get; set; }

    public virtual BrosShopCategory? BrosShopCategory { get; set; }

    public virtual ICollection<BrosShopProductAttribute> BrosShopProductAttributes { get; set; } = new List<BrosShopProductAttribute>();

    public virtual ICollection<BrosShopReview> BrosShopReviews { get; set; } = new List<BrosShopReview>();
}
