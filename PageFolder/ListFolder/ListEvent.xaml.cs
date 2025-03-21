﻿using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.PageFolder.AddFolder;
using EventApp.WindowFolder;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для ListEvent.xaml
    /// </summary>
    public partial class ListEvent : Page
    {
        private ObservableCollection<ClassEvent> _events;
        public ListEvent()
        {
            InitializeComponent();
            _events = new ObservableCollection<ClassEvent>();
            LoadEvents();
        }
        public void LoadEvents()
        {
            var context = EventEntities.GetContext();
            var currentDate = DateTime.Now;

            var eventsToUpdate = context.Events
                .Where(ev => ev.EndDate != null && ev.EndDate < currentDate && ev.StatusId != (int)EventStatuses.Passed)
                .ToList();

            // Обновляем статус мероприятий, если они завершились
            if (eventsToUpdate.Any())
            {
                foreach (var ev in eventsToUpdate)
                {
                    ev.StatusId = (int)EventStatuses.Passed;
                }
                context.SaveChanges(); // Сохраняем изменения в базе данных
            }

            var eventData = (from ev in context.Events
                             join loc in context.Locations
                             on ev.LocationId equals loc.IdLocation into locGroup
                             from loc in locGroup.DefaultIfEmpty()
                             let participantCount = context.Participants.Count(p => p.IdEvent == ev.IdEvent)
                             select new
                             {
                                 ev.IdEvent,
                                 ev.Title,
                                 ev.Description,
                                 ev.DateStart,
                                 ev.EndDate,
                                 locId = loc != null ? loc.IdLocation : (int?)null,
                                 locName = loc != null ? loc.LocationName : null,
                                 address = loc != null ? loc.Address : null,
                                 loc.NumberCab,
                                 capacity = loc != null ? loc.Capacity : (int?)null,
                                 ev.OrganizerId,
                                 participantCount,
                                 ev.StatusId
                             }).OrderBy(e => e.IdEvent)
                             .ToList();

            _events.Clear();

            foreach (var item in eventData)
            {
                _events.Add(new ClassEvent
                {
                    IdEvent = item.IdEvent,
                    Title = item.Title,
                    Description = item.Description,
                    DateStart = item.DateStart,
                    EndDate = item.EndDate,
                    LocationId = item.locId,
                    LocationName = item.locName,
                    Address = item.address,
                    NumberCab = item.NumberCab,
                    Capacity = item.capacity,
                    OrganizerId = item.OrganizerId,
                    CurrentParticipants = item.participantCount,
                    StatusId = item.StatusId
                });
            }

            EventsListBox.ItemsSource = _events;
        }

        private void ViewParticipants_Click(object sender, RoutedEventArgs e)
        {
            // Определяем, на какой элемент (мероприятие) нажали
            var button = sender as Button;
            if (button == null) return;

            var selectedEvent = button.DataContext as ClassEvent;
            if (selectedEvent == null) return;

            // Переходим на страницу со списком участников, передав IdEvent
            this.NavigationService?.Navigate(new PageParticipants(selectedEvent.IdEvent));
        }
        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем ссылку на WindowMain
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.OpenAddEventModal();
            }
        }


        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var selectedEvent = button.DataContext as ClassEvent;
            if (selectedEvent == null) return;

            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.OpenEditEventModal(selectedEvent.IdEvent);
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredEvents = _events.Where(ev =>
                ev.Title.ToLower().Contains(searchText) ||
                ev.Description.ToLower().Contains(searchText) ||
                (ev.LocationName != null && ev.LocationName.ToLower().Contains(searchText)) ||
                ev.DateStart?.ToString("dd.MM.yyyy").Contains(searchText) == true // Проверяем на null
            ).ToList();

            EventsListBox.ItemsSource = new ObservableCollection<ClassEvent>(filteredEvents);
        }
        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var border = contextMenu?.PlacementTarget as Border;
            var selectedEvent = border?.DataContext as ClassEvent;

            if (selectedEvent == null) return;

            if (selectedEvent.StatusId == (int)EventStatuses.Passed)
            {
                MessageBox.Show("Нельзя изменить прошедшее мероприятие.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.OpenEditEventModal(selectedEvent.IdEvent);
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var border = contextMenu?.PlacementTarget as Border;
            var selectedEvent = border?.DataContext as ClassEvent;

            if (selectedEvent == null) return;

            if (selectedEvent.StatusId == (int)EventStatuses.Passed)
            {
                MessageBox.Show("Нельзя удалить прошедшее мероприятие.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Добавьте код для удаления мероприятия
            using (var context = new EventEntities())
            {
                var eventToDelete = context.Events.Find(selectedEvent.IdEvent);

                if (eventToDelete != null)
                {
                    context.Events.Remove(eventToDelete);
                    context.SaveChanges();
                    LoadEvents(); // Перезагрузите список мероприятий
                }
            }
        }
        private void MenuItemParticipants_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var border = contextMenu?.PlacementTarget as Border;
            var selectedEvent = border?.DataContext as ClassEvent;

            if (selectedEvent == null) return;

            this.NavigationService?.Navigate(new PageParticipants(selectedEvent.IdEvent));
        }
    }
}

