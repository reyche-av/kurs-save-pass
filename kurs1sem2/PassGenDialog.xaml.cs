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

namespace kurs1sem2
{
    /// <summary>
    /// Логика взаимодействия для PassGenDialog.xaml
    /// </summary>
    public partial class PassGenDialog : Window
    {
        public PassGenDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PassField.Text = "";
            string gg = "qwertyuiopasdfghjklzxcvbnm";
            string wp = "";
            if (Upper.IsChecked == true) wp += gg.ToUpper();
            if (Lower.IsChecked == true) wp += gg;
            if (Special.IsChecked == true) wp += "!@$%^*()";
            if (Numbers.IsChecked == true) wp += "123456789";
            if (wp != "")
            {
                Random rnd = new Random();
                for (int i = 0; i < Length.Value; i++)
                PassField.Text += wp[rnd.Next(wp.Length)];
                PassField.Focus();
                PassField.CaretIndex = PassField.Text.Length;
            }
        }

        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label lb = (Label)e.Source;
            switch (lb.Content)
            {
                case "A - Z":
                    if (Upper.IsChecked == false) { Upper.IsChecked = true; } else { Upper.IsChecked = false; }
                    break;
                case "a - z":
                    if (Lower.IsChecked == false) { Lower.IsChecked = true; } else { Lower.IsChecked = false; }
                    break;
                case "0 - 9":
                    if (Numbers.IsChecked == false) { Numbers.IsChecked = true; } else { Numbers.IsChecked = false; }
                    break;
                case "Special characters":
                    if (Special.IsChecked == false) { Special.IsChecked = true; } else { Special.IsChecked = false; }
                    break;
            }
        }
    }
}
