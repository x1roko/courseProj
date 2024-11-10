using BrosShop.Pages;
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
                    case "OrdersTabItem":
                        mainFrame.Navigate(new InteractionWithOrdersPage());
                        break;
                    case "CategoryTabItem":
                        mainFrame.Navigate(new ShowCategoryPage());
                        break;
                    case "ClientsTabItem":
                        mainFrame.Navigate(new ShowClientsPage());
                        break;
                }
            }
        }
    }
}
