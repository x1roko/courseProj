using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrosShop
{
    public class BrosShopCategoryModel : INotifyPropertyChanged
    {
        public int BrosShopCategoryId { get; set; }

        public string BrosShopCategoryTitle { get; set; } = null!;

        private bool _brosShopCategoryIsActive;

        public bool BrosShopCategoryIsActive
        {
            get => _brosShopCategoryIsActive;
            set
            {
                if (_brosShopCategoryIsActive != value)
                {
                    _brosShopCategoryIsActive = value;
                    OnPropertyChanged(nameof(BrosShopCategoryIsActive));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
