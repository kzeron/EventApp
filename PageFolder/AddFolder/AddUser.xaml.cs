using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventApp.PageFolder.AddFolder
{
    /// <summary>
    /// Логика взаимодействия для AddUser.xaml
    /// </summary>
    public partial class AddUser : Page
    {
        public AddUser()
        {
            InitializeComponent();
            RoleCb.ItemsSource = EventEntities.GetContext().Role.ToList();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var contex = EventEntities.GetContext();
            var user = new User
            {
                Login = LoginTb.Text,
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем модальное окно
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.CloseModal();
            }
        }
    }
}
