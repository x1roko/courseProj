using BrosShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BrosShop.Pages
{
    /// <summary>
    /// Логика взаимодействия для ShowCategoryPage.xaml
    /// </summary>
    public partial class ShowCategoryPage : Page
    {
        public ShowCategoryPage()
        {
            InitializeComponent();
            Loaded += ShowCategoryPage_Loaded;
        }

        private async void ShowCategoryPage_Loaded(object sender, RoutedEventArgs e)
        {

			await LoadCategories();
		}

        public async Task LoadCategories()
        {
            using var context = new BrosShopDbContext();
            var categories = await context.BrosShopCategories
                .AsNoTracking()
                .ToListAsync();

            categoriesListView.ItemsSource = categories;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
