﻿using EventApp.ClassFolder;
using EventApp.DataFolder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для ListUser.xaml
    /// </summary>
    public partial class ListUser : Page
    {
        public ObservableCollection<ClassUser> _users;
        public ListUser()
        {
            InitializeComponent();
            _users = new ObservableCollection<ClassUser>();
            LoadUsers();
            UsersListBox.ItemsSource = _users;
        }
        private void LoadUsers()
        {
            var context = EventEntities.GetContext();
                var staffData = (from user in context.User
                                 join role in context.Role on user.IdRole equals role.IdRole into roleGroup
                                 from role in roleGroup.DefaultIfEmpty()
                                 join status in context.Statuses on user.StatusID equals status.IdStatus into statusGroup
                                 from status in statusGroup.DefaultIfEmpty()
                                 select new
                                 {
                                     user.IdUser,
                                     user.Login,
                                     user.Password,
                                     RoleId = role != null ? role.IdRole : 0,
                                     RoleName = role != null ? role.NameRole : null,
                                     StatusId = status != null ? status.IdStatus : 0,
                                     StatusName = status != null ? status.NameStatus : null,
                                     user.Email,
                                     user.Phone,
                                 }).OrderBy(u => u.IdUser)
                                 .ToList();

                _users.Clear();
                foreach (var user in staffData)
                {
                    _users.Add(new ClassUser
                    {
                        Login = user.Login,
                        Password = user.Password,
                        IDRole = user.RoleId,
                        Role = (UserRole)user.RoleId,
                        Statuses = (ClassFolder.Statuses)user.StatusId,
                        Email = user.Email,
                        Phone = user.Phone
                    });
                }
                UsersListBox.ItemsSource = _users;
            
        }

    }
}
