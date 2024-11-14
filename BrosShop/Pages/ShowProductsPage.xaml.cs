using BrosShop.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для Products.xaml
    /// </summary>
    public partial class ShowProductsPage : Page
    {
        private ObservableCollection<BrosShopCategoryModel> _categories = new();
        private int _currentPage = 1; // Текущая страница
        private const int _pageSize = 10; // Количество элементов на странице

        public ShowProductsPage()
        {
            InitializeComponent();
            LoadWindowAsync();
        }

        private async Task LoadWindowAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            UpdateCurrentPageDisplay();
        }

        public static decimal GetDiscountPrice(decimal price, int? discountPercent)
        {
            if (discountPercent is >= 0 and <= 100)
            {
                return Math.Round(price * (1 - discountPercent.Value / 100m), 2);
            }
            return Math.Round(price, 2);
        }

        public async Task LoadProductsAsync()
        {
            using var context = new BrosShopDbContext();

            var activeCategoryIds = _categories
                .Where(c => c.BrosShopCategoryIsActive)
                .Select(c => c.BrosShopCategoryId)
                .ToHashSet();

            var products = await context.BrosShopProducts
                .AsNoTracking()
                .Include(p => p.BrosShopProductAttributes)
                .Where(p => activeCategoryIds.Contains(p.BrosShopCategory.BrosShopCategoryId))
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .Select(p => new BrosShopProductsModel
                {
                    BrosShopProductId = p.BrosShopProductId,
                    BrosShopTitle = p.BrosShopTitle,
                    BrosShopPrice = p.BrosShopPrice,
                    BrosShopDiscountPercent = p.BrosShopDiscountPercent,
                    BrosShopDiscountPrice = GetDiscountPrice(p.BrosShopPrice, p.BrosShopDiscountPercent),
                    BrosShopPurcharesePrice = p.BrosShopPurcharesePrice,
                    BrosShopProfit = GetDiscountPrice(p.BrosShopPrice, p.BrosShopDiscountPercent) - p.BrosShopPurcharesePrice,
                    BrosShopCategoryTitle = p.BrosShopCategory.BrosShopCategoryTitle,
                    BrosShopAttributeId = p.BrosShopProductAttributes.Select(pa => pa.BrosShopAttributesId).FirstOrDefault(),
                    BrosShopCount = p.BrosShopProductAttributes.Count
                })
                .ToListAsync();

            productsListView.ItemsSource = products;
        }

        public async Task LoadCategoriesAsync()
        {
            using var context = new BrosShopDbContext();
            var categories = await context.BrosShopCategories
                .Select(c => new BrosShopCategoryModel
                {
                    BrosShopCategoryId = c.BrosShopCategoryId,
                    BrosShopCategoryTitle = c.BrosShopCategoryTitle,
                    BrosShopCategoryIsActive = true
                })
                .ToListAsync();

            _categories = new ObservableCollection<BrosShopCategoryModel>(categories);
            categoryListView.ItemsSource = _categories;
        }

        private async void CategoryCheckBox_ChangeChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int index = (int)checkBox.Tag;
                var category = _categories.FirstOrDefault(c => c.BrosShopCategoryId == index);
                if (category != null)
                {
                    category.BrosShopCategoryIsActive = checkBox.IsChecked.GetValueOrDefault();
                    category.OnPropertyChanged(nameof(category.BrosShopCategoryIsActive));
                    await LoadProductsAsync();
                }
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            new AddProductWindow().Show();
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadProductsAsync();
                UpdateCurrentPageDisplay();
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            await LoadProductsAsync();
            UpdateCurrentPageDisplay();
        }

        private void UpdateCurrentPageDisplay()
        {
            currentPageTextBlock.Text = $"Текущая страница : {_currentPage}";
        }

        private void ShowProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, которая вызвала событие
            Button button = sender as Button;

            // Получаем контекст данных, связанный с кнопкой
            var product = button.DataContext as BrosShopProductsModel; 

            if (product != null)
            {
                new ShowProductWindow(product.BrosShopProductId).Show();

            }
        }
    }
}
