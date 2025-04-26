using EventApp.ClassFolder;
using EventApp.PageFolder.ListFolder;
using EventApp.PageFolder.AddFolder;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventApp.PageFolder.EditFolder;
using EventApp.DataFolder;
using System.Linq;

namespace EventApp.WindowFolder
{
    /// <summary>
    /// Логика взаимодействия для WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private UserRole _userRole;
        private Employee _currentUser;

        public WindowMain(Page thisPage)
        {
            InitializeComponent();
            
            var sessionUser = ClassSaveSassion.LoadSession();
            if(sessionUser == null )
            {
                MBClass.ErrorMB("Аккаунт не найден");
                new WindowAuth().Show();
                Close();
                return;
            }

            using (var ctx = EventEntities.GetContext())
            {
                _currentUser = ctx.Employee.FirstOrDefault(emp => emp.UserId == sessionUser.IdUser);
            }
            if (_currentUser != null)
            {
                string surname = _currentUser.Surname ?? "";
                string name = _currentUser.Name ?? "";
                string patronymic = _currentUser.Patronymic ?? "";

                string initials = "";
                if (!string.IsNullOrEmpty(name)) initials += name[0] + ".";
                if (!string.IsNullOrEmpty(patronymic)) initials += " " + patronymic[0] + ".";

                UserFullName.Text = $"{surname} {initials}";
            }
            else
            {
                UserFullName.Text = "Данные не найдены";
            }

            MainFrame.Content = thisPage;
            _userRole =(UserRole)ClassSaveSassion.LoadSession().IdRole;
            SetupNavigation();
        }
        public void OpenAddEventModal()
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new AddEvent());
        }
        public void OpenAddUserModal()
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new AddUser());
        }
        public void OpenEditEventModal(int eventId)
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new EditEvent(eventId));
        }

        public void OpenEditUserModal(int userId)
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new EditUser(userId));
        }


        private void SetupNavigation()
        {
            if (_userRole == UserRole.Participant)
            {
                UsersButton.Visibility = Visibility.Collapsed;
                EventsButtonManage.Visibility = Visibility.Collapsed;
            }
            else if(_userRole == UserRole.Admin)
            {
                EventsButtonManage.Visibility = Visibility.Collapsed;
                ListEventButton.Visibility = Visibility.Collapsed;
            }
            else if(_userRole == UserRole.Teacher)
            {
                UsersButton.Visibility= Visibility.Collapsed;
            }
            // Добавьте проверки для других ролей
        }


        // Вызывается из AddEventPage, когда сохранено или отменено
        public void CloseModal()
        {
            OverlayGrid.Visibility = Visibility.Collapsed;

            // Разблокируем кнопки
            UsersButton.IsEnabled = true;
            EventsButtonManage.IsEnabled = true;
            ListEventButton.IsEnabled = true;
            if (MainFrame.Content is ListEvent listEventPage)
            {
                listEventPage.LoadEvents();
            }
            else if(MainFrame.Content is ListUser listUserPage)
            {
                listUserPage.LoadUsers();
            }
        }

        private void ShowUsersPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ListUser(); // Или ваша страница UsersPage
        }

        private void ShowEventsPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ListEvent(); // Или ваша страница EventsPage
        }
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MBClass.ExitMB();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ClassSaveSassion.ClearSession();
            new WindowAuth().Show();
            Close();
        }

        private void EventButtonList_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ListEventForParticipant();
        }
        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            EmailService.SendRemindersForTomorrowEvents();
        }

        private void ListSpekersButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
