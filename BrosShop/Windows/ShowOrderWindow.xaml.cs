using BrosShop.Models;
using BrosShop.Styles;
using BrosShop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace BrosShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для ShowOrderWindow.xaml
    /// </summary>
    public partial class ShowOrderWindow : Window, IThemeable
    {
        private readonly int _orderId;
        public ShowOrderWindow(int id)
        {
            InitializeComponent();
            _orderId = id;
            Loaded += ShowOrderWindow_Loaded;
        }

        private async void ShowOrderWindow_Loaded(object sender, RoutedEventArgs e)

        {
            ApplyTheme();
            await LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using BrosShopDbContext context = new(_connectionString);

            var products = await context.BrosShopOrderCompositions
                .Include(oc => oc.BrosShopAttributes.BrosShopProduct)
                .Include(oc => oc.BrosShopAttributes.BrosShopProduct.BrosShopCategory)
                .Include(oc => oc.BrosShopAttributes.BrosShopProduct.BrosShopProductAttributes)
                .Where(oc => oc.BrosShopOrderId == _orderId)
                .Select(oc => new BrosShopSaledProducts
                {
                    BrosShopProductId = oc.BrosShopAttributes.BrosShopProduct.BrosShopProductId,
                    BrosShopTitle = oc.BrosShopAttributes.BrosShopProduct.BrosShopTitle,
                    BrosShopPrice = oc.BrosShopCost,
                    BrosShopTurnover = Math.Round(oc.BrosShopCost * oc.BrosShopQuantity),
                    BrosShopProfit = oc.BrosShopCost - oc.BrosShopAttributes.BrosShopProduct.BrosShopPurcharesePrice,
                    BrosShopCategoryTitle = oc.BrosShopAttributes.BrosShopProduct.BrosShopCategory.BrosShopCategoryTitle,
                    BrosShopAttributeId = oc.BrosShopAttributes.BrosShopProduct.BrosShopProductAttributes.Select(pa => pa.BrosShopAttributesId).FirstOrDefault(),
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
