using BrosShop.Models;
using BrosShop.Windows;
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
        private readonly BrosShopDbContext _context;

        public ShowCategoryPage(BrosShopDbContext context)
        {
            InitializeComponent();
            Loaded += ShowCategoryPage_Loaded;
            _context = context;
        }

        private async void ShowCategoryPage_Loaded(object sender, RoutedEventArgs e)
        {

			await LoadCategories();
		}

        public async Task LoadCategories()
        {
            var categories = await _context.BrosShopCategories
                .AsNoTracking()
                .ToListAsync();

            categoriesListView.ItemsSource = categories;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            new AddCategoryWindow().Show();
        }
    }
}
