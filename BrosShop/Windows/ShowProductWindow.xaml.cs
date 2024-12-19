using BrosShop.Models;
using BrosShop.Serveces;
using BrosShop.Styles;
using BrosShop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для ShowProductWindow.xaml
    /// </summary>
    public partial class ShowProductWindow : Window, IThemeable
    {
        private ObservableCollection<ProductAttribute> attributeList = [];

        private readonly ImageService _imageService;
        private readonly int _productId;
        public ShowProductWindow(int productId)
        {
            InitializeComponent();
            _imageService = new ImageService();
            _productId = productId;
            Loaded += ShowProductWindow_Loaded;
            ApplyTheme();
        }

        private async void ShowProductWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadWindowAsync();
            await LoadCategoriesAsync();
            await LoadColorsAndSizesAsync();
        }

        public async Task LoadCategoriesAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using BrosShopDbContext context = new(_connectionString);

            var product = context.BrosShopProducts.FirstOrDefault(p => p.BrosShopProductId == _productId);

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

        public async Task LoadWindowAsync()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                var _connectionString = configuration.GetConnectionString("DefaultConnection");
                using BrosShopDbContext context = new(_connectionString);

                var product = await context.BrosShopProducts
                    .Include(p => p.BrosShopImages)
                    .Include(p => p.BrosShopCategory)
                    .FirstOrDefaultAsync(p => p.BrosShopProductId == _productId);

                if (product != null)
                {
                    idTextBlock.Text = $"{product.BrosShopProductId}";
                    // Заполняем поля для редактирования
                    nameProductTextBox.Text = product.BrosShopTitle;
                    purcharesePriceProductTextBox.Text = $"{product.BrosShopPurcharesePrice}";
                    priceProductTextBox.Text = $"{product.BrosShopPrice}";
                    descriptionProductTextBox.Text = product.BrosShopDescription;
                    wbArticulProductTextBox.Text = product.BrosShopWbarticul.ToString();


                    // Загружаем дополнительные изображения
                    var images = product.BrosShopImages.OrderBy(i => i.BrosShopImagesId);
                    if (images != null)
                    {
                        foreach (var imageInCollection in images)
                        {
                            AddImagesStackPanel(imageInCollection.BrosShopImagesId);
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
                    selectedImageProduct.Source = null; // Или установить изображение по умолчанию
                }

                var attributes = await context.BrosShopProductAttributes
                .Where(a => a.BrosShopProductId == _productId)
                .Include(a => a.BrosShopColor)
                .Include(a => a.BrosShopSize)
                .Select(a => new ProductAttribute
                {
                    ColorTitle = a.BrosShopColor.ColorTitle,
                    Size = a.BrosShopSize.Size,
                    Quantity = a.BrosShopCount
                })
                .ToListAsync();

                attributeList = new ObservableCollection<ProductAttribute>(attributes);
                attributesListBox.ItemsSource = attributeList;
            }
            catch (Exception ex)
            {
                // Логирование ошибки или отображение сообщения пользователю
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void ChangeSelectedImageInStackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
            if (clickedImage != null)
            {
                // Здесь вы можете заменить основное изображение
                selectedImageProduct.Source = clickedImage.Source;
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
                var result = await _imageService.UploadImageAsync(productId, filePath);
                MessageBox.Show(result != 0 ? "Изображение загруженно" : "Изображение не загруженно");
                AddImagesStackPanel(result);
            }
        }

        private async void AddImagesStackPanel(int imageId)
        {
            var image = new Image
            {
                Width = 50,
                Height = 50,
                Source = await _imageService.LoadImageAsync(imageId)
            };

            // Добавляем обработчик события MouseDown
            image.MouseDown += ChangeSelectedImageInStackPanel_MouseDown;

            // Проверяем, есть ли уже элементы в StackPanel
            if (imagesStackPanel.Children.Count > 0)
            {
                // Получаем последний элемент
                var lastElement = imagesStackPanel.Children[imagesStackPanel.Children.Count - 1];

                // Проверяем, является ли последний элемент кнопкой
                if (lastElement is Button)
                {
                    // Вставляем новое изображение перед последним элементом
                    imagesStackPanel.Children.Insert(imagesStackPanel.Children.Count - 1, image);
                }
                else
                {
                    // Если последний элемент не кнопка, просто добавляем новое изображение в конец
                    imagesStackPanel.Children.Add(image);
                }
            }
            else
            {
                // Если StackPanel пуст, просто добавляем новое изображение
                imagesStackPanel.Children.Add(image);
            }
            selectedImageProduct.Source = image.Source;
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

            Int32.TryParse(idTextBlock.Text, out int id);

            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using BrosShopDbContext context = new(_connectionString);

            var product = context.BrosShopProducts.Find(id);

            product.BrosShopTitle = name;
            product.BrosShopPurcharesePrice = purchasePrice;
            product.BrosShopPrice = salePrice;
            product.BrosShopCategoryId = category == 0 ? null : category;
            product.BrosShopDescription = description;
            product.BrosShopWbarticul = wbArticul == 0 ? null : wbArticul;
            product.BrosShopDiscountPercent = 0;

            context.SaveChanges();

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

            // Удаляем существующие атрибуты
            context.BrosShopProductAttributes.RemoveRange(product.BrosShopProductAttributes);

            // Добавляем новые атрибуты
            foreach (var attribute in attributeList)
            {
                var productAttribute = new BrosShopProductAttribute
                {
                    BrosShopProductId = product.BrosShopProductId,
                    BrosShopColorId = GetColorIdByTitle(attribute.ColorTitle, context), // Получаем ID цвета по названию
                    BrosShopSizeId = GetSizeIdBySize(attribute.Size, context), // Получаем ID размера по названию
                    BrosShopCount = attribute.Quantity
                };
                context.BrosShopProductAttributes.Add(productAttribute);
            }

            context.SaveChanges();
        }

        // Метод для получения ID цвета по названию
        private int GetColorIdByTitle(string colorTitle, BrosShopDbContext context)
        {
            var color = context.BrosShopColors.FirstOrDefault(c => c.ColorTitle == colorTitle);
            return color?.ColorId ?? 0; // Возвращаем 0, если цвет не найден
        }

        // Метод для получения ID размера по названию
        private int GetSizeIdBySize(string size, BrosShopDbContext context)
        {
            var sizeEntity = context.BrosShopSizes.FirstOrDefault(s => s.Size == size);
            return sizeEntity?.SizeId ?? 0; // Возвращаем 0, если размер не найден
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
                categoryComboBox.Visibility = Visibility.Collapsed;
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

        private void AddAttributeButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранные значения
            var selectedColor = colorComboBox.SelectedItem as ComboBoxItem;
            var selectedSize = sizeComboBox.SelectedItem as ComboBoxItem;
            int quantity;

            if (selectedColor == null || selectedSize == null || !int.TryParse(quantityTextBox.Text, out quantity))
            {
                MessageBox.Show("Пожалуйста, выберите цвет, размер и введите количество.");
                return;
            }

            // Создаем новый атрибут
            var attribute = new ProductAttribute
            {
                ColorTitle = selectedColor.Content.ToString(), // Сохраняем название цвета
                Size = selectedSize.Content.ToString(), // Сохраняем название размера
                Quantity = quantity
            };

            // Добавляем атрибут в список
            attributeList.Add(attribute);

            // Очищаем поля ввода после добавления
            colorComboBox.SelectedItem = null;
            sizeComboBox.SelectedItem = null;
            quantityTextBox.Clear();
        }

        public async Task LoadColorsAndSizesAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using (var context = new BrosShopDbContext(_connectionString))
            {
                // Загрузка цветов
                var colors = await context.BrosShopColors.ToListAsync();
                colorComboBox.Items.Clear();
                foreach (var color in colors)
                {
                    colorComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = color.ColorTitle, // Предположим, что у вас есть свойство ColorName
                        Tag = color.ColorId // Предположим, что у вас есть свойство BrosShopColorId
                    });
                }

                // Загрузка размеров
                var sizes = await context.BrosShopSizes.ToListAsync();
                sizeComboBox.Items.Clear();
                foreach (var size in sizes)
                {
                    sizeComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = size.Size, // Предположим, что у вас есть свойство SizeName
                        Tag = size.SizeId // Предположим, что у вас есть свойство BrosShopSizeId
                    });
                }
            }
        }

    }
}