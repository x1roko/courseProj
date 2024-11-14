using BrosShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
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
            LoadWindow(productId);
        }

        public async Task LoadWindow(int productId)
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
                    nameProductTextBlock.Text = product.BrosShopTitle;
                    purcharesePriceProductTextBlock.Text = $"{product.BrosShopPurcharesePrice}";
                    priceProductTextBlock.Text = $"{product.BrosShopPrice}";
                    descriptionProductTextBox.Text = $"{product.BrosShopDescription}";
                    categoryTextBlock.Text = $"{product.BrosShopCategory.BrosShopCategoryTitle}";
                    wbArticulProductTextBlock.Text = $"{product.BrosShopWbarticul}";
                    var image = product.BrosShopImages.FirstOrDefault();
                    if (image != null)
                    {
                        mainImageProduct.Source = await LoadImageAsync(image.BrosShopImagesId);
                    }
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
                    imagesStackPanel.Children.Add(new Button {
                        Content = "+",
                        Width = 50,
                        Height = 50
                    });
                }
                else
                {
                    nameProductTextBlock.Text = "Продукт не найден";
                    mainImageProduct.Source = null; // Или установить изображение по умолчанию
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или отображение сообщения пользователю
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }


        public async Task<BitmapImage> LoadImageAsync(int productId)
        {
            var apiString = _configuration["ApiSettings:BaseUrl"];
            var _httpClient = new HttpClient();
            var response = await _httpClient.GetAsync($"{apiString}{productId}");
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

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CategoryCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void PriceProductTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
