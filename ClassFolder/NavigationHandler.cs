using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows;
using EventApp.PageFolder.ListFolder;

namespace EventApp.ClassFolder
{
    internal class NavigationHandler
    {
        private readonly NavigationService _navigationService;
        private readonly UserRole _userRole;

        public NavigationHandler(NavigationService navigationService, UserRole userRole)
        {
            _navigationService = navigationService;
            _userRole = userRole;
        }
        public void NavigateBasedOnRole()
        {
            switch (_userRole)
            {
                case UserRole.Admin:
                    _navigationService.Navigate(new ListUser());
                    break;
                case UserRole.Teacher:
                    _navigationService.Navigate(new ListEvent());
                    break;
                case UserRole.Participant:
                    _navigationService.Navigate(new ListEventForParticipant());
                    break;
                default:
                    MBClass.ErrorMB("Роль не определена.");
                    break;
            }
        }
    }
}
