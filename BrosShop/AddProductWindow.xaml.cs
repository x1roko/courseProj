using BrosShop.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        private HttpClient _httpClient;
        public AddProductWindow()
        {
            InitializeComponent();
            LoadWindow();
        } 

        public async void LoadWindow()
        {
            await LoadCategoriesAsync();
        }

        public async Task LoadCategoriesAsync()
        {

            using BrosShopDbContext context = new();
            var categoriesQuery = context.BrosShopCategories.ToList();

            foreach (var category in categoriesQuery)
            {
                categoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = category.BrosShopCategoryTitle,
                    Tag = category.BrosShopCategoryId
                });
            }
            categoryComboBox.SelectedIndex = 0;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            BrosShopProduct product = new()
            {
                //
            };
            using BrosShopDbContext context = new();
            context.BrosShopProducts.Add(product);
            context.SaveChanges();
            MessageBox.Show($"Товар {product.BrosShopTitle} добавлен");
            Close();
        }

        private void CategoryCheckBox_ChangeChecked(object sender, RoutedEventArgs e)
        {
            bool isChecked = categoryCheckBox.IsChecked ?? false;
            categoryComboBox.Visibility = isChecked ? Visibility.Collapsed : Visibility.Visible;
            categoryTextBlock.Visibility = isChecked ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PriceProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                priceProductTextBox.Style = (Style)FindResource(typeof(TextBox));
                purcharesePriceProductTextBox.Style = (Style)FindResource(typeof(TextBox));
                if (priceProductTextBox.Text.Length > 0 && purcharesePriceProductTextBox.Text.Length > 0)
                {
                    decimal priceProduct, purchasePriceProduct;
                    bool isValidPrice = decimal.TryParse(priceProductTextBox.Text, out priceProduct);
                    bool isValidPurchasePrice = decimal.TryParse(purcharesePriceProductTextBox.Text, out purchasePriceProduct);
                    if (isValidPrice && isValidPurchasePrice)
                    {
                        profitTextBlock.Text = $"Прибыль с 1 штуки: {priceProduct - purchasePriceProduct}";
                        return;
                    }
                    if (!isValidPrice)
                    {
                        priceProductTextBox.Style = (Style)FindResource("ErrorTextBox");
                    }
                    if (!isValidPurchasePrice)
                    {
                        purcharesePriceProductTextBox.Style = (Style)FindResource("ErrorTextBox");
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Пожалуйста введите корректные значения для цены");
            }
        }
    }
}
