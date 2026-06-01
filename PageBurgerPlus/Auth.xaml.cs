using BurgerPlus.Application;
using BurgerPlus.ApplicationData;
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

namespace BurgerPlus.PageBurgerPlus
{
    /// <summary>
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Page
    {
        private string _captchaCode;
        private int _captchaFailedAttempts = 0;
        private const int MaxAttempts = 3;
        private string _tempLogin;
        private string _tempPassword;
        private user _currentUser;

        public Auth()
        {
            InitializeComponent();
            AppConnect.modelOdb = BurgerPlusEntities.GetContext();
        }

        private void AuthorizationButton_Click(object sender, RoutedEventArgs e)
        {
            string loginUser = loginBox.Text.Trim();
            string passwordUser = passwordBox.Password;

            if (string.IsNullOrEmpty(loginUser) || string.IsNullOrEmpty(passwordUser))
            {
                MessageBox.Show("Введите логин и пароль!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser = AppConnect.modelOdb.user
                .FirstOrDefault(u => u.login == loginUser && u.password == passwordUser);

            if (_currentUser == null)
            {
                var userToBlock = AppConnect.modelOdb.user
                    .FirstOrDefault(u => u.login == loginUser);

                if (userToBlock != null)
                {
                    userToBlock.failed_attempts = (userToBlock.failed_attempts ?? 0) + 1;

                    if (userToBlock.failed_attempts >= MaxAttempts)
                    {
                        userToBlock.is_locket = true;
                    }
                    AppConnect.modelOdb.SaveChanges();

                    if (userToBlock.is_locket == true)
                    {
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetForm();
                        return;
                    }
                }

                int attemptsLeft = MaxAttempts - (userToBlock?.failed_attempts ?? 0);
                MessageBox.Show($"Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные. Осталось попыток: {attemptsLeft}",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);

                passwordBox.Clear();
                loginBox.Focus();
                return;
            }

            if (_currentUser.is_locket == true)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetForm();
                return;
            }

            _currentUser.failed_attempts = 0;
            AppConnect.modelOdb.SaveChanges();

            _tempLogin = loginUser;
            _tempPassword = passwordUser;

            authPanel.Visibility = Visibility.Collapsed;
            captchaPanel.Visibility = Visibility.Visible;

            _captchaFailedAttempts = 0;
            GenerateCaptcha();
            captchaInputBox.Focus();
        }

        private void GenerateCaptcha()
        {
            var random = new Random();
            _captchaCode = "";
            for (int i = 0; i < 4; i++)
                _captchaCode += random.Next(0, 10).ToString();

            captchaTextBlock.Text = _captchaCode;

            var colors = new[] {
                Brushes.DarkBlue, Brushes.DarkRed, Brushes.DarkGreen,
                Brushes.Purple, Brushes.Teal, Brushes.Orange
            };
            captchaTextBlock.Foreground = colors[random.Next(colors.Length)];
            captchaTextBlock.RenderTransform = new RotateTransform(random.Next(-15, 15));

            attemptsText.Text = $"Осталось попыток: {MaxAttempts - _captchaFailedAttempts}";
            captchaInputBox.Clear();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string captchaInput = captchaInputBox.Text.Trim();

            if (string.IsNullOrEmpty(captchaInput))
            {
                MessageBox.Show("Введите цифры с картинки!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                captchaInputBox.Focus();
                return;
            }

            if (captchaInput != _captchaCode)
            {
                _captchaFailedAttempts++;

                if (_captchaFailedAttempts >= MaxAttempts)
                {
                    _currentUser.is_locket = true;
                    _currentUser.failed_attempts = MaxAttempts;
                    AppConnect.modelOdb.SaveChanges();

                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                        "Блокировка", MessageBoxButton.OK, MessageBoxImage.Error);

                    ReturnToAuthForm();
                    return;
                }

                MessageBox.Show($"Неверная капча! Осталось попыток: {MaxAttempts - _captchaFailedAttempts}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                attemptsText.Text = $"Осталось попыток: {MaxAttempts - _captchaFailedAttempts}";
                GenerateCaptcha();
                captchaInputBox.Focus();
                return;
            }

            CompleteAuthorization();
        }

        private void CompleteAuthorization()
        {
            MessageBox.Show("Вы успешно авторизовались",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            var context = AppConnect.modelOdb;
            var role = context.user.FirstOrDefault(r => r.IdRole == _currentUser.IdRole);

            if (role != null)
            {
                string IdRole = role.IdRole.ToString();

                if (IdRole.Contains("1") || IdRole.Contains("1"))
                {
                    AppFrame.frameMain.Navigate(new PageBurgerPlus.Admin());
                }
                else
                {
                    AppFrame.frameMain.Navigate(new PageBurgerPlus.UserPage());
                }
            }
        }

        private void ReturnToAuthForm()
        {
            loginBox.Clear();
            passwordBox.Clear();
            captchaInputBox.Clear();
            authPanel.Visibility = Visibility.Visible;
            captchaPanel.Visibility = Visibility.Collapsed;
            loginBox.Focus();
        }

        private void ResetForm()
        {
            loginBox.Clear();
            passwordBox.Clear();
            captchaInputBox.Clear();
            authPanel.Visibility = Visibility.Visible;
            captchaPanel.Visibility = Visibility.Collapsed;
            _captchaFailedAttempts = 0;
            loginBox.Focus();
        }
    }
}

