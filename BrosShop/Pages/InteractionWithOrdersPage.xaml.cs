using BrosShop.Models;
using BrosShop.ViewModels;
using BrosShop.Windows;
using Microsoft.EntityFrameworkCore;
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
            LoadPage();
        }

        public async Task LoadPage()
        {
            await LoadAllOrders();
            await CalculateTurnoverAndProfitAsync();
            wbCheckBox.IsChecked = true;
            cassaCheckBox.IsChecked = true;
            siteCheckBox.IsChecked = true;
        }

        public async Task LoadAllOrders()
        {
            try
            {
                using BrosShopDbContext context = new();

                var activeTypes = new HashSet<string>();

                // Получаем все заказы из базы данных
                var allOrders = await context.BrosShopOrders.AsNoTracking().ToListAsync(); 

                ordersListView.ItemsSource = allOrders;
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка, проверте доступность сервера");
            }
        }

        public async Task LoadOrders()
        {
            try
            {
                using BrosShopDbContext context = new();

                var activeTypes = new HashSet<string>();

                if (wbCheckBox.IsChecked == true) activeTypes.Add("WB");
                if (cassaCheckBox.IsChecked == true) activeTypes.Add("касса");
                if (siteCheckBox.IsChecked == true) activeTypes.Add("веб-сайт");

                // Получаем все заказы из базы данных
                var allOrders = await context.BrosShopOrders.AsNoTracking().ToListAsync();

                // Создаем новый список для хранения отфильтрованных заказов
                List<BrosShopOrder> filteredOrders = [];

                foreach (var order in allOrders)
                {
                    // Проверяем, содержит ли BrosShopTypeOrder хотя бы один из активных типов
                    if (order.BrosShopTypeOrder != null && activeTypes.Any(type => order.BrosShopTypeOrder.Contains(type)))
                    {
                        filteredOrders.Add(order);
                    }
                }

                ordersListView.ItemsSource = filteredOrders;
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка, проверте доступность сервера");
            }
        }

        private async void TypeOrderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private async Task CalculateTurnoverAndProfitAsync()
        {
            try
            {
                decimal turnover = 0, profit = 0;

                using var context = new BrosShopDbContext();

                if (!mainCalendar.SelectedDate.HasValue)
                {
                    mainCalendar.SelectedDate = DateTime.Today;
                }

                var ordersIds = await context.BrosShopOrders
                    .AsNoTracking()
                    .Where(o => o.BrosShopDateTimeOrder.Date == mainCalendar.SelectedDate.Value)
                    .Select(o => o.BrosShopOrderId)
                    .ToListAsync();

                if (ordersIds.Count == 0)
                {
                    ShowProfit();
                    return;
                }

                var ordersCompositions = await context.BrosShopOrderCompositions
                    .AsNoTracking()
                    .Where(o => ordersIds.Contains(o.BrosShopOrderId))
                    .Include(o => o.BrosShopProduct)
                    .ToListAsync();

                foreach (var order in ordersCompositions)
                {
                    turnover += order.BrosShopCost * order.BrosShopQuantity;
                    profit += (order.BrosShopCost - order.BrosShopProduct.BrosShopPurcharesePrice) * order.BrosShopQuantity;
                }
                ShowProfit(turnover, profit);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Возникла ошибка : {exception.Message}\n пожалуйста сохраните её и обратитесь к разработчику");
            }
        }

        private void ShowProfit(decimal turnover = 0, decimal profit = 0)
        {
            showProfitTextBlock.Text = $"За {mainCalendar.SelectedDate.Value.Date} оборот составил {turnover}\n, а прибыль {profit}";
        }

        private async void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            await CalculateTurnoverAndProfitAsync();
        }

        private void OrdersListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedOrder = ordersListView.SelectedItem as BrosShopOrder;

            if (selectedOrder != null)
            {
                new ShowOrderWindow(selectedOrder.BrosShopOrderId).Show();
            }
        }
    }
}
