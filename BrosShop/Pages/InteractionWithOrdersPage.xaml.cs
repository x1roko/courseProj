using BrosShop.Models;
using BrosShop.ViewModels;
using BrosShop.Windows;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для InteractionWithOrders.xaml
    /// </summary>
    public partial class InteractionWithOrdersPage : Page
    {

        private int _currentPage = 1; // Текущая страница
        private const int _pageSize = 18; // Количество элементов на странице
        public InteractionWithOrdersPage()
        {
            InitializeComponent();
            Loaded += InteractionWithOrdersPage_Loaded;
        }

        private async void InteractionWithOrdersPage_Loaded(object sender, RoutedEventArgs e)
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
                var allOrders = await context.BrosShopOrders
                    .AsNoTracking()
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToListAsync();

                ordersListView.ItemsSource = allOrders;
            }
            catch (Exception)
            {
                MessageBox.Show("Возникла ошибка, проверте доступность сервера");
            }
        }

        public async Task LoadOrdersAsync()
        {
            try
            {
                using BrosShopDbContext context = new();

                var activeTypes = new HashSet<string>();

                if (wbCheckBox.IsChecked == true) activeTypes.Add("WB");
                if (cassaCheckBox.IsChecked == true) activeTypes.Add("касса");
                if (siteCheckBox.IsChecked == true) activeTypes.Add("веб-сайт");

                // Получаем все заказы из базы данных
                var allOrders = await context.BrosShopOrders
                    .AsNoTracking()
                    .Skip((_currentPage - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToListAsync();

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
            await LoadOrdersAsync();
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
            showProfitTextBlock.Text = $"{mainCalendar.SelectedDate.Value.Date}\nОборот :\t{turnover}\nПрибыль :\t{profit}";
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

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadOrdersAsync();
                UpdateCurrentPageDisplay();
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            await LoadOrdersAsync();
            UpdateCurrentPageDisplay();
        }

        private void UpdateCurrentPageDisplay()
        {
            currentPageTextBlock.Text = $"{_currentPage}";
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveOrdersToExcel();
        }

        private async Task SaveOrdersToExcel()
        {
            try
            {
                using BrosShopDbContext context = new();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Получаем заказы за текущий месяц
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var orders = context.BrosShopOrders
                    .AsNoTracking()
                    .Where(o => o.BrosShopDateTimeOrder >= startDate && o.BrosShopDateTimeOrder <= endDate)
                    //.Include(o => o.)
                    //.ThenInclude(c => c.BrosShopProduct)
                    .Select(o => o.BrosShopOrderId)
                    .ToHashSet();

                var ordersComposition = await context.BrosShopOrderCompositions
                    .AsNoTracking()
                    .Where(oc => orders.Contains(oc.BrosShopOrderId)) // Сравниваем с idOrder
                    .Include(oc => oc.BrosShopOrder)
                    .Include(oc => oc.BrosShopProduct) // Загружаем продукты для каждой составной части
                    .ToListAsync();

                // Создаем Excel файл
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Orders");

                    // Заголовки
                    worksheet.Cells[1, 1].Value = "Номер заказа";
                    worksheet.Cells[1, 2].Value = "Время заказа";
                    worksheet.Cells[1, 3].Value = "Общая сумма заказа";
                    worksheet.Cells[1, 4].Value = "Имя товара";
                    worksheet.Cells[1, 5].Value = "Цена";
                    worksheet.Cells[1, 6].Value = "Количество";

                    worksheet.Column(2).Width = 20;
                    worksheet.Column(4).Width = 25;

                    int row = 2;

                    foreach (var order in ordersComposition)
                    {
                        // Сумма для текущего заказа
                        var orderCompositions = ordersComposition.Where(oc => oc.BrosShopOrderId == order.BrosShopOrderId).ToList();
                        var orderSum = orderCompositions.Sum(c => c.BrosShopCost * c.BrosShopQuantity);

                        // Записываем данные о заказе
                        worksheet.Cells[row, 1].Value = order.BrosShopOrderId;
                        worksheet.Cells[row, 2].Value = order.BrosShopOrder.BrosShopDateTimeOrder;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                        worksheet.Cells[row, 3].Value = orderSum;

                        row++;
                        // Добавляем составные части заказа
                        foreach (var composition in orderCompositions)
                        {
                            worksheet.Cells[row, 1].Value = ""; // idOrder
                            worksheet.Cells[row, 2].Value = ""; // DatetimeOrder
                            worksheet.Cells[row, 3].Value = ""; // summ
                            worksheet.Cells[row, 4].Value = composition.BrosShopProduct.BrosShopTitle; // Title
                            worksheet.Cells[row, 5].Value = composition.BrosShopCost; // Cost
                            worksheet.Cells[row, 6].Value = composition.BrosShopQuantity; // Quantity
                            row++;
                        }
                    }

                    // Сохраняем файл
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        Title = "Сохранить заказы"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var file = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(file);
                        MessageBox.Show("Заказы успешно сохранены!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private async void Page_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                await SaveOrdersToExcel();
        }
    }
}
