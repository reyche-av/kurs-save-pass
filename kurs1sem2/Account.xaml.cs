using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Account.xaml
    /// </summary>
    public partial class Account : Window
    {
        public Account()
        {
            InitializeComponent();
        }
        public string url;
        public string key;
        public string userEmail;
        public string userLogin;
        public string userPassword;
        private void Account_SnackbarMessBtn_ActionClick(object sender, RoutedEventArgs e)
        {
            Account_SnackbarMess.IsActive = false;
        }
        private void CANCEL_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //------------AREA-EMAIL-DIALOG------------
        class EmailChange
        {
            public string Email { get; set; }
        }
        private async void EmailDialog_SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool chk_email = EmailDialog_Validation("email", EmailDialog_EmailBox.Text);
            bool chk_pass = EmailDialog_Validation("pass", EmailDialog_PasswordBox.Password);
            bool result = chk_email && chk_pass;
            if (result)
            {
                EmailDialog_GridLoading.Visibility = Visibility.Visible;
                EmailDialog_ProgressLoading.IsIndeterminate = true;
                PrimaryWindow PrimeWin = this.Owner as PrimaryWindow;
                //----СОЗДАЕМ-JSON----
                EmailChange item = new EmailChange();
                item.Email = EmailDialog_EmailBox.Text;
                string json = JsonConvert.SerializeObject(item, Formatting.Indented);
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
                String response = await EmailDialog_ChangeEmailAsync(url + "CHANGE-EMAIL", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "200")
                {
                    /* TODO ( Все хорошо ) */
                    if (Properties.Settings.Default.RemMe == true)
                    {
                        Properties.Settings.Default.RemMe = false;
                        Properties.Settings.Default.Save();
                    }
                    Owner.Owner.Show();
                    Owner.Close();
                    Close();
                }
                else if (response == "4")
                {
                    /* TODO ( Аккаунт с таким Email уже есть ) */
                    Account_SnackbarMess.Message.Content = "Account with the same email address already exists";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "6")
                {
                    /* TODO ( Не удается создать запись ) */
                    Account_SnackbarMess.Message.Content = "Can't change email address";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "catch")
                {
                    /* TODO ( Нет интернета ) */
                    Account_SnackbarMess.Message.Content = "There is no internet connection";
                    Account_SnackbarMess.IsActive = true;
                }
                else
                {
                    /* TODO ( ошибка на сервере ) */
                    Account_SnackbarMess.Message.Content = "Error on server";
                    Account_SnackbarMess.IsActive = true;
                }
                EmailDialog_GridLoading.Visibility = Visibility.Collapsed;
                EmailDialog_ProgressLoading.IsIndeterminate = false;
            }
        }
        private bool EmailDialog_Validation(string field, string text)
        {
            bool result = false;
            switch (field)
            {
                case "email":
                    if (text == "")
                    {
                        EmailDialog_EmailBoxError.Text = "Field is required";
                        EmailDialog_EmailBoxError.Visibility = Visibility.Visible;
                        result = false;
                        return result;
                    }
                    string pattern_email = @"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[;]{0,1}\s*)+$";
                    if (!Regex.Match(text, pattern_email).Success)
                    {
                        EmailDialog_EmailBoxError.Text = "Incorrect email";
                        EmailDialog_EmailBoxError.Visibility = Visibility.Visible;
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "pass":
                    if (text.Length == 0)
                    {
                        EmailDialog_PasswordBoxError.Text = "Field is required";
                        EmailDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    if (userPassword != text)
                    {
                        EmailDialog_PasswordBoxError.Text = "Incorrect password";
                        EmailDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }
        private void EmailDialog_TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "EmailDialog_EmailBox":
                    EmailDialog_EmailBoxError.Visibility = Visibility.Hidden;
                    break;
                case "EmailDialog_PasswordBox":
                    EmailDialog_PasswordBoxError.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private async Task<String> EmailDialog_ChangeEmailAsync(String url, String dataPost)
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
        private void Email_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            EmailDialog_EmailBox.Text = "";
            EmailDialog_EmailBoxError.Visibility = Visibility.Hidden;
            EmailDialog_PasswordBox.Password = "";
            EmailDialog_PasswordBoxError.Visibility = Visibility.Hidden;
        }
        //-----------AREA-EMAIL-DIALOG-END---------

        //------------AREA-LOGIN-DIALOG------------
        class LoginChange
        {
            public string Login { get; set; }
        }
        private async void LoginDialog_SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool chk_login= LoginDialog_Validation("login", LoginDialog_LoginBox.Text);
            bool chk_pass = LoginDialog_Validation("pass", LoginDialog_PasswordBox.Password);
            bool result = chk_login && chk_pass;
            if (result)
            {
                LoginDialog_GridLoading.Visibility = Visibility.Visible;
                LoginDialog_ProgressLoading.IsIndeterminate = true;
                PrimaryWindow PrimeWin = this.Owner as PrimaryWindow;
                //----СОЗДАЕМ-JSON----
                LoginChange item = new LoginChange();
                item.Login = LoginDialog_LoginBox.Text;
                string json = JsonConvert.SerializeObject(item, Formatting.Indented);
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
                String response = await LoginDialog_ChangeLoginAsync(url + "CHANGE-LOGIN", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "200")
                {
                    /* TODO ( Все хорошо ) */
                    if (Properties.Settings.Default.RemMe == true)
                    {
                        Properties.Settings.Default.RemMe = false;
                        Properties.Settings.Default.Save();
                    }
                    Owner.Owner.Show();
                    Owner.Close();
                    Close();
                }
                else if (response == "4")
                {
                    /* TODO ( Аккаунт с таким Login уже есть ) */
                    Account_SnackbarMess.Message.Content = "Account with the same login already exists";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "6")
                {
                    /* TODO ( Не удается создать запись ) */
                    Account_SnackbarMess.Message.Content = "Can't change login";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "catch")
                {
                    /* TODO ( Нет интернета ) */
                    Account_SnackbarMess.Message.Content = "There is no internet connection";
                    Account_SnackbarMess.IsActive = true;
                }
                else
                {
                    /* TODO ( ошибка на сервере ) */
                    Account_SnackbarMess.Message.Content = "Error on server";
                    Account_SnackbarMess.IsActive = true;
                }
                LoginDialog_GridLoading.Visibility = Visibility.Collapsed;
                LoginDialog_ProgressLoading.IsIndeterminate = false;
            }
        }
        private bool LoginDialog_Validation(string field, string text)
        {
            bool result = false;
            switch (field)
            {
                case "login":
                    if (text.Length < 3 || text.Length > 20)
                    {
                        LoginDialog_LoginBoxError.Text = "Must contain 3 to 20 characters";
                        LoginDialog_LoginBoxError.Visibility = Visibility.Visible;
                        result = false;
                        return result;
                    }
                    string pattern_login = @"^[A-Za-z0-9]+$";
                    if (!Regex.Match(text, pattern_login).Success)
                    {
                        LoginDialog_LoginBoxError.Text = "Login must contain only Latin letters";
                        LoginDialog_LoginBoxError.Visibility = Visibility.Visible;
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "pass":
                    if (text.Length == 0)
                    {
                        LoginDialog_PasswordBoxError.Text = "Field is required";
                        LoginDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    if (userPassword != text)
                    {
                        LoginDialog_PasswordBoxError.Text = "Incorrect password";
                        LoginDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }
        private void LoginDialog_TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "LoginDialog_LoginBox":
                    LoginDialog_LoginBoxError.Visibility = Visibility.Hidden;
                    break;
                case "LoginDialog_PasswordBox":
                    LoginDialog_PasswordBoxError.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private async Task<String> LoginDialog_ChangeLoginAsync(String url, String dataPost)
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
        private void Login_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            LoginDialog_LoginBox.Text = "";
            LoginDialog_LoginBoxError.Visibility = Visibility.Hidden;
            LoginDialog_PasswordBox.Password = "";
            LoginDialog_PasswordBoxError.Visibility = Visibility.Hidden;
        }
        //-----------AREA-LOGIN-DIALOG-END---------

        //------------AREA-PASSWORD-DIALOG------------
        class PasswordChange
        {
            public string Password { get; set; }
        }
        private async void PasswordDialog_SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool chk_curPass= PasswordDialog_Validation("cur-pass", PasswordDialog_CurrentPasswordBox.Password);
            bool chk_pass = PasswordDialog_Validation("pass", PasswordDialog_PasswordBox.Password);
            bool chk_conPass = PasswordDialog_Validation("con-pass", PasswordDialog_ConfirmPasswordBox.Password, PasswordDialog_PasswordBox.Password);
            bool result = chk_curPass && chk_pass && chk_conPass;
            if (result)
            {
                PasswordDialog_GridLoading.Visibility = Visibility.Visible;
                PasswordDialog_ProgressLoading.IsIndeterminate = true;
                PrimaryWindow PrimeWin = this.Owner as PrimaryWindow;
                //----СОЗДАЕМ-JSON----
                PasswordChange item = new PasswordChange();
                item.Password = PasswordDialog_PasswordBox.Password;
                string json = JsonConvert.SerializeObject(item, Formatting.Indented);
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
                String response = await PasswordDialog_ChangePasswordAsync(url + "CHANGE-PASSWORD", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "200")
                {
                    /* TODO ( Все хорошо ) */
                    if (Properties.Settings.Default.RemMe == true)
                    {
                        Properties.Settings.Default.RemMe = false;
                        Properties.Settings.Default.Save();
                    }
                    Owner.Owner.Show();
                    Owner.Close();
                    Close();
                }
                else if (response == "6")
                {
                    /* TODO ( Не удается создать запись ) */
                    Account_SnackbarMess.Message.Content = "Can't change password";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "catch")
                {
                    /* TODO ( Нет интернета ) */
                    Account_SnackbarMess.Message.Content = "There is no internet connection";
                    Account_SnackbarMess.IsActive = true;
                }
                else
                {
                    /* TODO ( ошибка на сервере ) */
                    Account_SnackbarMess.Message.Content = "Error on server";
                    Account_SnackbarMess.IsActive = true;
                }
                PasswordDialog_GridLoading.Visibility = Visibility.Collapsed;
                PasswordDialog_ProgressLoading.IsIndeterminate = false;
            }
        }
        private bool PasswordDialog_Validation(string field, string text, string pass = "")
        {
            bool result = false;
            switch (field)
            {
                case "cur-pass":
                    if (text.Length == 0)
                    {
                        PasswordDialog_CurrentPasswordBoxError.Text = "Field is required";
                        PasswordDialog_CurrentPasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    if (userPassword != text)
                    {
                        PasswordDialog_CurrentPasswordBoxError.Text = "Incorrect password";
                        PasswordDialog_CurrentPasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "pass":
                    if (text.Length < 6)
                    {
                        PasswordDialog_PasswordBoxError.Text = "Password must be at least 6 characters";
                        PasswordDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    string pattern_pass = @"^[A-Za-z0-9]+$";
                    if (!Regex.Match(text, pattern_pass).Success)
                    {
                        PasswordDialog_PasswordBoxError.Text = "Password must contain only Latin letters and digits 0 to 9";
                        PasswordDialog_PasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                    break;
                case "con-pass":
                    if (text == "")
                    {
                        PasswordDialog_ConfirmPasswordBoxError.Text = "Field is required";
                        PasswordDialog_ConfirmPasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    if (text != pass)
                    {
                        PasswordDialog_ConfirmPasswordBoxError.Text = "Passwords must match";
                        PasswordDialog_ConfirmPasswordBoxError.Visibility = Visibility.Visible;
                        result = false;
                        break;
                    }
                    result = true;
                    break;
            }
            
            return result;
        }
        private void PasswordDialog_TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "PasswordDialog_CurrentPasswordBox":
                    PasswordDialog_CurrentPasswordBoxError.Visibility = Visibility.Hidden;
                    break;
                case "PasswordDialog_PasswordBox":
                    PasswordDialog_PasswordBoxError.Visibility = Visibility.Hidden;
                    break;
                case "PasswordDialog_ConfirmPasswordBox":
                    PasswordDialog_ConfirmPasswordBoxError.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private async Task<String> PasswordDialog_ChangePasswordAsync(String url, String dataPost)
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
        private void Password_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            PasswordDialog_CurrentPasswordBox.Password = "";
            PasswordDialog_CurrentPasswordBoxError.Visibility = Visibility.Hidden;
            PasswordDialog_PasswordBox.Password = "";
            PasswordDialog_PasswordBoxError.Visibility = Visibility.Hidden;
            PasswordDialog_ConfirmPasswordBox.Password = "";
            PasswordDialog_ConfirmPasswordBoxError.Visibility = Visibility.Hidden;
        }
        //-----------AREA-PASSWORD-DIALOG-END---------

        //------------AREA-DELETE-ACCOUNT-DIALOG------------
        class AccountDelete
        {
            public string GoodBye { get; set; }
        }
        private async void DeleteAccountDialog_DELETEbtn_Click(object sender, RoutedEventArgs e)
        {
            bool chk_Pass= DeleteAccountDialog_Validation(DeleteAccountDialog_Password.Password);
            if (chk_Pass)
            {
                DeleteAccountDialog_GridLoading.Visibility = Visibility.Visible;
                DeleteAccountDialog_ProgressLoading.IsIndeterminate = true;
                PrimaryWindow PrimeWin = this.Owner as PrimaryWindow;
                //----СОЗДАЕМ-JSON----
                AccountDelete item = new AccountDelete();
                item.GoodBye = "Bye";
                string json = JsonConvert.SerializeObject(item, Formatting.Indented);
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
                String response = await DeleteAccountDialog_DeleteAccountAsync(url + "DELETE-ACCOUNT", "data=" + Convert.ToBase64String(encryptedData) + "&key=" + key);
                if (response == "200")
                {
                    /* TODO ( Все хорошо ) */
                    if (Properties.Settings.Default.RemMe == true)
                    {
                        Properties.Settings.Default.RemMe = false;
                        Properties.Settings.Default.Save();
                    }
                    Owner.Owner.Show();
                    Owner.Close();
                    Close();
                }
                else if (response == "6")
                {
                    /* TODO ( Не удается создать запись ) */
                    Account_SnackbarMess.Message.Content = "Can't delete account";
                    Account_SnackbarMess.IsActive = true;
                }
                else if (response == "catch")
                {
                    /* TODO ( Нет интернета ) */
                    Account_SnackbarMess.Message.Content = "There is no internet connection";
                    Account_SnackbarMess.IsActive = true;
                }
                else
                {
                    /* TODO ( ошибка на сервере ) */
                    Account_SnackbarMess.Message.Content = "Error on server";
                    Account_SnackbarMess.IsActive = true;
                }
                DeleteAccountDialog_GridLoading.Visibility = Visibility.Collapsed;
                DeleteAccountDialog_ProgressLoading.IsIndeterminate = false;
            }
        }
        private bool DeleteAccountDialog_Validation(string pass)
        {
            bool result = false;
            if (pass.Length == 0)
            {
                DeleteAccountDialog_PasswordError.Text = "Field is required";
                DeleteAccountDialog_PasswordError.Visibility = Visibility.Visible;
                result = false;
            }
            else if (userPassword != pass)
            {
                DeleteAccountDialog_PasswordError.Text = "Incorrect password";
                DeleteAccountDialog_PasswordError.Visibility = Visibility.Visible;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        private void DeleteAccountDialog_TextBoxes_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement tbx = e.Source as FrameworkElement;
            switch (tbx.Name)
            {
                case "DeleteAccountDialog_Password":
                    DeleteAccountDialog_PasswordError.Visibility = Visibility.Hidden;
                    break;
            }
        }
        private async Task<String> DeleteAccountDialog_DeleteAccountAsync(String url, String dataPost)
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
        private void DeleteAccount_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            DeleteAccountDialog_Password.Password = "";
            DeleteAccountDialog_PasswordError.Visibility = Visibility.Hidden;
        }
        //-----------AREA-DELETE-ACCOUNT-DIALOG-END---------
    }
}
