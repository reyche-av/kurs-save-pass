using MaterialDesignThemes.Wpf;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace kurs1sem2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Brush ColorErrorOn = new SolidColorBrush(Color.FromArgb(0xff, 244, 65, 51));

        public string APIurl = "https://kursovaya19.000webhostapp.com/api.php?action=";

        public string RSApublicKey = null;

        public string checkError;

        public byte[] AESkey, AESIV;

        public class DataAuthentication
        {
            public string AESkey { get; set; }
            public string AESIV { get; set; }
            public string Login { get; set; }
            public string Pass { get; set; }
        }

        public class StrokeData
        {
            public string Name { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string Site { get; set; }
            public string Notes { get; set; }
            public string Favorite { get; set; }
            public string Added { get; set; }
            public string LastModified { get; set; }
        }

        public class NewAccount
        {
            public string Email { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string AESkey { get; set; }
            public string AESIV { get; set; }
        }

        private void SnackbarMessBtn_ActionClick(object sender, RoutedEventArgs e)
        {
            SnackbarMess.IsActive = false;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Login_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            CheckRemMe.Focus();
        }

        private void TextBoxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            IconAccount.Foreground=(Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
            LabelLoginErr.Visibility = Visibility.Hidden;
            LabelError.Visibility = Visibility.Hidden;
        }

        private void TextBoxPass_GotFocus(object sender, RoutedEventArgs e)
        {
            IconKey.Foreground=(Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
            LabelPassErr.Visibility = Visibility.Hidden;
            LabelError.Visibility = Visibility.Hidden;
        }

        private void TextBoxLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            IconAccount.Foreground = Brushes.Gray;
        }

        private void TextBoxPass_LostFocus(object sender, RoutedEventArgs e)
        {
            IconKey.Foreground = Brushes.Gray;
        }

        public async void ButtonSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxLogin.Text == "" || TextBoxPass.Password == "")
            {
                if (TextBoxLogin.Text == "")
                {
                    LabelLoginErr.Visibility = Visibility.Visible;
                    IconAccount.Foreground = ColorErrorOn;
                }
                if (TextBoxPass.Password == "")
                {
                    LabelPassErr.Visibility = Visibility.Visible;
                    IconKey.Foreground = ColorErrorOn;
                }
            }
            else
            {
                //------Работа-с-API----------
                GridLoading.Visibility = Visibility.Visible;
                ProgressLoading.IsIndeterminate = true;
                //----НАЧАЛО-ШИФРОВАНИЯ-----
                byte[] dataToEncrypt = null;
                string json = null;
                try
                {
                    Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                    AESkey = myAes.Key; // Получаем ключ и вектор инициализации
                    AESIV = myAes.IV;
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();// Создаем новый объект класса RSA
                    
                    RSA.FromXmlString(RSApublicKey);

                    dataToEncrypt = Encoding.UTF8.GetBytes(Convert.ToBase64String(AESkey));
                    byte[] encryptedDataAESkey = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false); // Зашифровываем ключ AES, вектор инициализации, логин и пароль, все с новой строки
                    dataToEncrypt = Encoding.UTF8.GetBytes(Convert.ToBase64String(AESIV));
                    byte[] encryptedDataAESIV = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false);
                    byte[] encryptedDataLogin = EncryptStringToBytes_Aes(TextBoxLogin.Text, AESkey, AESIV);
                    byte[] encryptedDataPass = EncryptStringToBytes_Aes(TextBoxPass.Password, AESkey, AESIV);
                    
                    //----СОЗДАЕМ-JSON----
                    DataAuthentication data = new DataAuthentication();
                    data.AESkey = Convert.ToBase64String(encryptedDataAESkey);
                    data.AESIV = Convert.ToBase64String(encryptedDataAESIV);
                    data.Login = Convert.ToBase64String(encryptedDataLogin);
                    data.Pass = Convert.ToBase64String(encryptedDataPass);
                    json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    //--------------------
                }
                catch
                {
                    MessageBox.Show("Ошибка в шифровании");
                }
                //-----КОНЕЦ-ШИФРОВАНИЯ-----
                PrimaryWindow PrimeWin = await MakeAuthenticationAsyncPOST(APIurl + "authentication", "data=" + json);
                if (checkError == "2") //-----логин-или-пароль-неверен-----
                {
                    GridLoading.Visibility = Visibility.Collapsed;
                    ProgressLoading.IsIndeterminate = false;
                    LabelError.Content = "Incorrect login or password";
                    LabelError.Visibility = Visibility.Visible;
                }
                else if (checkError == "1")
                {
                    GridLoading.Visibility = Visibility.Collapsed;
                    ProgressLoading.IsIndeterminate = false;
                }
                else if (checkError == "catch")
                {
                    GridLoading.Visibility = Visibility.Collapsed;
                    ProgressLoading.IsIndeterminate = false;
                    LabelError.Content = "There is no internet connection";
                    LabelError.Visibility = Visibility.Visible;
                }
                else if (checkError == "9")
                {
                    GridLoading.Visibility = Visibility.Collapsed;
                    ProgressLoading.IsIndeterminate = false;
                    LabelError.Content = "Confirm your email address";
                    LabelError.Visibility = Visibility.Visible;
                }
                else
                {
                    PrimeWin.listBoxSource.ItemsSource = PrimeWin.items;

                    if (CheckRemMe.IsChecked == true)
                    {
                        //PrimeWin.RemMe = true;
                        Properties.Settings.Default.RemMe = true;
                        Properties.Settings.Default.Save();
                    }
                    LabelError.Visibility = Visibility.Hidden;
                    PrimeWin.Show();
                    this.Hide();
                    GridLoading.Visibility = Visibility.Collapsed;
                    ProgressLoading.IsIndeterminate = false;
                    TextBoxLogin.Text = "";
                    TextBoxPass.Password = "";
                    CheckRemMe.IsChecked = false;
                }
            }
        }

        private async Task<PrimaryWindow> MakeAuthenticationAsyncPOST(string url, string dataPost)
        {
            PrimaryWindow PrimeWin = new PrimaryWindow
            {
                Owner = this
            };

            PrimaryWindow responseText = await Task.Run(() =>
            {
                string rsp;
                string[] check;
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
                        using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("Windows-1251")))
                        {
                            rsp = reader.ReadToEnd();
                        }
                    }
                    PostWebResponce.Close();
                    using (StringReader str = new StringReader(rsp))
                    {
                        var error = str.ReadLine();
                        check = error.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (check[0] == "2" || check[0] == "1" || check[0] == "9")
                    {
                        checkError = check[0];
                    }
                    else
                    {
                        //Расшифровка
                        string roundtripAES = DecryptStringFromBytes_Aes(Encoding.Default.GetBytes(rsp), AESkey, AESIV);
                        StringReader rspStrings = new StringReader(roundtripAES);
                        rsp = rspStrings.ReadLine();
                        check = rsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        while ((rsp = rspStrings.ReadLine()) != null)
                        {
                            StrokeData result = JsonConvert.DeserializeObject<StrokeData>(rsp);
                            PrimeWin.items.Add(new PrimaryWindow.Item(result.Name, result.Login, result.Password, result.Site, result.Notes, "", result.Favorite, result.Added, result.LastModified));
                        }
                        PrimeWin.AESkey = AESkey;
                        PrimeWin.AESIV = AESIV;
                        PrimeWin.key = check[0];
                        PrimeWin.userLogin = check[1];
                        PrimeWin.userEmail = check[2];
                        PrimeWin.userPassword = TextBoxPass.Password;
                        PrimeWin.url = APIurl;
                        PrimeWin.DataForSaveOnFile = roundtripAES;
                        checkError = "";
                        rspStrings.Dispose();
                    }
                }
                catch
                {
                    checkError = "catch";
                }
                return PrimeWin;
            });
            return responseText;
        }

        private void TextBoxLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBoxPass.Focus();
            }
        }

        private void TextBoxPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonSignIn.Focus();
                ButtonSignIn_Click(new object(), new RoutedEventArgs());
            }
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "PublicKey.rsa";
            byte[] dataToDecrypt = Convert.FromBase64String(File.ReadAllText(path));
            string EncryptedData = Encoding.UTF8.GetString(dataToDecrypt);
            RSApublicKey = EncryptedData;

            if (Properties.Settings.Default.RemMe == true)
            {
                TextBoxLogin.Text = Properties.Settings.Default.userLogin;
                TextBoxPass.Password = Properties.Settings.Default.userPassword;
                CheckRemMe.IsChecked = true;
            }
            new PaletteHelper().ReplacePrimaryColor(Properties.Settings.Default.Color);

            if (Properties.Settings.Default.Color != "bluegrey") { new PaletteHelper().ReplaceAccentColor(Properties.Settings.Default.Color); }
            new PaletteHelper().SetLightDark(Properties.Settings.Default.isDark);
        }

        private void DialogCreateAccount_Closing(object sender, DialogClosingEventArgs eventArgs)
        {
            Create_TextBoxEmail.Text = "";
            Create_TextBoxLogin.Text = "";
            Create_PasswordBoxPass.Password = "";
            Create_TextBoxPass.Text = "";
            Create_PasswordBoxPassConf.Password = "";
            Create_TextBoxPassConf.Text = "";
            Create_btnPassVisilibility.IsChecked = false;
            Create_TextBoxEmailError.Visibility = Visibility.Hidden;
            Create_TextBoxLoginError.Visibility = Visibility.Hidden;
            Create_TextBoxPassError.Visibility = Visibility.Hidden;
            Create_TextBoxPassConfError.Visibility = Visibility.Hidden;
            Create_IconEmail.Foreground = Brushes.Gray;
            Create_IconLogin.Foreground = Brushes.Gray;
            Create_IconPass.Foreground = Brushes.Gray;
            Create_IconPassConf.Foreground = Brushes.Gray;
        }

        private void Create_btnPassVisilibility_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (Create_btnPassVisilibility.IsChecked == true)
            {
                Create_btnPassVisilibilityIcon.Kind = PackIconKind.EyeOff;
                Create_TextBoxPass.Text = Create_PasswordBoxPass.Password;
                Create_TextBoxPassConf.Text = Create_PasswordBoxPassConf.Password;
                Create_PasswordBoxPass.Visibility = Visibility.Collapsed;
                Create_PasswordBoxPassConf.Visibility = Visibility.Collapsed;
                Create_TextBoxPass.Visibility = Visibility.Visible;
                Create_TextBoxPassConf.Visibility = Visibility.Visible;
            }
            else
            {
                Create_btnPassVisilibilityIcon.Kind = PackIconKind.Eye;
                Create_PasswordBoxPass.Visibility = Visibility.Visible;
                Create_PasswordBoxPassConf.Visibility = Visibility.Visible;
                Create_TextBoxPass.Visibility = Visibility.Collapsed;
                Create_TextBoxPassConf.Visibility = Visibility.Collapsed;
            }
        }

        private async void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            var chk_email = Create_Validation("email", Create_TextBoxEmail.Text);
            var chk_login = Create_Validation("login", Create_TextBoxLogin.Text);
            var chk_password = Create_Validation("password", Create_PasswordBoxPass.Password);
            var chk_passwordConf = Create_Validation("passwordConf", Create_PasswordBoxPass.Password, Create_PasswordBoxPassConf.Password);
            var result = chk_email && chk_login && chk_password && chk_passwordConf;
            if (result)
            {
                //----НАЧАЛО-ШИФРОВАНИЯ-----
                byte[] dataToEncrypt = null;
                string json = null;
                try
                {
                    Aes myAes = Aes.Create(); // Создаем новый объект класса Aes, уже здесь создается ключ и вектор инициализации IV
                    AESkey = myAes.Key; // Получаем ключ и вектор инициализации
                    AESIV = myAes.IV;

                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();// Создаем новый объект класса RSA
                    RSA.FromXmlString(RSApublicKey);

                    dataToEncrypt = Encoding.UTF8.GetBytes(Convert.ToBase64String(AESkey));
                    byte[] encryptedDataAESkey = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false); // Зашифровываем ключ AES, вектор инициализации, логин и пароль, все с новой строки
                    dataToEncrypt = Encoding.UTF8.GetBytes(Convert.ToBase64String(AESIV));
                    byte[] encryptedDataAESIV = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false);
                    byte[] encryptedDataEmail = EncryptStringToBytes_Aes(Create_TextBoxEmail.Text, AESkey, AESIV);
                    byte[] encryptedDataLogin = EncryptStringToBytes_Aes(Create_TextBoxLogin.Text, AESkey, AESIV);
                    byte[] encryptedDataPass = EncryptStringToBytes_Aes(Create_PasswordBoxPass.Password, AESkey, AESIV);

                    //----СОЗДАЕМ-JSON----
                    NewAccount ac = new NewAccount
                    {
                        Email = Convert.ToBase64String(encryptedDataEmail),
                        Login = Convert.ToBase64String(encryptedDataLogin),
                        Password = Convert.ToBase64String(encryptedDataPass),
                        AESkey = Convert.ToBase64String(encryptedDataAESkey),
                        AESIV = Convert.ToBase64String(encryptedDataAESIV)
                    };
                    json = JsonConvert.SerializeObject(ac, Formatting.Indented);
                    //--------------------
                }
                catch
                {
                    MessageBox.Show("Ошибка в шифровании");
                }
                Create_GridLoading.Visibility = Visibility.Visible;
                Create_ProgressLoading.IsIndeterminate = true;
                String response = await MakeNewAccountAsync(APIurl + "CREATE-ACCOUNT", "data=" + json);
                if (response == "200")
                {
                    /* TODO ( Все хорошо ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    SnackbarMess.Message.Content = "Проверьте почту там письмо";
                    SnackbarMess.IsActive = true;
                    DialogCreateAccount_Closing(null, null); DialogCreateAccount.IsOpen = false;

                }
                else if (response == "3")
                {
                    /* TODO ( Аккаунт с таким Емайлом и Логином уже есть ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    Create_TextBoxEmailError.Text = "Account with the same email address already exists"; Create_TextBoxEmailError.Visibility = Visibility.Visible;
                    Create_TextBoxLoginError.Text = "Account with the same login already exists"; Create_TextBoxLoginError.Visibility = Visibility.Visible;
                    Create_IconEmail.Foreground = ColorErrorOn;
                    Create_IconLogin.Foreground = ColorErrorOn;
                }
                else if (response == "4")
                {
                    /* TODO ( Аккаунт с таким Email уже есть ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    Create_TextBoxEmailError.Text = "Account with the same email address already exists"; Create_TextBoxEmailError.Visibility = Visibility.Visible;
                    Create_IconEmail.Foreground = ColorErrorOn;
                }
                else if (response == "5")
                {
                    /* TODO ( Аккаунт с таким Login уже есть ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    Create_TextBoxLoginError.Text = "Account with the same login already exists"; Create_TextBoxLoginError.Visibility = Visibility.Visible;
                    Create_IconLogin.Foreground = ColorErrorOn;
                }
                else if (response == "6")
                {
                    /* TODO ( Не удается создать запись ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    SnackbarMess.Message.Content = "Can't create account";
                    SnackbarMess.IsActive = true;
                    DialogCreateAccount.IsOpen = false;
                }
                else if (response == "catch")
                {
                    /* TODO ( Нет интернета ) */
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    LabelError.Content = "There is no internet connection";
                    LabelError.Visibility = Visibility.Visible;
                    DialogCreateAccount.IsOpen = false;
                }
                else
                {
                    Create_GridLoading.Visibility = Visibility.Collapsed;
                    Create_ProgressLoading.IsIndeterminate = false;
                    SnackbarMess.Message.Content = "Error on server";
                    SnackbarMess.IsActive = true;
                    DialogCreateAccount.IsOpen = false;
                }
            }
        }
        private bool Create_Validation(string field, string text, string pass = "")
        {
            bool result = false;
            switch (field)
            {
                case "email":
                    if (text=="")
                    {
                        Create_TextBoxEmailError.Text = "Field is required";
                        Create_TextBoxEmailError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconEmail.Foreground = ColorErrorOn;
                        break;
                    }
                    string pattern_email = @"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[;]{0,1}\s*)+$";
                    if (!Regex.Match(text, pattern_email).Success)
                    {
                        Create_TextBoxEmailError.Text = "Incorrect email";
                        Create_TextBoxEmailError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconEmail.Foreground = ColorErrorOn;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "login":
                    if (text.Length < 3 || text.Length > 20)
                    {
                        Create_TextBoxLoginError.Text = "Must contain 3 to 20 characters";
                        Create_TextBoxLoginError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconLogin.Foreground = ColorErrorOn;
                        break;
                    }
                    string pattern_login = @"^[A-Za-z0-9]+$";
                    if (!Regex.Match(text, pattern_login).Success)
                    {
                        Create_TextBoxLoginError.Text = "Login must contain only Latin letters";
                        Create_TextBoxLoginError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconLogin.Foreground = ColorErrorOn;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "password":
                    if (text.Length < 6)
                    {
                        Create_TextBoxPassError.Text = "Password must be at least 6 characters";
                        Create_TextBoxPassError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconPass.Foreground = ColorErrorOn;
                        break;
                    }
                    string pattern_pass = @"^[A-Za-z0-9]+$"; //УВЕЛИЧИТЬ СЛОЖНОСТЬ ВЫРАЖЕНИЯ ДЛЯ ПАРОЛЯ (ВСЕ ЕСТЬ В ГУГЛЕ)
                    if (!Regex.Match(text, pattern_pass).Success)
                    {
                        Create_TextBoxPassError.Text = "Password must contain only Latin letters and digits 0 to 9";
                        Create_TextBoxPassError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconPass.Foreground = ColorErrorOn;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "passwordConf":
                    if (pass == "")
                    {
                        Create_TextBoxPassConfError.Text = "Field is required";
                        Create_TextBoxPassConfError.Visibility = Visibility.Visible;
                        result = false;
                        Create_IconPassConf.Foreground = ColorErrorOn;
                        break;
                    }
                    if (text != pass)
                    {
                        Create_TextBoxPassConfError.Text = "Passwords must match";
                        Create_TextBoxPassConfError.Visibility = Visibility.Visible;
                        result = false; 
                        Create_IconPassConf.Foreground = ColorErrorOn;
                        break;
                    }
                    result = true;
                    break;
            }
            return result;
        }

        private async Task<String> MakeNewAccountAsync(string url, string dataPost)
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
                            rsp = reader.ReadToEnd();
                            check = rsp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

        private void Create_TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "Create_TextBoxEmail":
                    Create_IconEmail.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxEmailError.Visibility = Visibility.Hidden;
                    break;
                case "Create_TextBoxLogin":
                    Create_IconLogin.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxLoginError.Visibility = Visibility.Hidden;
                    break;
                case "Create_PasswordBoxPass":
                    Create_IconPass.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxPassError.Visibility = Visibility.Hidden;
                    break;
                case "Create_TextBoxPass":
                    Create_IconPass.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxPassError.Visibility = Visibility.Hidden;
                    break;
                case "Create_PasswordBoxPassConf":
                    Create_IconPassConf.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxPassConfError.Visibility = Visibility.Hidden;
                    break;
                case "Create_TextBoxPassConf":
                    Create_IconPassConf.Foreground = (Brush)Application.Current.Resources["PrimaryHueDarkBrush"];
                    Create_TextBoxPassConfError.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void Create_TextBoxes_LostFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "Create_TextBoxEmail":
                    Create_IconEmail.Foreground = Brushes.Gray;
                    break;
                case "Create_TextBoxLogin":
                    Create_IconLogin.Foreground = Brushes.Gray;
                    break;
                case "Create_PasswordBoxPass":
                    Create_IconPass.Foreground = Brushes.Gray;
                    break;
                case "Create_TextBoxPass":
                    Create_IconPass.Foreground = Brushes.Gray;
                    break;
                case "Create_PasswordBoxPassConf":
                    Create_IconPassConf.Foreground = Brushes.Gray;
                    break;
                case "Create_TextBoxPassConf":
                    Create_IconPassConf.Foreground = Brushes.Gray;
                    break;
            }
        }

        // Методы для AES и RSA шифрования
        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Создаем новый объект RSACryptoServiceProvider
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Импортируем публичный ключ
                    RSA.ImportParameters(RSAKeyInfo);
                    //Шифруем данные  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Показываем ошибку
            catch (CryptographicException e)
            {
                MessageBox.Show(e.Message);

                return null;
            }
        }
        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Создаем новый объект RSACryptoServiceProvider
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Импортируем приватный ключ
                    RSA.ImportParameters(RSAKeyInfo);

                    //Расшифровываем данные
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Показываем ошибку
            catch (CryptographicException e)
            {
                MessageBox.Show(e.ToString());

                return null;
            }
        }
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Проверяем аргументы
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Создаем новый объект класса Aes, присвоив ему ключ и вектор инициализации IV
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Создаем объект-шифратор для алгоритма симметричного шифрования
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Возвращаем заишфрованные данные из потока
            return encrypted;
        }
        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            // Объявляем переменную для расшифрованного текста
            string plaintext = null;
            // Создаем новый объект класса Aes, присвоив ему ключ и вектор инициализации IV
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Создаем объект-дешифратор для алгоритма симметричного шифрования
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            // Возвращаем расшифрованные данные из потока
            return plaintext;
        }
        // Пример использования:
        //byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
        //string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);
        //dataToEncrypt - информация для шифрования RSA.ExportParameters(false) - открытый ключ RSA.ExportParameters(true) - приватный ключ
        //encryptedData = RSAEncrypt(dataToEncrypt, RSA.ExportParameters(false), false);
        //encryptedData - зашифрованная информация RSA.ExportParameters(false) - открытый ключ RSA.ExportParameters(true) - приватный ключ
        //decryptedData = RSADecrypt(encryptedData, RSA.ExportParameters(true), false);
    }
}
