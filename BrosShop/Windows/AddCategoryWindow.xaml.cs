using BrosShop.Models;
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
using System.Windows.Shapes;

namespace BrosShop.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window
    {
        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            using var context = new BrosShopDbContext();
            if (nameCategoryTextBox.Text.Length > 0)
                await context.BrosShopCategories.AddAsync(new BrosShopCategory { BrosShopCategoryTitle = nameCategoryTextBox.Text});
        }
    }
}