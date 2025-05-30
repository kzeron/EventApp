    using EventApp.ClassFolder;
    using EventApp.DataFolder;
    using EventApp.PageFolder.AddFolder;
    using EventApp.WindowFolder;
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
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
        public ObservableCollection<StatusFilter> StatusFilters { get; set; } = new ObservableCollection<StatusFilter>();

        public ListEvent()
        {
            InitializeComponent();
            _events = new ObservableCollection<ClassEvent>();
            InitializeFilters();
            LoadEvents();
        }
        private void InitializeFilters()
        {
            StatusFilters.Add(new StatusFilter { Id = 6, Name = "Сбор на мероприятие" });
            StatusFilters.Add(new StatusFilter { Id = 7, Name = "Началось" });
            StatusFilters.Add(new StatusFilter { Id = 8, Name = "Проходит" });
            StatusFilters.Add(new StatusFilter { Id = 9, Name = "Завершилось" });
            StatusFilters.Add(new StatusFilter { Id = 10, Name = "Отменено" });
        }
        private void ApplyFilters()
        {
            string searchText = SearchTextBox.Text.ToLower();

            var selectedIds = StatusFilters
                .Where(f => f.IsChecked)
                .Select(f => f.Id)
                .ToList();

            var filtered = _events.AsEnumerable();

            // Фильтрация по статусу (если выбраны фильтры)
            if (selectedIds.Any())
            {
                filtered = filtered.Where(ev => selectedIds.Contains(ev.StatusId));
            }

            // Фильтрация по тексту поиска
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(ev =>
                    ev.Title.ToLower().Contains(searchText) ||
                    ev.Description.ToLower().Contains(searchText) ||
                    (ev.LocationName != null && ev.LocationName.ToLower().Contains(searchText)) ||
                    ev.DateStart?.ToString("dd.MM.yyyy").Contains(searchText) == true
                );
            }

            EventsListBox.ItemsSource = new ObservableCollection<ClassEvent>(filtered);
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemWidth();
        }

        private void UpdateItemWidth()
        {
            // Отнимаем от ширины ListBox отступы, скроллбар и прочее (например, 40 пикселей)
            double availableWidth = EventsListBox.ActualWidth - 40;
            if (availableWidth <= 0) return;

            // Желаемая минимальная ширина одного элемента
            double minItemWidth = 300;

            // Сколько элементов влезает?
            int itemsPerRow = Math.Max(1, (int)(availableWidth / minItemWidth));

            // Новая ширина для каждого элемента
            double newItemWidth = (availableWidth / itemsPerRow) - 10; // учёт отступов

            // Сохраняем в Tag, чтобы привязка могла использовать
            EventsListBox.Tag = newItemWidth;
        }

        public void LoadEvents()
        {
                try
                {
                    var context = EventEntities.GetContext();
                    var currentDate = DateTime.Now;

                    // Обновляем статус мероприятий, если они завершились
                    var eventsToUpdate = context.Events
                        .Where(ev => ev.EndDate != null && ev.EndDate < currentDate && ev.StatusId != (int)EventStatuses.Ended)
                        .ToList();

                    if (eventsToUpdate.Any())
                    {
                        foreach (var ev in eventsToUpdate)
                        {
                            ev.StatusId = (int)EventStatuses.Ended;
                        }
                        context.SaveChanges(); 
                    }
                    var eventsToStart =  context.Events
                        .Where(ev => ev.DateStart != null && ev.DateStart == currentDate && ev.StatusId != (int)EventStatuses.Start && ev.StatusId != (int)EventStatuses.OnGoing)
                        .ToList();
                    if (eventsToStart.Any())
                    {
                        foreach (var ev in eventsToStart)
                        {
                            ev.StatusId = (int)EventStatuses.Start;
                        }
                         context.SaveChanges();
                    }
                    // Получаем данные мероприятий
                    var eventData = (from ev in context.Events
                                     join loc in context.Locations
                                     on ev.LocationId equals loc.IdLocation into locGroup
                                     from loc in locGroup.DefaultIfEmpty()
                                     let participantCount = context.Participants.Count(p => p.IdEvent == ev.IdEvent)
                                     join st in context.Status
                                     on ev.StatusId equals st.IdStatus into statGroup
                                     from stat in statGroup.DefaultIfEmpty()
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
                                         capacity = loc != null ? loc.Capacity : null,
                                         ev.OrganizerId,
                                         participantCount,
                                         ev.StatusId,
                                         stat.NameStatus
                                     }).OrderByDescending(e => e.DateStart)
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
                            //Address = item.address,
                            NumberCab = item.NumberCab,
                            Capacity = item.capacity,
                            OrganizerId = item.OrganizerId,
                            CurrentParticipants = item.participantCount,
                            StatusId = item.StatusId,
                            StatusName = item.NameStatus
                        });
                    }

                    EventsListBox.ItemsSource = _events;
                }
                catch (Exception ex)
                {
                    // Обработка ошибок, например, запись в лог
                    MBClass.ErrorMB(ex.Message);
                }
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
                ApplyFilters();
            }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = !FilterPopup.IsOpen;
        }

        private void StatusListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the sender is a ListBox
            if (sender is ListBox listBox)
            {
                // Get the selected ListBoxItem
                if (listBox.SelectedItem is ListBoxItem selectedItem)
                {
                    // Get the Tag value from the selected ListBoxItem
                    string tagValue = selectedItem.Tag as string;

                    if (tagValue == null) // "Все" (All)
                    {
                        // If "Все" is selected, show all events
                        EventsListBox.ItemsSource = _events;
                    }
                    else
                    {
                        // Parse the tagValue to an integer (StatusId)
                        if (int.TryParse(tagValue, out int statusId))
                        {
                            // Filter events by StatusId
                            EventsListBox.ItemsSource = new ObservableCollection<ClassEvent>(
                                _events.Where(ev => ev.StatusId == statusId).ToList()
                            );
                        }
                        else
                        {
                            MBClass.WarningMB("Неверное значение фильтра статуса.");
                        }
                    }
                }
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
                var menuItem = sender as MenuItem;
                var contextMenu = menuItem?.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;
                var selectedEvent = border?.DataContext as ClassEvent;

                if (selectedEvent == null) return;

                if (selectedEvent.StatusId == (int)EventStatuses.Ended)
                {
                    MBClass.WarningMB("Нельзя изменить прошедшее мероприятие.");
                    return;
                }

                WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
                if (mainWindow != null)
                {
                    mainWindow.OpenEditEventModal(selectedEvent.IdEvent);
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
            private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
            {
                var menuItem = sender as MenuItem;
                var contextMenu = menuItem?.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;
                var selectedEvent = border?.DataContext as ClassEvent;

                if (selectedEvent == null) return;

                if (selectedEvent.StatusId == (int)EventStatuses.Ended)
                {
                    MBClass.WarningMB("Нельзя отменить прошедшее мероприятие.");
                    return;
                }

                // Изменяем статус на "Отменено"
                using (var context = new EventEntities())
                {
                    var eventToCancel = context.Events.Find(selectedEvent.IdEvent);

                    if (eventToCancel != null)
                    {
                        eventToCancel.StatusId = (int)EventStatuses.Canceled;
                        context.SaveChanges();
                        LoadEvents(); 
                    }
                }
            }
    }
}

