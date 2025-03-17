using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder.EditFolder
{
    /// <summary>
    /// Логика взаимодействия для EditUser.xaml
    /// </summary>
    public partial class EditUser : Page
    {
        private User _user;
        public EditUser(int userId)
        {
            InitializeComponent();
            LoadUser(userId);
        }
        private void LoadUser(int userId)
        {
            var context = EventEntities.GetContext();
            _user = context.User.FirstOrDefault(u => u.IdUser == userId);
            if(_user == null)
            {
                MBClass.ErrorMB("Пользователь не найдет");
                return;
            }
            DataContext = _user;
            RoleCb.ItemsSource = context.Role.ToList();
            RoleCb.SelectedItem = _user.IdRole;
            LoginTb.Text = _user.Login;
            EmailTb.Text = _user.Email;
            PhoneTb.Text = _user.Phone;
            FirstNameTb.Text = _user.Name;
            LastNameTb.Text = _user.Surname;
            MiddleNameTb.Text = _user.Patronymic;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null)
            {
                MBClass.ErrorMB("Ошибка загрузки пользователя.");
                return;
            }

            _user.Login = LoginTb.Text.Trim();
            _user.Email = EmailTb.Text.Trim();
            _user.Phone = PhoneTb.Text.Trim();
            _user.Name = FirstNameTb.Text.Trim();
            _user.Surname = LastNameTb.Text.Trim();
            _user.Patronymic = MiddleNameTb.Text.Trim();
            _user.IdRole = RoleCb.SelectedValue != null ? (int)RoleCb.SelectedValue : _user.IdRole;

            if (string.IsNullOrWhiteSpace(_user.Login))
            {
                MBClass.ErrorMB("Введите логин.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_user.Email) || !ClassDataValidator.IsEmailValid(_user.Email))
            {
                MBClass.ErrorMB("Некорректный email.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_user.Phone) || !ClassDataValidator.IsPhoneNumberValid(_user.Phone))
            {
                MBClass.ErrorMB("Некорректный номер телефона.");
                return;
            }

            var context = EventEntities.GetContext();
            context.SaveChanges();

            MBClass.InformationMB("Изменения сохранены!");
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.CloseModal();
        }
    }
}
