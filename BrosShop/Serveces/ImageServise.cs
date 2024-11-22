using BrosShop.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Configuration;

namespace BrosShop.Serveces
{
    public class ImageService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ImageService()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Загружает изображение на сервер.
        /// </summary>
        /// <param name="productId">Идентификатор продукта, к которому будет привязано изображение.</param>
        /// <param name="filePath">Путь к файлу изображения.</param>
        /// <returns>Идентификатор загруженного изображения.</returns>
        public async Task<int> UploadImageAsync(int productId, string filePath)
        {
            using (var form = new MultipartFormDataContent())
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                    form.Add(fileContent, "file", Path.GetFileName(filePath));
                    var apiString = _configuration["ApiSettings:BaseUrl"];
                    var response = await _httpClient.PostAsync($"{apiString}?productId={productId}", form);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var brosShopImage = JsonConvert.DeserializeObject<BrosShopImage>(jsonResponse);
                        return brosShopImage.BrosShopImagesId; // Возвращаем идентификатор изображения
                    }
                }
            }
            return 0; // Возвращаем 0 в случае неудачи
        }

        /// <summary>
        /// Загружает изображение по его идентификатору.
        /// </summary>
        /// <param name="imageId">Идентификатор изображения.</param>
        /// <returns>BitmapImage загруженного изображения.</returns>
        public async Task<BitmapImage> LoadImageAsync(int imageId)
        {
            var apiString = _configuration["ApiSettings:BaseUrl"];
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
            return null; // Возвращаем null в случае неудачи
        }
    }
}
