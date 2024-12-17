using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrosShop.ViewModels
{
    public class BrosShopOrderModel
    {
        public int BrosShopOrderId { get; set; }
        public DateTime BrosShopDateTimeOrder { get; set; }
        public string BrosShopTypeOrder { get; set; }
        public string UserName { get; set; } // Имя пользователя
        public int ItemCount { get; set; } // Количество позиций
    }
}
