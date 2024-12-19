using BrosShop.Models;
using BrosShop.Pages;
using BrosShop.Serveces;
using BrosShop.Styles;
using BrosShop.Windows;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BrosShop
{
    /// <summary>
    /// Логика взаимодействия для CommodityMonitorWindow.xaml
    /// </summary>
    public partial class CommodityMonitorWindow : Window, IThemeable
    {
        private readonly AuthService _authService;
        private readonly BrosShopDbContext _context;
        private readonly string _connectionString;
        public CommodityMonitorWindow()
        {
            InitializeComponent();
            Loaded += CommodityMonitorWindow_Loaded;
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _authService = new AuthService();
            _context = new BrosShopDbContext(_connectionString);
            CheckTokenAndOpenAuthWindow();
        }

        private void CommodityMonitorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeToggleButton.IsChecked = Properties.Settings.Default.isDarkTheme;
            UpdateThemeImage();
            ApplyTheme();
            //CheckTokenAndOpenAuthWindow();
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
                        mainFrame.Navigate(new ShowProductsPage(_context));
                        break;
                    case "OrdersTabItem":
                        mainFrame.Navigate(new InteractionWithOrdersPage(_context, _connectionString));
                        break;
                    case "CategoryTabItem":
                        mainFrame.Navigate(new ShowCategoryPage(_context));
                        break;
                    case "ClientsTabItem":
                        mainFrame.Navigate(new ShowClientsPage(_context));
                        break;
                }
            }
        }

        public void ApplyTheme()
        {
            // Очищаем текущие словари ресурсов
            Application.Current.MainWindow.Resources.MergedDictionaries.Clear();

            // Загружаем новые словари ресурсов
            ResourceDictionary lightTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/LightTheme.xaml", UriKind.Relative));
            ResourceDictionary darkTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/DarkTheme.xaml", UriKind.Relative));

            // Определяем новый фон
            SolidColorBrush newBackground = Properties.Settings.Default.isDarkTheme
                    ? (SolidColorBrush)darkTheme["WindowBackground"]
                    : (SolidColorBrush)lightTheme["WindowBackground"];
            // Анимируем смену фона
            AnimateWindowBackgroundChange(newBackground);

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

            // Обновляем фон и другие элементы после смены темы
            Background = (Brush)Resources["WindowBackground"];
            ThemeToggleButton.Background = (Brush)Resources["ThemeButtonBackground"];
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

        private void AnimateWindowBackgroundChange(SolidColorBrush newBackground)
        {
            // Создаем новый SolidColorBrush для анимации
            var animatedBrush = new SolidColorBrush(((SolidColorBrush)Background).Color);
            Background = animatedBrush; // Устанавливаем анимируемый цвет как фон окна

            // Создаем анимацию цвета
            var colorAnimation = new ColorAnimation
            {
                From = animatedBrush.Color,
                To = newBackground.Color,
                Duration = TimeSpan.FromMilliseconds(1300)
            };

            // Привязываем анимацию к свойству Color
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(colorAnimation);
            Storyboard.SetTarget(colorAnimation, animatedBrush);
            Storyboard.SetTargetProperty(colorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));

            // Запускаем анимацию
            storyboard.Begin();

            // Устанавливаем новый фон после завершения анимации
            storyboard.Completed += (s, e) =>
            {
                Background = newBackground;
            };
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