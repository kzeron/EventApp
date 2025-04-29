using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace EventApp.PageFolder
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {

        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }
        private void LoadProfile()
        {
            var session = ClassSaveSassion.LoadSession();
            if (session == null) return;

            var ctx = EventEntities.GetContext();
            var user = ctx.Users.FirstOrDefault(u => u.IdUser == session.IdUser);
            var emp = ctx.Employee.FirstOrDefault(e => e.UserId == session.IdUser);

            if (user == null || emp == null) return;

            // ФИО
            FullNameTb.Text = $"{emp.Surname} {emp.Name} {emp.Patronymic}".Trim();

            // Логин, email, телефон
            LoginTb.Text = user.Login;
            EmailTb.Text = emp.Email;
            PhoneTb.Text = emp.Phone;

            // Фото, если есть
            if (emp.Photo != null && emp.Photo.Length > 0)
            {
                var ms = new MemoryStream(emp.Photo);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                UserPhoto.Source = bmp;
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Вернуться назад: убрать оверлей и очистить фрейм
            if (Window.GetWindow(this) is WindowMain window)
            {
                window.OverlayGrid.Visibility = Visibility.Collapsed;
                window.UsersButton.IsEnabled =
                window.EventsButtonManage.IsEnabled =
                window.ListEventButton.IsEnabled = true;
                window.AddEventFrame.Content = null;
            }
        }
    }
}
