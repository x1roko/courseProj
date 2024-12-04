using BrosShop.Pages;
using BrosShop.Serveces;
using BrosShop.Styles;
using BrosShop.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для CommodityMonitorWindow.xaml
    /// </summary>
    public partial class CommodityMonitorWindow : Window, IThemeable
    {
        private readonly AuthService _authService;
        public CommodityMonitorWindow()
        {
            InitializeComponent();
            Loaded += CommodityMonitorWindow_Loaded;
			_authService = new AuthService();
		}

        private void CommodityMonitorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeToggleButton.IsChecked = Properties.Settings.Default.isDarkTheme;
            UpdateThemeImage();
            ApplyTheme();
			CheckTokenAndOpenAuthWindow();
		}

        private void CheckTokenAndOpenAuthWindow()
        {
            if (_authService.LoadToken() == null)
            {
                // Открываем окно авторизации
                var authWindow = new AuthorizateWindow();
                authWindow.Closed += AuthWindow_Closed; // Подписываемся на событие закрытия
                authWindow.ShowDialog(); // Открываем окно как модальное
            }
            else
            {
                return;
            }
        }

        private void AuthWindow_Closed(object sender, System.EventArgs e)
        {
            if (_authService.LoadToken() == null)
            {
                MessageBox.Show("Не удалось авторизоваться. Приложение будет закрыто.");
                Close();
            }
            else
            {
                return;
            }
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

        public void ApplyTheme()
        {
            Application.Current.MainWindow.Resources.MergedDictionaries.Clear();

            ResourceDictionary lightTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/LightTheme.xaml", UriKind.Relative));
            ResourceDictionary darkTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/DarkTheme.xaml", UriKind.Relative));

            if (Properties.Settings.Default.isDarkTheme)
            {
                Application.Current.MainWindow.Resources.MergedDictionaries.Add(darkTheme);
                Application.Current.Resources.MergedDictionaries.Add(darkTheme);
            }
            else
            {
                Application.Current.MainWindow.Resources.MergedDictionaries.Add(lightTheme);
                Application.Current.Resources.MergedDictionaries.Add(lightTheme);
            }
            Background = (Brush)Resources["WindowBackground"];
            ThemeToggleButton.Background = (Brush)Resources["ThemeButtonBackground"]; ;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Token = null;
            Properties.Settings.Default.Save();
            Close();
            new CommodityMonitorWindow().ShowDialog();
        }

        private void ThemeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.isDarkTheme = true;
            Properties.Settings.Default.Save(); // Сохраняем настройки
            UpdateThemeImage();
            ApplyTheme(); // Применяем выбранную тему
        }

        private void ThemeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.isDarkTheme = false;
            Properties.Settings.Default.Save(); // Сохраняем настройки
            UpdateThemeImage();
            ApplyTheme(); // Применяем выбранную тему
        }

        private void UpdateThemeImage()
        {
            var (visibleImage, hiddenImage) = Properties.Settings.Default.isDarkTheme
                ? (DarkThemeImage, LightThemeImage) : (LightThemeImage, DarkThemeImage);

            AnimateImageVisibility(visibleImage, 0.0, 1.0);
            AnimateImageVisibility(hiddenImage, 1.0, 0.0);
        }

        private static void AnimateImageVisibility(Image image, double fromOpacity, double toOpacity)
        {
            var animation = new DoubleAnimation(fromOpacity, toOpacity, TimeSpan.FromMilliseconds(300));
            animation.Completed += (s, e) =>
            {
                // Устанавливаем видимость на Collapsed только если opacity 0
                if (toOpacity == 0.0)
                    image.Visibility = Visibility.Collapsed;
                else
                    image.Visibility = Visibility.Visible;
            };
            image.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}