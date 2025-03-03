using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EventApp.ClassFolder;
using EventApp.PageFolder;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EventApp.DataFolder;
using EventApp.PageFolder.ListFolder;

namespace EventApp.WindowFolder
{
    /// <summary>
    /// Логика взаимодействия для WindowAuth.xaml
    /// </summary>
    public partial class WindowAuth : Window
    {
        public WindowAuth()
        {
            InitializeComponent();
            this.Loaded += AuthLoaded;
        }
        private void AuthLoaded(object sender, RoutedEventArgs e)
        {
            var currentUser = ClassSaveSassion.LoadSession();
            if (currentUser != null)
            {
                try
                {
                    var user = EventEntities.GetContext().User.FirstOrDefault(u => u.IdUser == currentUser.IdUser);
                    if (user == null)
                    {
                        MBClass.ErrorMB("Связанный сотрудник не найден.");
                        return;
                    }
                    if (user.StatusID == (int)EventApp.ClassFolder.Statuses.Fired)
                    {
                        MBClass.ErrorMB("Ваша учетная запись не действительна");
                        ClassSaveSassion.ClearSession();
                        return; // Немедленно выйти из метода
                    }
                }
                catch (Exception ex)
                {
                    MBClass.ErrorMB(ex);
                }
            }

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

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(LoginTb.Text))
            {
                MBClass.ErrorMB("Введите логин");
                LoginTb.Focus();
            }
            else if (string.IsNullOrWhiteSpace(PasswordPb.Password))
            {
                MBClass.ErrorMB("Введите пароль");
            }
            else
            {
                try
                {
                    var context = EventEntities.GetContext();
                    var user = context.User
                        .FirstOrDefault(u => u.Login == LoginTb.Text);
                    if (user == null)
                    {
                        MBClass.ErrorMB("Введен неправильный логин");
                        LoginTb.Focus();
                    }
                    else if (user.Password != PasswordPb.Password)
                    {
                        MBClass.ErrorMB("Введен неправильный пароль");
                    }
                    else
                    {
                        if (user.StatusID == (int)EventApp.ClassFolder.Statuses.Fired)
                        {
                            MBClass.ErrorMB("Ваша учетная запись не действительна");
                            return; // Немедленно выйти из метода
                        }
                        ClassSaveSassion.SaveSassion(user);
                        switch((UserRole)user.IdRole)
                        {
                            case UserRole.Admin:
                                new WindowMain(new ListUser())
                                    .Show();
                                break;
                            case UserRole.Participant:
                                new WindowMain(new ListEventForParticipant())
                                    .Show(); 
                                break;
                            case UserRole.Teacher:
                                new WindowMain(new ListEvent())
                                    .Show(); 
                                break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    MBClass.ErrorMB(ex);
                }
            }
        }
    }
}
