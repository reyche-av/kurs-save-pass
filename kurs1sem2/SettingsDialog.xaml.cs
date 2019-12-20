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
using MaterialDesignThemes.Wpf;

namespace kurs1sem2
{
    /// <summary>
    /// Логика взаимодействия для SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public void HiddenIcon()
        {
            switch (Properties.Settings.Default.Color)
            {
                case "green":
                    GreenBtnIcon.Visibility = Visibility.Hidden;
                    break;
                case "blue":
                    BlueBtnIcon.Visibility = Visibility.Hidden;
                    break;
                case "bluegrey":
                    BluegreyBtnIcon.Visibility = Visibility.Hidden;
                    break;
                case "orange":
                    OrangeBtnIcon.Visibility = Visibility.Hidden;
                    break;
                case "purple":
                    PurpleBtnIcon.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (Properties.Settings.Default.Color)
            {
                case "green":
                    GreenBtnIcon.Visibility = Visibility.Visible;
                    break;
                case "blue":
                    BlueBtnIcon.Visibility = Visibility.Visible;
                    break;
                case "bluegrey":
                    BluegreyBtnIcon.Visibility = Visibility.Visible;
                    break;
                case "orange":
                    OrangeBtnIcon.Visibility = Visibility.Visible;
                    break;
                case "purple":
                    PurpleBtnIcon.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void GreenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GreenBtnIcon.Visibility != Visibility.Visible)
            {
                HiddenIcon();
                new PaletteHelper().ReplacePrimaryColor("green");

                new PaletteHelper().ReplaceAccentColor("green");
                Properties.Settings.Default.Color = "green";
                GreenBtnIcon.Visibility = Visibility.Visible;
            }
        }

        private void BlueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BlueBtnIcon.Visibility != Visibility.Visible)
            {
                HiddenIcon();
                new PaletteHelper().ReplacePrimaryColor("blue");

                new PaletteHelper().ReplaceAccentColor("blue");
                Properties.Settings.Default.Color = "blue";
                BlueBtnIcon.Visibility = Visibility.Visible;
            }
        }

        private void BluegreyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BluegreyBtnIcon.Visibility != Visibility.Visible)
            {
                HiddenIcon();
                new PaletteHelper().ReplacePrimaryColor("bluegrey");
                new PaletteHelper().ReplaceAccentColor("indigo");
                Properties.Settings.Default.Color = "bluegrey";
                BluegreyBtnIcon.Visibility = Visibility.Visible;
            }
        }

        private void OrangeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (OrangeBtnIcon.Visibility != Visibility.Visible)
            {
                HiddenIcon();
                new PaletteHelper().ReplacePrimaryColor("orange");
                new PaletteHelper().ReplaceAccentColor("orange");
                Properties.Settings.Default.Color = "orange";
                OrangeBtnIcon.Visibility = Visibility.Visible;
            }
        }

        private void PurpleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PurpleBtnIcon.Visibility != Visibility.Visible)
            {
                HiddenIcon();
                new PaletteHelper().ReplacePrimaryColor("purple");
                new PaletteHelper().ReplaceAccentColor("purple");
                Properties.Settings.Default.Color = "purple";
                PurpleBtnIcon.Visibility = Visibility.Visible;
            }
        }

        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
