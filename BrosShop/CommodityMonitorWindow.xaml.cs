﻿using System;
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

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для CommodityMonitorWindow.xaml
    /// </summary>
    public partial class CommodityMonitorWindow : Window
    {
        public CommodityMonitorWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainTabControl.SelectedItem is TabItem selectedTab)
            {
                switch (selectedTab.Name.ToString())
                {
                    case "ProductsTabItem":
                        mainFrame.Navigate(new ShowProductsPage());
                        break;
                    /*case "Page 2":
                        mainFrame.Navigate(new Page2());
                        break;
                    case "Page 3":
                        mainFrame.Navigate(new Page2());
                        break;
                    case "Page 4":
                        mainFrame.Navigate(new Page2());
                        break;*/
                }
            }
        }
    }
}
