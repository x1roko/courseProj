﻿using BrosShop.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для Products.xaml
    /// </summary>
    public partial class ShowProductsWindow : Window
    {
        private ObservableCollection<BrosShopProductsModel> _products = new(); // Исправлено
        private ObservableCollection<BrosShopCategoryModel> _categories = new();
        private BrosShopCategoryModel _brosShopCategoryModel = new();

        public ShowProductsWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
            categoryListView.DataContext = _categories;
        }

        public static decimal GetDiscountPrice(decimal price, int? discountPercent)
        {
            if (discountPercent.HasValue && discountPercent.Value >= 0 && discountPercent.Value <= 100)
            {
                decimal discountFactor = (100 - discountPercent.Value) / 100m; 
                return Math.Round(price * discountFactor, 2);
            }
            return Math.Round(price, 2);
        }

        public void LoadProducts()
        {
            _products.Clear();
            using BrosShopDbContext context = new();

            // Выполняем запрос к базе данных
            var productsQuery = context.BrosShopProducts
                .Include(p => p.BrosShopProductAttributes)
                .Select(p => new BrosShopProductsModel
                {
                    BrosShopProductId = p.BrosShopProductId,
                    BrosShopTitle = p.BrosShopTitle,
                    BrosShopPrice = p.BrosShopPrice,
                    BrosShopDiscountPercent = p.BrosShopDiscountPercent,
                    BrosShopDiscountPrice = GetDiscountPrice(p.BrosShopPrice, p.BrosShopDiscountPercent),
                    BrosShopPurcharesePrice = p.BrosShopPurcharesePrice,
                    BrosShopProfit = GetDiscountPrice(p.BrosShopPrice, p.BrosShopDiscountPercent) - p.BrosShopPurcharesePrice,
                    BrosShopCategoryTitle = p.BrosShopCategory.BrosShopCategoryTitle,
                    BrosShopAttributeId = p.BrosShopProductAttributes.Select(pa => pa.BrosShopAttributesId).FirstOrDefault(),
                    BrosShopCount = p.BrosShopProductAttributes.Count,
                    //BrosShopDescription = p.BrosShopDescription
                }).ToList(); // Выполняем запрос здесь

            // Получаем активные категории
            var activeCategories = _categories
                .Where(c => c.BrosShopCategoryIsActive)
                .Select(c => c.BrosShopCategoryTitle)
                .ToList();

            // Фильтруем продукты по активным категориям
            foreach (var product in productsQuery)
            {
                if (activeCategories.Contains(product.BrosShopCategoryTitle))
                {
                    _products.Add(product);
                }
            }

            productsListView.ItemsSource = _products;
        }

        public void LoadCategories()
        {
            using BrosShopDbContext context = new();
            var categoriesQuery = context.BrosShopCategories
                .Select(c => new BrosShopCategoryModel
                {
                    BrosShopCategoryId = c.BrosShopCategoryId,
                    BrosShopCategoryTitle = c.BrosShopCategoryTitle,
                    BrosShopCategoryIsActive = true
                }).ToList(); // Выполняем запрос здесь

            _categories = new ObservableCollection<BrosShopCategoryModel>(categoriesQuery);
            categoryListView.ItemsSource = _categories; // Убираем ToList()
        }

        private void CategoryCheckBox_ChangeChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int index = (int)checkBox.Tag;
                var category = _categories.FirstOrDefault(c => c.BrosShopCategoryId == index);
                if (category != null)
                {
                    category.BrosShopCategoryIsActive = checkBox.IsChecked.GetValueOrDefault(); // Используем GetValueOrDefault
                    category.OnPropertyChanged(nameof(category.BrosShopCategoryIsActive));
                    //MessageBox.Show($"Category {category.BrosShopCategoryTitle} is now {(category.BrosShopCategoryIsActive ? "active" : "inactive")}");
                }
                LoadProducts();
            }
        }

        private void CahngeProductButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddProductWindow window = new();
            window.Show();
        }
    }
}
