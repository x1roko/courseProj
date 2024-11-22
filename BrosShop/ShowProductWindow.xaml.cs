using BrosShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Логика взаимодействия для ShowProductWindow.xaml
    /// </summary>
    public partial class ShowProductWindow : Window
    {
        private readonly IConfiguration _configuration;
        public ShowProductWindow(int productId)
        {
            InitializeComponent();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            LoadWindowAsync(productId);
            LoadCategoriesAsync(productId);
        }

        public async Task LoadCategoriesAsync(int productId)
        {
            using BrosShopDbContext context = new();

            var product = context.BrosShopProducts.FirstOrDefault(p => p.BrosShopProductId == productId);

            if (product != null)
            {
                var categoriesQuery = context.BrosShopCategories.ToList();

                foreach (var category in categoriesQuery)
                {
                    categoryComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = category.BrosShopCategoryTitle,
                        Tag = category.BrosShopCategoryId
                    });
                }

                // Установка выбранной категории на основе продукта
                var selectedCategoryIndex = categoriesQuery.FindIndex(c => c.BrosShopCategoryId == product.BrosShopCategoryId);
                if (selectedCategoryIndex >= 0)
                {
                    categoryComboBox.SelectedIndex = selectedCategoryIndex;
                }
            }
        }

        public async Task LoadWindowAsync(int productId)
        {
            try
            {
                using var context = new BrosShopDbContext();

                var product = await context.BrosShopProducts
                    .Include(p => p.BrosShopImages)
                    .Include(p => p.BrosShopCategory)
                    .FirstOrDefaultAsync(p => p.BrosShopProductId == productId);

                if (product != null)
                {
                    idTextBlock.Text = $"{product.BrosShopProductId}";
                    // Заполняем поля для редактирования
                    nameProductTextBox.Text = product.BrosShopTitle;
                    purcharesePriceProductTextBox.Text = $"{product.BrosShopPurcharesePrice}";
                    priceProductTextBox.Text = $"{product.BrosShopPrice}";
                    descriptionProductTextBox.Text = product.BrosShopDescription;
                    wbArticulProductTextBox.Text = product.BrosShopWbarticul.ToString();

                    // Загружаем изображение
                    var image = product.BrosShopImages.FirstOrDefault();
                    if (image != null)
                    {
                        mainImageProduct.Source = await LoadImageAsync(image.BrosShopImagesId);
                    }

                    // Загружаем дополнительные изображения
                    var images = product.BrosShopImages;
                    if (images != null)
                    {
                        foreach (var imageInCollection in images)
                        {
                            imagesStackPanel.Children.Add(
                                new Image
                                {
                                    Width = 50,
                                    Height = 50,
                                    Source = await LoadImageAsync(imageInCollection.BrosShopImagesId)
                                });
                        }
                    }
                    var uploadButton = new Button
                    {
                        Content = "+",
                        Width = 50,
                        Height = 50
                    };

                    // Привязка обработчика события Click
                    uploadButton.Click += UploadImage_ButtonClick;

                    // Добавление кнопки в StackPanel
                    imagesStackPanel.Children.Add(uploadButton);
                }
                else
                {
                    nameProductTextBox.Text = "Продукт не найден";
                    mainImageProduct.Source = null; // Или установить изображение по умолчанию
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или отображение сообщения пользователю
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private async void UploadImage_ButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif",
                Title = "Select an Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                Int32.TryParse(idTextBlock.Text, out int productId);
                if (productId == 0)
                    return;
                var result = await UploadImageAsync(productId, filePath);
                MessageBox.Show(result ? "Изображение загруженно" : "Изображение не загруженно");
            }
        }

        private async Task<bool> UploadImageAsync(int productId, string filePath)
        {

            var _httpClient = new HttpClient();
            using (var form = new MultipartFormDataContent())
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg"); // Убедитесь, что тип контента соответствует вашему файлу

                form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));
                var apiString = _configuration["ApiSettings:BaseUrl"];
                var response = await _httpClient.PostAsync($"{apiString}?productId={productId}", form);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<BitmapImage> LoadImageAsync(int imageId)
        {
            var apiString = _configuration["ApiSettings:BaseUrl"];
            var _httpClient = new HttpClient();
            var response = await _httpClient.GetAsync($"{apiString}{imageId}");
            if (response.IsSuccessStatusCode)
            {
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream(imageBytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Замораживаем изображение для использования в разных потоках
                }
                return bitmapImage;
            }
            return null;
        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Разрешаем редактирование полей
            nameProductTextBox.IsReadOnly = false;
            purcharesePriceProductTextBox.IsReadOnly = false;
            priceProductTextBox.IsReadOnly = false;
            categoryComboBox.IsEnabled = true;
            categoryCheckBox.IsEnabled = true;
            wbArticulProductTextBox.IsReadOnly = false;
            descriptionProductTextBox.IsReadOnly = false;

            saveProductButton.Visibility = Visibility.Visible;
            editProductButton.Visibility = Visibility.Hidden;
        }

        private void SaveProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные из текстовых полей
            string name = nameProductTextBox.Text;
            decimal purchasePrice;
            decimal salePrice;
            int category = 0;
            if (categoryCheckBox.IsChecked.Value)
                 category = (categoryComboBox.SelectedItem as BrosShopCategory).BrosShopCategoryId;
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

            var product = new BrosShopProduct
            {
                BrosShopTitle = name,
                BrosShopPurcharesePrice = purchasePrice,
                BrosShopPrice = salePrice,
                BrosShopCategoryId = category,
                BrosShopDescription = description,
                BrosShopWbarticul = wbArticul,
                BrosShopDiscountPercent = 0
            };


            // После успешного сохранения
            MessageBox.Show("Изменения успешно сохранены!");

            // Деактивируем кнопку "Сохранить изменения" и активируем кнопку "Редактировать"
            saveProductButton.Visibility = Visibility.Hidden;
            editProductButton.Visibility = Visibility.Visible;

            // Запрещаем редактирование полей
            nameProductTextBox.IsReadOnly = true;
            purcharesePriceProductTextBox.IsReadOnly = true;
            priceProductTextBox.IsReadOnly = true;
            categoryComboBox.IsEnabled = false;
            categoryCheckBox.IsEnabled = false;
            wbArticulProductTextBox.IsReadOnly = true;
            descriptionProductTextBox.IsReadOnly = true;
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
            if (categoryCheckBox.IsChecked.HasValue)
            {
                categoryComboBox.Visibility = Visibility.Collapsed;
                return;
            }
            categoryComboBox.Visibility = Visibility.Visible;
        }
    }
}