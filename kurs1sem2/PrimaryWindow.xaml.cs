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
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Xml.Serialization;
using System.Configuration;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.ComponentModel;

namespace kurs1sem2
{
    /// <summary>
    /// Логика взаимодействия для PrimaryWindow.xaml
    /// </summary>
    public partial class PrimaryWindow : Window
    {
        public PrimaryWindow()
        {
            InitializeComponent();
            items = new ObservableCollection<Item>();
            if (Properties.Settings.Default.isDark == true) { GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x10, 0x13, 0x13)); } else { GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)); }
        }

        public ObservableCollection<Item> items;

        public class Fav_item
        {
            public string Name { get; set; }
            public string Set { get; set; }
        }

        public class Item : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string name;
            private string login;
            private string password;
            private string site;
            private string notes;
            private readonly string icon;
            private string favorite;
            private string added;
            private string lastModified;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            public string Login
            {
                get { return login; }
                set { login = value; }
            }
            public string Password
            {
                get { return password; }
                set { password = value; }
            }
            public string Site
            {
                get { return site; }
                set { site = value; }
            }
            public string Notes
            {
                get { return notes; }
                set { notes = value; }
            }
            public string Icon
            {
                get { return name[0].ToString().ToUpper(); }
            }
            public string Favorite
            {
                get { return favorite; }
                set { favorite = value;
                    OnPropertyChanged("Favorite");
                }
            }
            public string Added
            {
                get { return added; }
                set { added = value; }
            }
            public string LastModified
            {
                get { return lastModified; }
                set { lastModified = value; }
            }
            public Item(string name, string login, string password, string site, string notes, string icon, string favorite, string added, string lastModified)
            {
                this.name = name;
                this.login = login;
                this.password = password;
                this.site = site;
                this.notes = notes;
                this.icon = icon;
                this.favorite = favorite;
                this.added = added;
                this.lastModified = lastModified;
            }

            protected void OnPropertyChanged(string favorite)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(favorite));
            }
        }

        public string url;
        public bool isDark;
        public bool RemMe;
        public string userLogin;
        public string userPassword;
        public string userEmail;
        public string key;
        public bool _checked = true;
        public bool itemChanged = false;
        public byte[] AESkey;
        public byte[] AESIV;
        public string DataForSaveOnFile;

        public void JustWaiting()
        {
            Thread.Sleep(2000);
        }
        public async void ShowSnackMess()
        {
            if (SnackbarMess.IsActive == true)
            {
                SnackbarMess.IsActive = false;
            }
            else
            {
                SnackbarMess.IsActive = true;
                await Task.Run(() => JustWaiting());
                SnackbarMess.IsActive = false;
            }
        }

        private void WindowPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Owner.Close();
        }

        private void MiniBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ToggleMenu_Checked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (this.FindResource("OpenMenu") as Storyboard);
            sb.Begin();
            ListViewMenu1.IsEnabled = true; ListViewMenu2.IsEnabled = true;
            GridMenu_Backround.Visibility = Visibility.Visible;
        }

        private void ToggleMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (this.FindResource("CloseMenu") as Storyboard);
            sb.Begin();
            ListViewMenu1.IsEnabled = false; ListViewMenu2.IsEnabled = false;
            GridMenu_Backround.Visibility = Visibility.Collapsed;
        }

        private void GridMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ToggleMenu.IsChecked == false) { GridMenu_Backround.Visibility = Visibility.Visible; ToggleMenu_Checked(new object(), new RoutedEventArgs()); ToggleMenu.IsChecked=true; }
        }

        private void LogoutBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Owner.Show();
            this.Close();
        }

        private void AccountSettingsBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            item.IsSelected = false;
            GridMenu_Backround.Visibility = Visibility.Visible;
            Account dlg = new Account
            {
                Owner = this
            };
            dlg.TextBlockEmail.Text = userEmail;
            dlg.TextBlockLogin.Text = userLogin;
            dlg.url = url;
            dlg.key = key;
            dlg.userEmail = userEmail;
            dlg.userLogin = userLogin;
            dlg.userPassword = userPassword;
            dlg.ShowInTaskbar = false;
            dlg.ShowDialog();
            GridMenu_Backround.Visibility = Visibility.Collapsed;
        }

        private void DarkLightBtn_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (DarkLightBtn.IsChecked != true)
            { 
                isDark = false;
                GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
            }
            else
            {
                isDark = true;
                GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x13, 0x13, 0x13));
            }
            new PaletteHelper().SetLightDark(isDark);
            Properties.Settings.Default.isDark = isDark;
            Properties.Settings.Default.Save();
        }

        private void DataWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.RemMe)
            {
                Properties.Settings.Default.userLogin = userLogin;
                Properties.Settings.Default.userPassword = userPassword;
            }
            else
            {
                Properties.Settings.Default.userLogin = "";
                Properties.Settings.Default.userPassword = "";
            }
            Properties.Settings.Default.Save();
        }

        private void DataWindow_Loaded(object sender, RoutedEventArgs e)
        {
            labelLogin.Content = userLogin;
            labelEmail.Content = userEmail;
            if (Properties.Settings.Default.isDark == true) { GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x13, 0x13, 0x13)); DarkLightBtn.IsChecked = true; }
            else { GridMenu.Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)); DarkLightBtn.IsChecked = false; }

            AllitemsCount.Text = items.Count.ToString();
            var listItems = items.Where(i => i.Favorite == "HeartOff");
            FavoritesCount.Text = listItems.Count().ToString();
        }

        private void ChooseTheme_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChooseTheme.IsSelected = false;
            DarkLightBtn_IsCheckedChanged(new object(), new RoutedEventArgs());
            DarkLightBtn.IsChecked = !DarkLightBtn.IsChecked;
        }

        private void GridMenu_Backround_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleMenu_Unchecked(new object(), new RoutedEventArgs()); ToggleMenu.IsChecked = false; GridMenu_Backround.Visibility = Visibility.Collapsed;
        }

        private void PassGen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PassGenDialog dlg = new PassGenDialog
            {
                Owner = this
            };
            dlg.ShowInTaskbar = false;
            dlg.ShowDialog();
            PassGen.IsSelected = false;
        }

        private void Settings_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsDialog dlg = new SettingsDialog
            {
                Owner = this
            };
            dlg.ShowInTaskbar = false;
            dlg.ShowDialog();
            Settings.IsSelected = false;
        }

        private void ListBoxSource_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (listBoxSource.SelectedItem == null) return;
            var selectedItem = items.FirstOrDefault(p => p == listBoxSource.SelectedItem);
            if (selectedItem != null)
            {
                GridItemInfo.Visibility = Visibility.Visible;
                LabelChooseItem.Visibility = Visibility.Collapsed;
                ListItemName.Text = selectedItem.Name;
                ListItemLogin.Text = selectedItem.Login;
                passwordField_p.Password = selectedItem.Password;
                ListItemNotes.Text = selectedItem.Notes;
                LabelWebSite.Content = selectedItem.Site;
                AddedDate.Text = selectedItem.Added;
                LastModifiedDate.Text = selectedItem.LastModified;
            }
        }

        private void ListBoxSource_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxSource_MouseLeftButtonUp(sender, e);
        }

        private async Task<String> MakeDeleteAsync(String url)
        {
            String responseText = await Task.Run(() =>
            {
                string rsp;
                try
                {
                    HttpWebRequest DELETEWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse DELETEWebResponce = (HttpWebResponse)DELETEWebRequest.GetResponse();
                    using (StreamReader sr = new StreamReader(DELETEWebResponce.GetResponseStream()))
                    {
                        rsp = sr.ReadLine();
                    }
                    DELETEWebResponce.Close();
                    var check = rsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return check[0];
                }
                catch
                {
                    return "catch";
                    
                }
            });

            return responseText;
        }

        private async void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ButtonBase b = (System.Windows.Controls.Primitives.ButtonBase)sender;
            Item item = (Item)b.Tag;
            //------Работа-с-API----------
            //----НАЧАЛО-ШИФРОВАНИЯ-----
            byte[] encryptedData = null;
            try
            {
                Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                string dataToEncrypt = item.Name;
                encryptedData = MainWindow.EncryptStringToBytes_Aes(dataToEncrypt, AESkey, AESIV);
            }
            catch
            {
                MessageBox.Show("Ошибка в шифровании");
            }
            //-----КОНЕЦ-ШИФРОВАНИЯ-----
            String response = await MakeDeleteAsync(url + "DELETE&" + "data=" + Convert.ToBase64String(encryptedData) + "&" + "key=" + key);
            if (response == "200")
            {
                items.Remove(item);
                listBoxSource.UnselectAll();
                labelWhichItemSource.Content = "All items";
                listBoxSource.ItemsSource = items;
                GridItemInfo.Visibility = Visibility.Collapsed;
                LabelChooseItem.Visibility = Visibility.Visible;
                AllitemsCount.Text = items.Count.ToString();
                var listItems = items.Where(i => i.Favorite == "HeartOff");
                FavoritesCount.Text = listItems.Count().ToString();
                SnackbarMess.Message.Content = "Item successfully deleted";
                ShowSnackMess();
            }
            else if (response == "6")
            {
                SnackbarMess.Message.Content = "Can't delete item";
                ShowSnackMess();
            }
            else if (response == "catch")
            {
                SnackbarMess.Message.Content = "There is no internet connection";
                ShowSnackMess();
            }
        }

        private void GridItemInfoBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenWeb_btn.Focus();
        }

        private void PasswordField_p_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordField_t.Text = passwordField_p.Password;
            passwordField_p.Visibility = Visibility.Collapsed;
            passwordField_t.Visibility = Visibility.Visible;
            passwordField_t.Focus();
            passwordField_t.CaretIndex = passwordField_t.Text.Length;
        }

        private void PasswordField_t_LostFocus(object sender, RoutedEventArgs e)
        {
            passwordField_p.Visibility = Visibility.Visible;
            passwordField_t.Visibility = Visibility.Collapsed;
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ButtonBase b = (System.Windows.Controls.Primitives.ButtonBase)sender;
            Item item = (Item)b.Tag;
            GridMenu_Backround.Visibility = Visibility.Visible;
            var index = listBoxSource.SelectedIndex;
            CreateAndChangeWin dlg = new CreateAndChangeWin
            {
                Owner = this
            };
            dlg.b = b;
            dlg.ShowInTaskbar = false;
            dlg.NewItem = false;
            dlg.CreateOrChange.Content = "Edit: " + item.Name;
            dlg.itemName = item.Name;
            dlg.ListItemName.Text = item.Name;
            dlg.ListItemLogin.Text = item.Login;
            dlg.ListItemPassword.Text = item.Password;
            dlg.ListItemSite.Text = item.Site;
            dlg.ListItemNotes.Text = item.Notes;
            dlg.key = key;
            dlg.url = url;
            dlg.ShowDialog();
            if (itemChanged)
            {
                itemChanged = false;
                listBoxSource.ItemsSource = null;
                labelWhichItemSource.Content = "All items";
                SearchTextBox.Text = "";
                listBoxSource.ItemsSource = items;
                listBoxSource.UnselectAll();
                GridItemInfo.Visibility = Visibility.Collapsed;
                SnackbarMess.Message.Content = "Item successfully changed";
                ShowSnackMess();
            }
            GridMenu_Backround.Visibility = Visibility.Collapsed;
        }

        private void OpenWeb_btn_Click(object sender, RoutedEventArgs e)
        {
            Uri.TryCreate(LabelWebSite.Content.ToString(), UriKind.RelativeOrAbsolute, out Uri myUri);
            if (myUri.IsAbsoluteUri == true)
            {
                Process.Start(myUri.ToString());
            }
            else
            {
                SnackbarMess.Message.Content = "The link has an incorrect format";
                ShowSnackMess();
            }
        }

        private void ListItemNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListItemNotesQuantity.Content = 256 - ListItemNotes.Text.Length + "/256";
        }

        private void AddItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CreateAndChangeWin dlg = new CreateAndChangeWin
            {
                Owner = this
            };
            dlg.ShowInTaskbar = false;
            dlg.NewItem = true;
            dlg.key = key;
            dlg.url = url;
            dlg.ShowDialog();
            AddItem.IsSelected = false;
        }

        private async Task<String> MakeFavoriteItAsync(String url, String dataPost)
        {
            String responseText = await Task.Run(() =>
            {
                string rsp;
                //-------Данные-для-отправки-преобразуем-в-массив-байтов---------
                string data = dataPost;
                //MessageBox.Show(url + data);

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
                            rsp = reader.ReadToEnd();
                        }
                    }
                    PostWebResponce.Close();
                    var check = rsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return check[0];
                }
                catch
                {
                    return "catch";
                }
            });

            return responseText;
        }

        private async void PopupFavButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ButtonBase b = (System.Windows.Controls.Primitives.ButtonBase)sender;
            Item i = (Item)b.Tag;
            //----СОЗДАЕМ-JSON----
            Fav_item fav_item = new Fav_item();
            fav_item.Name = i.Name;
            if (i.Favorite == "Heart")
            {
                fav_item.Set = "HeartOff";
            }
            else
            {
                fav_item.Set = "Heart";
            }
            string json = JsonConvert.SerializeObject(fav_item, Formatting.Indented);
            //--------------------
            //----НАЧАЛО-ШИФРОВАНИЯ-----
            byte[] encryptedData = null;
            try
            {
                Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                string dataToEncrypt = json;
                encryptedData = MainWindow.EncryptStringToBytes_Aes(dataToEncrypt, AESkey, AESIV);
            }
            catch
            {
                MessageBox.Show("Ошибка в шифровании");
            }
            //-----КОНЕЦ-ШИФРОВАНИЯ-----
            String response = await MakeFavoriteItAsync(url + "FAVORITE", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
            if (i.Favorite == "Heart")
            {
                if (response == "200")
                {
                    i.Favorite = "HeartOff";
                    var listItems = items.Where(p => p.Favorite == "HeartOff");
                    FavoritesCount.Text = listItems.Count().ToString();
                    SnackbarMess.Message.Content = "Item successfully marked as favorite";
                    ShowSnackMess();
                }
                else if (response == "6")
                {
                    i.Favorite = "Heart";
                    SnackbarMess.Message.Content = "Can't mark item as favorite";
                    ShowSnackMess();
                    //b.IsChecked = !b.IsChecked;
                }
                else if (response == "catch")
                {
                    i.Favorite = "Heart";
                    SnackbarMess.Message.Content = "There is no internet connection";
                    ShowSnackMess();
                    //b.IsChecked = !b.IsChecked;
                }
            }
            else
            {
                if (response == "200")
                {
                    i.Favorite = "Heart";
                    var listFav = items.Where(p => p.Favorite == "HeartOff");
                    FavoritesCount.Text = listFav.Count().ToString();
                    SnackbarMess.Message.Content = "Item successfully removed from favorites";
                    ShowSnackMess();
                    if (labelWhichItemSource.Content.ToString() == "Favorites")
                    {
                        listBoxSource.ItemsSource = listFav;
                    }
                }
                else if (response == "6")
                {
                    i.Favorite = "HeartOff";
                    SnackbarMess.Message.Content = "Can't remove item from favorites";
                    ShowSnackMess();
                    //b.IsChecked = !b.IsChecked;
                }
                else if (response == "catch")
                {
                    i.Favorite = "HeartOff";
                    SnackbarMess.Message.Content = "There is no internet connection";
                    ShowSnackMess();
                    //b.IsChecked = !b.IsChecked;
                }
            }
            
        }

        private void Favorites_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listFav = items.Where(p => p.Favorite == "HeartOff");
            if (FavoritesCount.Text != "0")
            {
                labelWhichItemSource.Content = "Favorites";
                listBoxSource.ItemsSource = listFav;
            }
            Favorites.IsSelected = false;
        }

        private void AllItems_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            labelWhichItemSource.Content = "All items";
            listBoxSource.ItemsSource = items;
            AllItems.IsSelected = false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text == "")
            {
                labelWhichItemSource.Content = "All items";
                listBoxSource.ItemsSource = items;
            }
            else
            {
                var listSearch = items.Where(p => (p.Name.ToLower().Contains(SearchTextBox.Text.ToLower())) || (p.Site.ToLower().Contains(SearchTextBox.Text.ToLower()))); 
                labelWhichItemSource.Content = "Search \"" + SearchTextBox.Text + "\"";
                listBoxSource.ItemsSource = listSearch;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ButtonBase b = (System.Windows.Controls.Primitives.ButtonBase)sender;
            Item i = (Item)b.Tag;
            MessageBox.Show(i.Name);
        }

    }
}
