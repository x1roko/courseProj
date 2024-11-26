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
    /// Логика взаимодействия для ShowOrderWindow.xaml
    /// </summary>
    public partial class ShowOrderWindow : Window, IThemeable
    {
        public ShowOrderWindow()
        {
            InitializeComponent();
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
    }
}
