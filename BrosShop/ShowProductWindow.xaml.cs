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
            await LoadImageAsync(productId);
        }

        public async Task LoadImageAsync(int productId)
        {
            var apiString = _configuration["ApiSettings:BaseUrl"];
            MessageBox.Show($"{apiString}");
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
                mainImageProduct.Source = bitmapImage;
            }
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
