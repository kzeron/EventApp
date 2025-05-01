using EventApp.ClassFolder;
using EventApp.PageFolder.ListFolder;
using EventApp.PageFolder.AddFolder;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventApp.PageFolder.EditFolder;
using EventApp.DataFolder;
using System.Linq;
using EventApp.PageFolder;
using System.Text;

namespace EventApp.WindowFolder
{
    /// <summary>
    /// Логика взаимодействия для WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private UserRole _userRole;
        private Employee _currentUser;
        private EventEntities _ctx;


        public WindowMain()
        {
            InitializeComponent();

            // Сначала скрываем основное и боковую панель
            SidePanel.Visibility =   Visibility.Collapsed;
            MainFrame.Visibility =   Visibility.Collapsed; 
            OverlayGrid.Visibility =   Visibility.Visible;
            AddEventFrame.Visibility = Visibility.Visible;

            AddEventFrame.Navigate(new LoginPage());

            _ctx =  EventEntities.GetContext();
            this.Loaded += WindowMain_Loaded;
            
            //SetupNavigation();
        }
        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            // После того, как окно отображено, проверяем сессию
            var sessionUser = ClassSaveSassion.LoadSession();
            if (sessionUser == null)
            {
                // Остаёмся на странице логина
                return;
            }

            // Проверяем, валидна ли эта сессия
            var user = _ctx.Users.FirstOrDefault(u => u.IdUser == sessionUser.IdUser);
            if (user == null || user.StatusID == (int)Statuses.Fired)
            {
                ClassSaveSassion.ClearSession();
                return;
            }

            // Всё хорошо — прячем логин и показываем основное меню
            OverlayGrid.Visibility = Visibility.Collapsed;
            AddEventFrame.Visibility = Visibility.Collapsed;
            SidePanel.Visibility = Visibility.Visible;
            MainFrame.Visibility = Visibility.Visible;

            // Сохраняем роль и профиль
            _userRole = (UserRole)user.IdRole;
            _currentUser = _ctx.Employee.FirstOrDefault(e2 => e2.UserId == user.IdUser);
            InitUserInfo();

            // Навигация на стартовую страницу для этой роли
            switch (_userRole)
            {
                case UserRole.Admin:
                    MainFrame.Navigate(new ListUser());
                    SetPageTitle("Список пользователей");
                    break;
                case UserRole.Teacher:
                    MainFrame.Navigate(new ListEvent());
                    SetPageTitle("Список мероприятий");
                    break;
                case UserRole.Participant:
                    MainFrame.Navigate(new ListEventForParticipant());
                    SetPageTitle("Мои мероприятия");
                    break;
            }
            EmailService.SendRemindersForTomorrowEvents();
        }
        private void InitUserInfo()
        {
            if (_currentUser != null)
            {
                var s = _currentUser.Surname ?? "";
                var n = _currentUser.Name ?? "";
                var p = _currentUser.Patronymic ?? "";

                var initials = new StringBuilder();
                if (n.Length > 0) initials.Append(n[0]).Append('.');
                if (p.Length > 0) initials.Append(' ').Append(p[0]).Append('.');

                UserFullName.Text = $"{s} {initials}";
            }
            else
            {
                UserFullName.Text = "Данные не найдены";
            }
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
            AddEventFrame.Visibility = Visibility.Visible;

            AddEventFrame.NavigationService.RemoveBackEntry();
            AddEventFrame.Content = null;
            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new AddUser());
        }
        public void OpenEditEventModal(int eventId)
        {
            OverlayGrid.Visibility = Visibility.Visible;
            AddEventFrame.Visibility = Visibility.Visible;

            AddEventFrame.NavigationService.RemoveBackEntry();
            AddEventFrame.Content = null;
            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new EditEvent(eventId));
        }

        public void OpenEditUserModal(int userId)
        {
            OverlayGrid.Visibility = Visibility.Visible;
            AddEventFrame.Visibility = Visibility.Visible;

            AddEventFrame.NavigationService.RemoveBackEntry();
            AddEventFrame.Content = null;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            ListEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new EditUser(userId));
        }
        public void OpenProfileModal()
        {
            OverlayGrid.Visibility = Visibility.Visible;
            AddEventFrame.Visibility = Visibility.Visible;
            
            AddEventFrame.NavigationService.RemoveBackEntry();
            AddEventFrame.Content = null;

            UsersButton.IsEnabled = EventsButtonManage.IsEnabled = ListEventButton.IsEnabled = false;
            AddEventFrame.Navigate(new ProfilePage());
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
            MainFrame.Content = new ListUser();
            SetPageTitle("Список пользователей");
        }

        private void ShowEventsPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ListEvent();
            SetPageTitle("Список Мероприятий");
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

        private void ListSpekersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ListSpeaker();
            SetPageTitle("Список организаторов");
        }

        private void UserFullName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenProfileModal();
        }
        public void SetPageTitle(string title)
        {
            PageTitleTB.Text = title;
        }

        private void AddUsers_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = GetWindow(this) as WindowMain;
            mainWindow?.OpenAddUserModal();
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.OpenAddEventModal();
            }
        }
    }
}
