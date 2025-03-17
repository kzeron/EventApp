﻿using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder.ListFolder
{
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

        public void LoadUsers()
        {
            var context = EventEntities.GetContext();
            var staffData = (from user in context.User
                             join role in context.Role on user.IdRole equals role.IdRole into roleGroup
                             from role in roleGroup.DefaultIfEmpty()
                             join status in context.Status on user.StatusID equals status.IdStatus into statusGroup
                             from status in statusGroup.DefaultIfEmpty()
                             select new
                             {
                                 user.IdUser,
                                 user.Login,
                                 user.Password,
                                 user.Name,
                                 user.Surname,
                                 user.Patronymic,
                                 RoleId = role != null ? role.IdRole : 0,
                                 RoleName = role != null ? role.NameRole : null,
                                 StatusId = status != null ? status.IdStatus : 0,
                                 StatusName = status != null ? status.NameStatus : null, // Получаем название статуса
                                 user.Email,
                                 user.Phone,
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
                    StatusName = user.StatusName, // Заполняем StatusName
                    Email = user.Email,
                    Phone = user.Phone
                });
            }
            UsersListBox.ItemsSource = _users;
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
                
                    var user = context.User.FirstOrDefault(u => u.IdUser == selectedUser.IdUser);
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
