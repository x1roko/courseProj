using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BrosShop.Models;
using BrosShop.Styles;
using BrosShop.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BrosShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для ShowOrderWindow.xaml
    /// </summary>
    public partial class ShowOrderWindow : Window, IThemeable
    {
        int orderId;
        public ShowOrderWindow(int id)
        {
            InitializeComponent();
            orderId = id;
            LoadWindow();
        }

        private async Task LoadWindow()
        {
            ApplyTheme();
            await LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            using var context = new BrosShopDbContext();

            var products = await context.BrosShopOrderCompositions
                .Include(oc => oc.BrosShopProduct)
                .Include(oc => oc.BrosShopProduct.BrosShopCategory)
                .Include(oc => oc.BrosShopProduct.BrosShopProductAttributes)
                .Where(oc => oc.BrosShopOrderId == orderId)
                .Select(oc => new BrosShopSaledProducts
                {
                    BrosShopProductId = oc.BrosShopProductId,
                    BrosShopTitle = oc.BrosShopProduct.BrosShopTitle,
                    BrosShopPrice = oc.BrosShopCost,
                    BrosShopTurnover = oc.BrosShopCost * oc.BrosShopQuantity,
                    BrosShopProfit = oc.BrosShopCost - oc.BrosShopProduct.BrosShopPurcharesePrice,
                    BrosShopCategoryTitle = oc.BrosShopProduct.BrosShopCategory.BrosShopCategoryTitle,
                    BrosShopAttributeId = oc.BrosShopProduct.BrosShopProductAttributes.Select(pa => pa.BrosShopAttributesId).FirstOrDefault(),
                    BrosShopCount = oc.BrosShopQuantity
                }).ToListAsync();

            productsInOrderListView.ItemsSource = products;
        }

        public void ApplyTheme()
        {
            Resources.MergedDictionaries.Clear();

            ResourceDictionary lightTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/LightTheme.xaml", UriKind.Relative));
            ResourceDictionary darkTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/DarkTheme.xaml", UriKind.Relative));

            if (Properties.Settings.Default.isDarkTheme)
                Resources.MergedDictionaries.Add(darkTheme);
            else
                Resources.MergedDictionaries.Add(lightTheme);
            Background = (Brush)Resources["WindowBackground"];
        }
    }
}
