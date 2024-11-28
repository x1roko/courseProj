using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrosShop.ViewModels
{
    public class BrosShopSaledProducts
    {
        public int BrosShopProductId { get; set; }

        public decimal BrosShopPrice { get; set; }

        public decimal BrosShopProfit { get; set; }

        public string BrosShopTitle { get; set; } = null!;

        public string BrosShopCategoryTitle { get; set; }

        public int? BrosShopAttributeId { get; set; }
        public int BrosShopCount { get; set; }
    }
}
