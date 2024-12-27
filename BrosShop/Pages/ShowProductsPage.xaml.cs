using BrosShop.Models;
using BrosShop.Serveces;
using BrosShop.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для Products.xaml
    /// </summary>
    public partial class ShowProductsPage : Page
    {
        private ObservableCollection<BrosShopCategoryModel> _categories = new();
        private int _currentPage = 1; // Текущая страница
        private const int _pageSize = 18; // Количество элементов на странице
        private readonly string _connectionString;

        public ShowProductsPage(string connectionString)
        {
            InitializeComponent();
            Loaded += ShowProductsPage_Loaded;
            _connectionString = connectionString;
        }

        private async void ShowProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
			await LoadCategoriesAsync();
			await LoadAllProductsAsync();
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

        public async Task LoadAllProductsAsync()
        {
            BrosShopDbContext context = new(_connectionString);
            var products = await context.BrosShopProducts
                .AsNoTracking()
                .Include(p => p.BrosShopProductAttributes)
                .Include(p => p.BrosShopImages)
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
                }).ToListAsync();

            productsListView.ItemsSource = products;
        }

        public async Task LoadProductsAsync()
        {
            var activeCategoryIds = _categories
                .Where(c => c.BrosShopCategoryIsActive)
                .Select(c => c.BrosShopCategoryId)
                .ToHashSet();

            BrosShopDbContext context = new(_connectionString);

            var products = await context.BrosShopProducts
                .AsNoTracking()
                .Include(p => p.BrosShopProductAttributes)
                .Include(p => p.BrosShopImages)
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
                }).ToListAsync();

            productsListView.ItemsSource = products;
        }

        public async Task LoadCategoriesAsync()
        {
            BrosShopDbContext context = new(_connectionString);

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
            currentPageTextBlock.Text = $"{_currentPage}";
        }

        private void ProductsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedProduct = productsListView.SelectedItem as BrosShopProductsModel;
            if (selectedProduct != null)
            {
                new ShowProductWindow(selectedProduct.BrosShopProductId).Show();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAllProductsAsync();
        }

        private async void ShowAllProductsButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAllProductsAsync();
        }
    }
}