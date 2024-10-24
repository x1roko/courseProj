using BrosShop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        public void LoadCategories()
        {
            
            using BrosShopDbContext context = new();
            var categoriesQuery = context.BrosShopCategories.ToList();

            //categoryComboBox.ItemsSource = categoriesQuery;

            foreach (var category in categoriesQuery)
            {
                categoryComboBox.Items.Add(new ComboBoxItem
                {
                    Content = category.BrosShopCategoryTitle,
                    Tag = category.BrosShopCategoryId
                });
            }
            categoryComboBox.SelectedIndex = 0;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            BrosShopProduct product = new()
            {
                //
            };
            using BrosShopDbContext context = new();
            context.BrosShopProducts.Add(product);
            context.SaveChanges();
            MessageBox.Show($"Товар {product.BrosShopTitle} добавлен");
            Close();
        }

        private void CategoryCheckBox_ChangeChecked(object sender, RoutedEventArgs e)
        {

        }

    }
}
