using BrosShop.Models;
using BrosShop.Styles;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace BrosShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window, IThemeable
    {
        public AddCategoryWindow()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            using var context = new BrosShopDbContext(_connectionString);
            if (nameCategoryTextBox.Text.Length > 0)
                await context.BrosShopCategories.AddAsync(new BrosShopCategory { BrosShopCategoryTitle = nameCategoryTextBox.Text });
            await context.SaveChangesAsync();
            MessageBox.Show("Категория добавлена");
            Close();
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