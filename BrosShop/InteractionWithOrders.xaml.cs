using BrosShop.Models;
using System.Windows;
using System.Windows.Controls;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для InteractionWithOrders.xaml
    /// </summary>
    public partial class InteractionWithOrdersPage : Page
    {

        public InteractionWithOrdersPage()
        {
            InitializeComponent();
            LoadWindow();
        }

        public void LoadWindow()
        {
            wbCheckBox.IsChecked = true;
            cassaCheckBox.IsChecked = true;
            siteCheckBox.IsChecked = true;
        }

        public void LoadOrders()
        {
            try
            {
                BrosShopDbContext context = new();
                List<BrosShopOrder> orders = [];
                if (wbCheckBox.IsChecked.Value)
                    orders.AddRange(context.BrosShopOrders.Where(o => o.BrosShopTypeOrder == "WB"));
                if (cassaCheckBox.IsChecked.HasValue)
                    orders.AddRange(context.BrosShopOrders.Where(o => o.BrosShopTypeOrder == "касса"));
                if (siteCheckBox.IsChecked.HasValue)
                    orders.AddRange(context.BrosShopOrders.Where(o => o.BrosShopTypeOrder == "веб-сайт"));
                orders.OrderBy(o => o.BrosShopOrderId);
                ordersListView.ItemsSource = orders;
            }
            catch (Exception)
            {
            }
        }

        private void TypeOrderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
