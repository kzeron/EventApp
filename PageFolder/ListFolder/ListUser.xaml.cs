using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EventApp.PageFolder.ListFolder
{
    public partial class ListUser : Page
    {
        private ObservableCollection<ClassUser> _users;
        private ObservableCollection<ClassUser> _filteredUsers;
        public ObservableCollection<StatusFilter> StatusFilters { get; set; } = new ObservableCollection<StatusFilter>();

        public ListUser()
        {
            InitializeComponent();
            _users = new ObservableCollection<ClassUser>();
            _filteredUsers = new ObservableCollection<ClassUser>();
            LoadUsers();
            InitializeFilters();
            UsersListBox.ItemsSource = _filteredUsers;
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = !FilterPopup.IsOpen;
        }
        private void InitializeFilters()
        {
            StatusFilters.Add(new StatusFilter { Id = (int)Statuses.Working, Name = "Работает" });
            StatusFilters.Add(new StatusFilter { Id = (int)Statuses.Fired, Name = "Уволен" });
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var selectedIds = StatusFilters
                .Where(f => f.IsChecked)
                .Select(f => f.Id)
                .ToList();

            if (!selectedIds.Any())
            {
                UsersListBox.ItemsSource = _users;
            }
            else
            {
                UsersListBox.ItemsSource = _users
                    .Where(u => selectedIds.Contains((int)u.Statuses))
                    .ToList();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemWidth();
        }

        private void UpdateItemWidth()
        {
            // Отнимаем от ширины ListBox отступы, скроллбар и прочее (например, 40 пикселей)
            double availableWidth = UsersListBox.ActualWidth - 40;
            if (availableWidth <= 0) return;

            // Желаемая минимальная ширина одного элемента
            double minItemWidth = 300;

            // Сколько элементов влезает?
            int itemsPerRow = Math.Max(1, (int)(availableWidth / minItemWidth));

            // Новая ширина для каждого элемента
            double newItemWidth = (availableWidth / itemsPerRow) - 10; // учёт отступов

            // Сохраняем в Tag, чтобы привязка могла использовать
            UsersListBox.Tag = newItemWidth;
        }

        public void LoadUsers()
        {
            var context = EventEntities.GetContext();
            var staffData = (from user in context.Users
                             join employee in context.Employee on user.IdUser equals employee.UserId
                             join role in context.Role on user.IdRole equals role.IdRole into roleGroup
                             from role in roleGroup.DefaultIfEmpty()
                             join status in context.Status on user.StatusID equals status.IdStatus into statusGroup
                             from status in statusGroup.DefaultIfEmpty()
                             select new
                             {
                                 user.IdUser,
                                 user.Login,
                                 user.Password,
                                 employee.Name,
                                 employee.Surname,
                                 employee.Patronymic,
                                 RoleId = role != null ? role.IdRole : 0,
                                 RoleName = role != null ? role.NameRole : null,
                                 StatusId = status != null ? status.IdStatus : 0,
                                 StatusName = status != null ? status.NameStatus : null,
                                 employee.Email,
                                 employee.Phone,
                                 employee.Photo
                             }).OrderBy(u => u.IdUser).ToList();

            _users.Clear();
            foreach (var user in staffData)
            {
                _users.Add(new ClassUser
                {
                    IdUser = user.IdUser,
                    Login = user.Login,
                    Password = user.Password,
                    Name = user.Name,
                    Surname = user.Surname,
                    Patronymic = user.Patronymic,
                    IDRole = user.RoleId,
                    Role = (UserRole)user.RoleId,
                    Statuses = (Statuses)user.StatusId,
                    StatusName = user.StatusName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Photo = LoadImage(user.Photo)
                });
            }

            _filteredUsers = new ObservableCollection<ClassUser>(_users);
            UsersListBox.ItemsSource = _filteredUsers;
        }


        // Метод для конвертации байтового массива в BitmapImage
        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            BitmapImage image = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }


        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();

            _filteredUsers.Clear();

            foreach (var user in _users.Where(u =>
                (u.Login?.ToLower().Contains(searchText) ?? false) ||
                (u.Email?.ToLower().Contains(searchText) ?? false) ||
                (u.Phone?.Contains(searchText) ?? false) ||
                (u.Name?.ToLower().Contains(searchText) ?? false) ||
                (u.Surname?.ToLower().Contains(searchText) ?? false) ||
                (u.Patronymic?.ToLower().Contains(searchText) ?? false) ||
                (u.StatusName?.ToLower().Contains(searchText) ?? false)
            ))
            {
                _filteredUsers.Add(user);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.OpenAddUserModal();
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

            private void ChangeStatus_Working_Click(object sender, RoutedEventArgs e)
            {
                if (UsersListBox.SelectedItem is ClassUser selectedUser)
                {
                    UpdateUserStatus(selectedUser, Statuses.Working);
                }
            }

            private void ChangeStatus_Fired_Click(object sender, RoutedEventArgs e)
            {
                if (UsersListBox.SelectedItem is ClassUser selectedUser)
                {
                    UpdateUserStatus(selectedUser, Statuses.Fired);
                }
            }

        private void UpdateUserStatus(ClassUser selectedUser, Statuses newStatus)
        {
            try
            {
                var context = EventEntities.GetContext(); // Используйте новый контекст
                
                    var user = context.Users.FirstOrDefault(u => u.IdUser == selectedUser.IdUser);
                    if (user != null)
                    {
                        user.StatusID = (int)newStatus;
                        context.Entry(user).State = EntityState.Modified;
                        context.SaveChanges();

                        UsersListBox.Items.Refresh();

                    // Обновляем статус в коллекции
                    selectedUser.Statuses = newStatus;
                    }
                LoadUsers();
            }
            catch (Exception ex)
            {
                MBClass.ErrorMB($"Ошибка: {ex.Message}");
            }
        }
        private void MenuItemHistory_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi?.Parent as ContextMenu;
            var border = cm?.PlacementTarget as Border;
            var selectedUser = border?.DataContext as ClassUser;
            if (selectedUser == null) return;

            // Находим EmployeeId по выбранному пользователю
            var ctx = EventEntities.GetContext();
            var employee = ctx.Employee.FirstOrDefault(emp => emp.UserId == selectedUser.IdUser);
            if (employee == null)
            {
                MBClass.WarningMB("У данного пользователя нет профиля сотрудника/участника.");
                return;
            }

            // Навигация на страницу истории
            this.NavigationService?.Navigate(new HistoryEventParticant(employee.EmployeeId));
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            var selectedUser = button.DataContext as ClassUser;
            if (selectedUser == null) return;

            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.OpenEditUserModal(selectedUser.IdUser);
        }
    }
}
