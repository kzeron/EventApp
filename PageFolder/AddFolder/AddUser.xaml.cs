﻿using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder.AddFolder
{
    public partial class AddUser : Page
    {
        public AddUser()
        {
            InitializeComponent();
            RoleCb.ItemsSource = EventEntities.GetContext().Role.ToList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTb.Text.Trim();
            string password = PasswordPb.Password.Trim();
            string email = EmailTb.Text.Trim();
            string phone = PhoneTb.Text.Trim();
            string firstName = FirstNameTb.Text.Trim();
            string lastName = LastNameTb.Text.Trim();
            string middleName = MiddleNameTb.Text.Trim();
            int? roleId = RoleCb.SelectedValue as int?;

            if (string.IsNullOrWhiteSpace(login))
            {
                MBClass.ErrorMB("Введите логин.");
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                MBClass.ErrorMB("Введите пароль.");
            }
            else if (string.IsNullOrWhiteSpace(email))
            {
                MBClass.ErrorMB("Введите email.");
            }
            else if (!ClassDataValidator.IsEmailValid(email))
            {
                MBClass.ErrorMB("Некорректный email. Допустимые домены: gmail.com, yandex.ru, mail.ru.");
            }
            else if (string.IsNullOrWhiteSpace(phone))
            {
                MBClass.ErrorMB("Введите номер телефона.");
            }
            else if (!ClassDataValidator.IsPhoneNumberValid(phone))
            {
                MBClass.ErrorMB("Некорректный номер телефона.");
            }
            else if (string.IsNullOrWhiteSpace(firstName))
            {
                MBClass.ErrorMB("Введите имя.");
            }
            else if (string.IsNullOrWhiteSpace(lastName))
            {
                MBClass.ErrorMB("Введите фамилию.");
            }
            else if (!roleId.HasValue)
            {
                MBClass.ErrorMB("Выберите роль пользователя.");
            }
            else
            {
                var context = EventEntities.GetContext();

                var user = new User
                {
                    Login = login,
                    Password = password,
                    Email = email,
                    Phone = phone,
                    Name = firstName,
                    Surname = lastName,
                    Patronymic = middleName,
                    IdRole = roleId.Value,
                    StatusID = (int)Statuses.Working
                };

                context.User.Add(user);
                context.SaveChanges();

                switch ((UserRole)user.IdRole)
                {
                    case UserRole.Teacher:
                        context.Trainers.Add(new Trainers { UserId = user.IdUser });

                        break;
                    case UserRole.Participant:
                        context.Participants.Add(new Participants 
                        {
                            IdUser = user.IdUser,
                        });
                        break;
                }

                context.SaveChanges();
                MBClass.InformationMB("Пользователь успешно добавлен!");

                WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
                mainWindow?.CloseModal();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.CloseModal();
        }
    }
}
