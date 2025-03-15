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
            var context = EventEntities.GetContext();

            var user = new User
            {
                Login = LoginTb.Text,
                Password = PasswordPb.Password,
                Email = EmailTb.Text,
                Phone = PhoneTb.Text,
                Name = FirstNameTb.Text,
                Surname = LastNameTb.Text,
                Patronymic = MiddleNameTb.Text,
                IdRole = (int)RoleCb.SelectedValue,
                StatusID = (int)ClassFolder.Statuses.Working
            };

            context.User.Add(user);
            context.SaveChanges();

            // Добавляем пользователя в соответствующую таблицу
            switch ((UserRole)user.IdRole)
            {
                case UserRole.Teacher:
                    var trainer = new Trainers { UserId = user.IdUser };
                    context.Trainers.Add(trainer);
                    break;

                case UserRole.Participant:
                    var participant = new Participants { IdUser = user.IdUser };
                    context.Participants.Add(participant);
                    break;
            }

            context.SaveChanges();
            MessageBox.Show("Пользователь успешно добавлен!");

            // Закрываем модальное окно
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.CloseModal();
            }
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
