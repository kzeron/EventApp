using System.Collections.ObjectModel;
using System.Linq;
using EventApp.ClassFolder;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using EventApp.DataFolder;
using System.Data;
using System.IO;
using System.Windows;
using System.Data.Entity;
using System;
using EventApp.WindowFolder;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для ListSpeaker.xaml
    /// </summary>
    public partial class ListSpeaker : Page
    {
        private ObservableCollection<ClassSpeaker> _speakers;
        private ObservableCollection<ClassSpeaker> _filtersSpeakers;
        public ObservableCollection<StatusFilter> StatusFilters { get; set; } = new ObservableCollection<StatusFilter>();
        public ListSpeaker()
        {
            InitializeComponent();
            _speakers = new ObservableCollection<ClassSpeaker>();
            _filtersSpeakers = new ObservableCollection<ClassSpeaker>();
            InitializeFilters();
            LoadSpeakers();
            SpeakersListBox.ItemsSource = _filtersSpeakers;
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
            ApplyFilters();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemWidth();
        }

        private void UpdateItemWidth()
        {
            // Отнимаем от ширины ListBox отступы, скроллбар и прочее (например, 40 пикселей)
            double availableWidth = SpeakersListBox.ActualWidth - 40;
            if (availableWidth <= 0) return;

            // Желаемая минимальная ширина одного элемента
            double minItemWidth = 300;

            // Сколько элементов влезает?
            int itemsPerRow = Math.Max(1, (int)(availableWidth / minItemWidth));

            // Новая ширина для каждого элемента
            double newItemWidth = (availableWidth / itemsPerRow) - 10; // учёт отступов

            // Сохраняем в Tag, чтобы привязка могла использовать
            SpeakersListBox.Tag = newItemWidth;
        }

        public void LoadSpeakers()
        {
            var context = EventEntities.GetContext();
            var speakersData = (from trainers in  context.Trainers
                                join employee in context.Employee on trainers.EmployeeId equals employee.EmployeeId
                                join events in context.Events on trainers.IdTrainer equals events.OrganizerId into eventsGroup
                                from events in eventsGroup.DefaultIfEmpty()
                                join user in context.Users on employee.UserId equals user.IdUser
                                join role in context.Role on user.IdRole equals role.IdRole
                                join status in context.Status on user.StatusID equals status.IdStatus into statusGroup
                                from status in statusGroup.DefaultIfEmpty()
                                select new
                                {
                                    trainers.IdTrainer,
                                    user.Login,
                                    employee.Name,
                                    employee.Surname,
                                    employee.Patronymic,
                                    RoleId = role != null ? role.IdRole : 0,
                                    RoleName = role != null ? role.NameRole : null,
                                    StatusId = status != null ? status.IdStatus : 0,
                                    StatusName = status != null ? status.NameStatus : null,
                                    employee.Email,
                                    employee.Phone,
                                    employee.Photo,
                                    events.Title,
                                    events.DateStart
                                }).OrderBy(t => t.IdTrainer).ToList();
            _speakers.Clear();
            foreach(var speaker in speakersData)
            {
                _speakers.Add(new ClassSpeaker
                {
                    IdSpeaker = speaker.IdTrainer,
                    Login = speaker.Login,
                    Name = speaker.Name,
                    Surname = speaker.Surname,
                    Patronymic = speaker.Patronymic,
                    IDRole = speaker.RoleId,
                    Role = (UserRole)speaker.RoleId,
                    Statuses = (Statuses)speaker.StatusId,
                    StatusName = speaker.StatusName,
                    Email = speaker.Email,
                    Phone = speaker.Phone,
                    Photo = LoadImage(speaker.Photo)
                });
            }
            _filtersSpeakers = new ObservableCollection<ClassSpeaker>(_speakers);
            SpeakersListBox.ItemsSource = _filtersSpeakers;
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if(imageData == null || imageData.Length == 0)
            {
                return null;
            }
            BitmapImage image = new BitmapImage();
            using(var stream = new MemoryStream(imageData))
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
            ApplyFilters();
        }
        private void ApplyFilters()
        {
            string searchText = (SearchTextBox.Text ?? "").ToLower();

            var selectedStatusIds = StatusFilters
                .Where(f => f.IsChecked)
                .Select(f => f.Id)
                .ToList();

            var filtered = _speakers.Where(s =>
                (selectedStatusIds.Count == 0 || selectedStatusIds.Contains((int)s.Statuses)) && (
                (s.Login?.ToLower().Contains(searchText) ?? false) ||
                (s.Email?.ToLower().Contains(searchText) ?? false) ||
                (s.Phone?.Contains(searchText) ?? false) ||
                (s.Name?.ToLower().Contains(searchText) ?? false) ||
                (s.Surname?.ToLower().Contains(searchText) ?? false) ||
                (s.Patronymic?.ToLower().Contains(searchText) ?? false) ||
                (s.StatusName?.ToLower().Contains(searchText) ?? false)
            )).ToList();

            _filtersSpeakers.Clear();
            foreach (var speaker in filtered)
            {
                _filtersSpeakers.Add(speaker);
            }
        }

        private void ChangeStatusButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                button.ContextMenu.IsOpen = true;
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

                    SpeakersListBox.Items.Refresh();

                    // Обновляем статус в коллекции
                    selectedUser.Statuses = newStatus;
                }
                LoadSpeakers();
            }
            catch (Exception ex)
            {
                MBClass.ErrorMB($"Ошибка: {ex.Message}");
            }
        }
        private void ChangeStatus_Working_Click(object sender, RoutedEventArgs e)
        {
            if (SpeakersListBox.SelectedItem is ClassUser selectedUser)
            {
                UpdateUserStatus(selectedUser, Statuses.Working);
            }
        }

        private void ChangeStatus_Fired_Click(object sender, RoutedEventArgs e)
        {
            if (SpeakersListBox.SelectedItem is ClassUser selectedUser)
            {
                UpdateUserStatus(selectedUser, Statuses.Fired);
            }
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
