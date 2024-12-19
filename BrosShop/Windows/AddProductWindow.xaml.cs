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
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Net.Http.Headers;
using BrosShop.Styles;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window, IThemeable
    {
        private readonly IConfiguration _configuration;
        public AddProductWindow()
        {
            InitializeComponent();
            Loaded += AddProductWindow_Loaded;
            ApplyTheme();
        }

        private async void AddProductWindow_Loaded(object sender, RoutedEventArgs e)
        {

			await LoadCategoriesAsync();
		}

        public async Task LoadCategoriesAsync()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using BrosShopDbContext context = new(_connectionString);

            var categoriesQuery = context.BrosShopCategories.ToList();

            foreach (var category in categoriesQuery)
            {
                categoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = category.BrosShopCategoryTitle,
                    Tag = category.BrosShopCategoryId
                });
            }
        }

        public async Task LoadWindowAsync()
        {
        }

        private async void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные из текстовых полей
                string name = nameProductTextBox.Text;
                decimal purchasePrice;
                decimal salePrice;
                int category = 0;
                if (!categoryCheckBox.IsChecked.Value)
                    if (categoryComboBox.SelectedItem != null)
                        Int32.TryParse((categoryComboBox.SelectedItem as ComboBoxItem).Tag.ToString(), out category);
                    else
                        category = 0;
                Int32.TryParse(wbArticulProductTextBox.Text, out int wbArticul);
                string description = descriptionProductTextBox.Text;

                // Проверяем, что цены корректные
                if (!decimal.TryParse(purcharesePriceProductTextBox.Text, out purchasePrice))
                {
                    MessageBox.Show("Некорректная закупочная цена.");
                    return;
                }

                if (!decimal.TryParse(priceProductTextBox.Text, out salePrice))
                {
                    MessageBox.Show("Некорректная цена продажи.");
                    return;
                }

                //var categoryId = category == 0 ? null : category;

                var product = new BrosShopProduct
                {
                    BrosShopTitle = name,
                    BrosShopPurcharesePrice = purchasePrice,
                    BrosShopPrice = salePrice,
                    BrosShopCategoryId = category == 0 ? null : category,
                    BrosShopDescription = description,
                    BrosShopWbarticul = wbArticul,
                    BrosShopDiscountPercent = 0
                };
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                var _connectionString = configuration.GetConnectionString("DefaultConnection");
                using BrosShopDbContext context = new(_connectionString);
                await context.BrosShopProducts.AddAsync(product);
                context.SaveChanges();
                // После успешного сохранения
                MessageBox.Show("Изменения успешно сохранены!");
                Close();
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Произошла ошибка {ex.Message}");
            }
        }

        private void PriceProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                priceProductTextBox.Style = (Style)FindResource(typeof(TextBox));
                purcharesePriceProductTextBox.Style = (Style)FindResource(typeof(TextBox));
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
            catch (Exception)
            {
                MessageBox.Show("Пожалуйста введите корректные значения для цены");
            }
        }

        private void CategoryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (categoryCheckBox.IsChecked.Value)
            {
                categoryComboBox.Visibility = Visibility.Hidden;
                return;
            }
            categoryComboBox.Visibility = Visibility.Visible;
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