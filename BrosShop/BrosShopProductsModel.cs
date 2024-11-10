using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrosShop
{
    public class BrosShopProductsModel
    {
        public int BrosShopProductId { get; set; }

        public decimal BrosShopPrice { get; set; }

        public decimal BrosShopPurcharesePrice { get; set; }

        public decimal BrosShopDiscountPrice { get; set; }

        public decimal BrosShopProfit {  get; set; }

        public string BrosShopTitle { get; set; } = null!;

        public int? BrosShopDiscountPercent { get; set; }

        public string BrosShopCategoryTitle { get; set; }

        public int? BrosShopAttributeId { get; set; }
        public int BrosShopCount { get; set; }
    }
}
