using System;
using System.Collections.Generic;

namespace BrosShop.Models;

public partial class BrosShopReview
{
    public int BrosShopReviewId { get; set; }

    public int BrosShopProductId { get; set; }

    public int BrosShopUserId { get; set; }

    public sbyte BrosShopRating { get; set; }

    public string? BrosShopComment { get; set; }

    public DateTime BrosShopDateTime { get; set; }

    public virtual BrosShopProduct BrosShopProduct { get; set; } = null!;

    public virtual BrosShopUser BrosShopUser { get; set; } = null!;
}
