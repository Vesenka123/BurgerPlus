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
    /// Логика взаимодействия для AdminPageEditor.xaml
    /// </summary>
    public partial class AdminPageEditor : Page
    {
        public AdminPageEditor()
        {
            InitializeComponent();
            LoadUsers();
        }



        private void LoadUsers()
        {
            UsersGrid.ItemsSource = AppConnect.modelOdb.user.ToList();
        }



        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.frameMain.Navigate(new PageBurgerPlus.Admin());
        }

        private void Unlock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int userId)
            {
                var user = AppConnect.modelOdb.user.FirstOrDefault(x => x.id == userId);
                if (user != null)
                {
                    user.failed_attempts = 0;
                    user.is_locket = false;
                    user.lockout_expires = null;
                    AppConnect.modelOdb.SaveChanges();

                    UsersGrid.Items.Refresh();
                    MessageBox.Show($"Пользователь {user.login} разблокирован", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.modelOdb.SaveChanges();
            MessageBox.Show("Изменения сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
