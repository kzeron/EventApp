using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.PageFolder.ListFolder;
using EventApp.WindowFolder;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginTb.Text.Trim();
            var pass = PasswordPb.Password;

            if (string.IsNullOrEmpty(login))
            {
                MBClass.ErrorMB("Введите логин");
                return;
            }
            if (string.IsNullOrEmpty(pass))
            {
                MBClass.ErrorMB("Введите пароль");
                return;
            }

            // имитация асинхронного запроса
            await Task.Delay(200);

            var ctx = EventEntities.GetContext();
            var user = ctx.Users.FirstOrDefault(u => u.Login == login);
            if (user == null || user.Password != pass)
            {
                MBClass.ErrorMB("Неверный логин или пароль");
                return;
            }

            ClassSaveSassion.SaveSassion(user);

            if (Application.Current.MainWindow is WindowMain win)
            {
                win.InitializeForCurrentSession();
            }
        }
    }
}
