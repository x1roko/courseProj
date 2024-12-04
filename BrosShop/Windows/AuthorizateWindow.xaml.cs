using BrosShop.Serveces;
using BrosShop.Styles;
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
    /// Логика взаимодействия для AuthorizateWindow.xaml
    /// </summary>
    public partial class AuthorizateWindow : Window, IThemeable
    {
        private readonly AuthService _authService;
        public AuthorizateWindow()
        {
            InitializeComponent();
            ApplyTheme();
            _authService = new AuthService();
            CheckAuthorize();
        }

        public void CheckAuthorize()
        {
            if (_authService.LoadToken() != null)
            {
                Close();
            }
        }

        public void ApplyTheme()
        {
            Resources.MergedDictionaries.Clear();

            ResourceDictionary lightTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/LightTheme.xaml", UriKind.Relative));
            ResourceDictionary darkTheme = (ResourceDictionary)Application.LoadComponent(new Uri("../Styles/DarkTheme.xaml", UriKind.Relative));

            if (Properties.Settings.Default.isDarkTheme)
                Resources.MergedDictionaries.Add(darkTheme);
            else
                Resources.MergedDictionaries.Add(lightTheme);
            Background = (Brush)Resources["WindowBackground"];
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var token = await _authService.AuthenticateAsync(loginTextBox.Text, passwordTextBox.Text);
                _authService.SaveToken(token);
                CheckAuthorize();
            }
            catch (Exception)
            {
                loginTextBox.Background = Brushes.Red;
                passwordTextBox.Background = Brushes.Red;
                MessageBox.Show("Произошла ошибка");
            }
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AuthorizeButton_Click(sender, e);
            }
        }

    }
}