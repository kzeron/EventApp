using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EventApp.PageFolder.EditFolder
{
    /// <summary>
    /// Логика взаимодействия для EditUser.xaml
    /// </summary>
    public partial class EditUser : Page
    {
        private EventEntities _context = EventEntities.GetContext();
        private Users _user;
        private Employee _employee;
        public EditUser(int userId)
        {
            InitializeComponent();
            LoadUser(userId);
        }
        private BitmapImage LoadImage(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                return image;
            }
        }
        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                byte[] imageBytes = File.ReadAllBytes(filePath);

                _employee.Photo = imageBytes; // Запись в объект пользователя
                UserPhoto.Source = LoadImage(imageBytes); // Отображение в UI
            }
        }

        private void LoadUser(int userId)
        {
            _user = _context.Users.FirstOrDefault(u => u.IdUser == userId);
            if (_user == null)
            {
                MBClass.ErrorMB("Пользователь не найден");
                return;
            }
            _employee = _context.Employee.FirstOrDefault(emp => emp.UserId == _user.IdUser);
            if (_employee == null)
            {
                MBClass.ErrorMB("Сотрудник, связанный с пользователем, не найден.");
                return;
            }

            DataContext = _user;
            RoleCb.ItemsSource = _context.Role.ToList();
            RoleCb.SelectedValue = _user.IdRole;
            LoginTb.Text = _user.Login;
            EmailTb.Text = _employee.Email;
            PhoneTb.Text = _employee.Phone;
            FirstNameTb.Text = _employee.Name;
            LastNameTb.Text = _employee.Surname;
            MiddleNameTb.Text = _employee.Patronymic;

            if (_employee.Photo != null && _employee.Photo.Length > 0)
            {
                UserPhoto.Source = LoadImage(_employee.Photo);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_user == null)
            {
                MBClass.ErrorMB("Ошибка загрузки пользователя.");
                return;
            }

            _user.Login = LoginTb.Text.Trim();
            _employee.Email = EmailTb.Text.Trim();
            _employee.Phone = PhoneTb.Text.Trim();
            _employee.Name = FirstNameTb.Text.Trim();
            _employee.Surname = LastNameTb.Text.Trim();
            _employee.Patronymic = MiddleNameTb.Text.Trim();
            _user.IdRole = RoleCb.SelectedValue != null ? (int)RoleCb.SelectedValue : _user.IdRole;

            if (string.IsNullOrWhiteSpace(_user.Login))
            {
                MBClass.ErrorMB("Введите логин.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_employee.Email) || !ClassDataValidator.IsEmailValid(_employee.Email))
            {
                MBClass.ErrorMB("Некорректный email.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_employee.Phone) || !ClassDataValidator.IsPhoneNumberValid(_employee.Phone))
            {
                MBClass.ErrorMB("Некорректный номер телефона.");
                return;
            }

            try
            {
                _context.SaveChanges();
                MBClass.InformationMB("Изменения сохранены!");

                WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
                mainWindow?.CloseModal();
            }
            catch (Exception ex)
            {
                MBClass.ErrorMB($"Ошибка сохранения: {ex.Message}");
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.CloseModal();
        }
    }
}
