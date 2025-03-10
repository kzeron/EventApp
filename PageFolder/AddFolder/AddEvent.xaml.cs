﻿using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System;
using System.Collections.Generic;
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

namespace EventApp.PageFolder.AddFolder
{
    /// <summary>
    /// Логика взаимодействия для AddEvent.xaml
    /// </summary>
    public partial class AddEvent : Page
    {
        public AddEvent()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Считываем данные
            var title = TitleTb.Text;
            var description = DescriptionTb.Text;
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название.");
                return;
            }

            var context = EventEntities.GetContext();
            var newEvent = new Events
            {
               Title = title,
               Description = description,
               DateStart = startDate,
               EndDate = endDate,
               // Здесь можно указать LocationId, OrganizerId, если нужно
            };

                context.Events.Add(newEvent);
                context.SaveChanges();
            

            MBClass.InformationMB("Мероприятие добавлено!");
            // Закрываем модальное окно
            (Application.Current.MainWindow as WindowFolder.WindowMain)?.CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем модальное окно
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.CloseModal();
            }
        }

    }
}
