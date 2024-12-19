namespace BrosShop.Models;

public partial class BrosShopProductAttribute
{
    public int BrosShopAttributesId { get; set; }

    public int BrosShopProductId { get; set; }

    public int BrosShopCount { get; set; }

    public int? BrosShopColorId { get; set; }

    public int? BrosShopSizeId { get; set; }

    public virtual BrosShopColor? BrosShopColor { get; set; }

    public virtual BrosShopProduct BrosShopProduct { get; set; } = null!;

    public virtual BrosShopSize? BrosShopSize { get; set; }
}
