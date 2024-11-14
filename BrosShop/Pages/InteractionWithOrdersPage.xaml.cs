using BrosShop.Models;
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
            wbCheckBox.IsChecked = true;
            cassaCheckBox.IsChecked = true;
            siteCheckBox.IsChecked = true;
            await CalculateTurnoverAndProfitAsync();
            LoadOrders();
        }

        public void LoadOrders()
        {
            try
            {
                BrosShopDbContext context = new();
                List<BrosShopOrder> orders = [];
                if (wbCheckBox.IsChecked.HasValue)
                    orders.AddRange(context.BrosShopOrders.Include(o => o.BrosShopUser).Where(o => o.BrosShopTypeOrder == "WB"));
                if (cassaCheckBox.IsChecked.HasValue)
                    orders.AddRange(context.BrosShopOrders.Include(o => o.BrosShopUser).Where(o => o.BrosShopTypeOrder == "касса"));
                if (siteCheckBox.IsChecked.HasValue)
                    orders.AddRange(context.BrosShopOrders.Include(o => o.BrosShopUser).Where(o => o.BrosShopTypeOrder == "веб-сайт"));
                orders.OrderBy(o => o.BrosShopOrderId);
                ordersListView.ItemsSource = orders;//.Select(new {});
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка, проверте доступность сервера");
            }
        }

        private void TypeOrderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoadOrders();
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

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateTurnoverAndProfitAsync();
        }
    }
}
