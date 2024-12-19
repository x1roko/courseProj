﻿using BrosShop.Models;
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
    /// Логика взаимодействия для ShowClientsPage.xaml
    /// </summary>
    public partial class ShowClientsPage : Page
    {
        private readonly BrosShopDbContext _context;

        public ShowClientsPage(BrosShopDbContext context)
        {
            InitializeComponent();
            Loaded += ShowClientsPage_Loaded;
            _context = context;
        }

        private async void ShowClientsPage_Loaded(object sender, RoutedEventArgs e)
        {
			await LoadClients();
		}

        public async Task LoadClients()
        {
            var clients = await _context.BrosShopUsers
                .AsNoTracking()
                .ToListAsync();

            clientsListView.ItemsSource = clients;
        }
    }
}
