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
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace kurs1sem2
{
    /// <summary>
    /// Логика взаимодействия для CreateAndChangeWin.xaml
    /// </summary>
    public partial class CreateAndChangeWin : Window
    {
        public CreateAndChangeWin()
        {
            InitializeComponent();
        }

        public string key;
        public string url;
        public string itemName;
        public bool NewItem;
        public string AddOrModDate;
        public System.Windows.Controls.Primitives.ButtonBase b;

        class Item
        {
            public string Re_name { get; set; }
            public string Name { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Site { get; set; }
            public string Notes { get; set; }
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[#&]", "");
            textboxSender.SelectionStart = cursorPosition;
            
        }

        private void ListItemNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter_TextChanged(sender, e);
            ListItemNotesQuantity.Content = 256 - ListItemNotes.Text.Length + "/256";
        }

        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task<String> MakeNewOrEditItemAsync(String url, String dataPost)
        {
            String responseText = await Task.Run(() =>
            {
                string[] check;
                string rsp;
                //-------Данные-для-отправки-преобразуем-в-массив-байтов---------
                string data = dataPost;
                byte[] dataByteArray = Encoding.UTF8.GetBytes(data);
                try
                {
                    HttpWebRequest PostWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    PostWebRequest.Method = "POST";
                    //-------Устанавливаем-тип-содержимого---------
                    PostWebRequest.ContentType = "application/x-www-form-urlencoded";
                    //-------Устанавливаем-заголовок-Content-Length-запроса-свойство-ContentLength--------
                    PostWebRequest.ContentLength = dataByteArray.Length;
                    //-------Записываем-данные-в-поток-запроса------
                    using (Stream dataStream = PostWebRequest.GetRequestStream())
                    {
                        dataStream.Write(dataByteArray, 0, dataByteArray.Length);
                    }
                    //-------Отправляем-данные-и-ждем-ответ-------
                    HttpWebResponse PostWebResponce = (HttpWebResponse)PostWebRequest.GetResponse();
                    using (Stream stream = PostWebResponce.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            rsp = reader.ReadLine();
                            check = rsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            AddOrModDate = reader.ReadToEnd();
                        }
                    }
                    PostWebResponce.Close();
                    return check[0];
                }
                catch
                {
                    return "catch";
                }
            });

            return responseText;
        }

        private async void ACCEPT_Click(object sender, RoutedEventArgs e)
        {
            PrimaryWindow PrimeWin = this.Owner as PrimaryWindow;
            if (ListItemName.Text.Length < 3)
            {
                ListItemNameErr.Visibility = Visibility.Visible;
                return;
            }
            if (NewItem)
            {
                //----СОЗДАЕМ-JSON----
                Item newitem = new Item();
                newitem.Name = ListItemName.Text;
                newitem.Login = ListItemLogin.Text;
                newitem.Password = ListItemPassword.Text;
                newitem.Site = ListItemSite.Text;
                newitem.Notes = ListItemNotes.Text;
                string json = JsonConvert.SerializeObject(newitem, Formatting.Indented);
                //--------------------
                //----НАЧАЛО-ШИФРОВАНИЯ-----
                byte[] encryptedData = null;
                try
                {
                    Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                    string dataToEncrypt = json;
                    encryptedData = MainWindow.EncryptStringToBytes_Aes(dataToEncrypt, PrimeWin.AESkey, PrimeWin.AESIV);
                }
                catch
                {
                    MessageBox.Show("Ошибка в шифровании");
                }
                //-----КОНЕЦ-ШИФРОВАНИЯ-----
                GridLoading.Visibility = Visibility.Visible;
                ProgressLoading.IsIndeterminate = true;
                String response = await MakeNewOrEditItemAsync(url + "POST", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "201")
                {
                    PrimeWin.items.Add(new PrimaryWindow.Item(ListItemName.Text, ListItemLogin.Text, ListItemPassword.Text, ListItemSite.Text, ListItemNotes.Text, "", "Heart", AddOrModDate, AddOrModDate));
                    PrimeWin.AllitemsCount.Text = PrimeWin.items.Count.ToString();
                    PrimeWin.SnackbarMess.Message.Content = "Item successfully created";
                    PrimeWin.ShowSnackMess();
                    Close();
                }
                else if (response == "3")
                {
                    PrimeWin.SnackbarMess.Message.Content = "Item with the same name already exists";
                    PrimeWin.ShowSnackMess();
                }
                else if (response == "4")
                {
                    PrimeWin.SnackbarMess.Message.Content = "Can't create item";
                    PrimeWin.ShowSnackMess();
                }
                else if (response == "catch")
                {
                    PrimeWin.SnackbarMess.Message.Content = "There is no internet connection";
                    PrimeWin.ShowSnackMess();
                }
                GridLoading.Visibility = Visibility.Collapsed;
                ProgressLoading.IsIndeterminate = false;
            }
            else
            {
                //----СОЗДАЕМ-JSON----
                Item re_item = new Item();
                re_item.Re_name = ListItemName.Text;
                re_item.Name = itemName;
                re_item.Login = ListItemLogin.Text;
                re_item.Password = ListItemPassword.Text;
                re_item.Site = ListItemSite.Text;
                re_item.Notes = ListItemNotes.Text;
                string json = JsonConvert.SerializeObject(re_item, Formatting.Indented);
                //--------------------
                //----НАЧАЛО-ШИФРОВАНИЯ-----
                byte[] encryptedData = null;
                try
                {
                    Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                    string dataToEncrypt = json;
                    //string dataToEncrypt = itemName + " " + ListItemName.Text + " " + ListItemLogin.Text + " " + ListItemPassword.Text + " " + ListItemSite.Text + " " + ListItemNotes.Text;
                    encryptedData = MainWindow.EncryptStringToBytes_Aes(dataToEncrypt, PrimeWin.AESkey, PrimeWin.AESIV);
                }
                catch
                {
                    MessageBox.Show("Ошибка в шифровании");
                }
                //-----КОНЕЦ-ШИФРОВАНИЯ-----
                GridLoading.Visibility = Visibility.Visible;
                ProgressLoading.IsIndeterminate = true;
                String response = await MakeNewOrEditItemAsync(url + "PUT", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "200")
                {
                    PrimaryWindow.Item changedItem = (PrimaryWindow.Item)b.Tag;
                    changedItem.Name = ListItemName.Text;
                    changedItem.Login = ListItemLogin.Text;
                    changedItem.Password = ListItemPassword.Text;
                    changedItem.Site = ListItemSite.Text;
                    changedItem.Notes = ListItemNotes.Text;
                    changedItem.LastModified = AddOrModDate;
                    PrimeWin.itemChanged = true;
                    Close();
                }
                else if (response == "5")
                {
                    PrimeWin.SnackbarMess.Message.Content = "Can't change item";
                    PrimeWin.ShowSnackMess();
                }
                else if (response == "catch")
                {
                    PrimeWin.SnackbarMess.Message.Content = "There is no internet connection";
                    PrimeWin.ShowSnackMess();
                }
                GridLoading.Visibility = Visibility.Collapsed;
                ProgressLoading.IsIndeterminate = false;
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CANCEL.Focus();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var ErrorText = (TextBlock)this.FindName(textboxSender.Name + "Err");
            ErrorText.Visibility = Visibility.Hidden;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var ErrorText = (TextBlock)this.FindName(textboxSender.Name + "Err");
            if (textboxSender.Text.Length > 30)
            {
                textboxSender.Text = textboxSender.Text.Remove(30, textboxSender.Text.Length - 30);
                ErrorText.Visibility = Visibility.Visible;
            }
        }
    }
}
