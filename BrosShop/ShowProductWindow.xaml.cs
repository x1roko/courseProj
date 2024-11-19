﻿using BrosShop.Models;
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
                    imagesStackPanel.Children.Add(new Button
                    {
                        Content = "+",
                        Width = 50,
                        Height = 50
                    });
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

            // Активируем кнопку "Сохранить изменения"
            saveProductButton.IsEnabled = true;
            editProductButton.IsEnabled = false; // Деактивируем кнопку "Ред
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
            string wbArticul = wbArticulProductTextBox.Text;
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

            // var product = new BrosShopProduct
            // {
            //     BrosShopTitle = name,
            //     BrosShopPurcharesePrice = purchasePrice,
            //     BrosShopPrice = salePrice,
            //     BrosShopCategoryTitle = category,
            //     BrosShopWbArticul = wbArticul,
            //     BrosShopDescription = description
            // };


            // После успешного сохранения
            MessageBox.Show("Изменения успешно сохранены!");

            // Деактивируем кнопку "Сохранить изменения" и активируем кнопку "Редактировать"
            saveProductButton.IsEnabled = false;
            editProductButton.IsEnabled = true;

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
    }
}