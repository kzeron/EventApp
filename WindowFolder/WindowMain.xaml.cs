using EventApp.ClassFolder;
using EventApp.PageFolder.ListFolder;
using EventApp.PageFolder.AddFolder;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EventApp.PageFolder.EditFolder;

namespace EventApp.WindowFolder
{
    /// <summary>
    /// Логика взаимодействия для WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private UserRole _userRole;

        public WindowMain(Page thisPage)
        {
            InitializeComponent();
            MainFrame.Content = thisPage;
            _userRole =(UserRole)ClassSaveSassion.LoadSession().IdRole;
            //SetupNavigation();
        }
        public void OpenAddEventModal()
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            AddEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new AddEvent());
        }
        public void OpenEditEventModal(int eventId)
        {
            OverlayGrid.Visibility = Visibility.Visible;

            // Блокируем кнопки
            UsersButton.IsEnabled = false;
            EventsButtonManage.IsEnabled = false;
            AddEventButton.IsEnabled = false;

            AddEventFrame.Navigate(new EditEvent(eventId));
        }


        private void SetupNavigation()
        {
            if (_userRole == UserRole.Participant)
            {
                UsersButton.Visibility = Visibility.Collapsed;
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
            AddEventButton.IsEnabled = true;
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
    }
}
