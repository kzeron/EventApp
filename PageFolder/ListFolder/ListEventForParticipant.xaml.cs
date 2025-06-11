using EventApp.ClassFolder;
using EventApp.DataFolder;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static EventApp.ClassFolder.ClassParticipants;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для ListEventForParticipant.xaml
    /// </summary>
    public partial class ListEventForParticipant : Page
    {
        private ObservableCollection<ClassEvent> _events;
        public ObservableCollection<StatusFilter> StatusFilters { get; set; } = new ObservableCollection<StatusFilter>();
        private Users _currentUser;
        public ListEventForParticipant()
        {
            InitializeComponent();
            _events = new ObservableCollection<ClassEvent>();
            _currentUser = ClassSaveSassion.LoadSession();
            LoadEvents();
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            StatusFilters.Add(new StatusFilter { Id = 6, Name = "Сбор на мероприятие" });
            StatusFilters.Add(new StatusFilter { Id = 7, Name = "Началось" });
            StatusFilters.Add(new StatusFilter { Id = 8, Name = "Проходит" });
            StatusFilters.Add(new StatusFilter { Id = 9, Name = "Завершилось" });
            StatusFilters.Add(new StatusFilter { Id = 10, Name = "Отменено" });
        }
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = !FilterPopup.IsOpen;
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

        private void LoadEvents()
        {
            var context = EventEntities.GetContext();
            var currentDate = DateTime.Now;

            var eventsToUpdate = context.Events
                .Where(ev => ev.EndDate != null && ev.EndDate < currentDate && ev.StatusId != (int)EventStatuses.Ended)
                .ToList();

            // Обновляем статус мероприятий, если они завершились
            if (eventsToUpdate.Any())
            {
                foreach (var ev in eventsToUpdate)
                {
                    ev.StatusId = (int)EventStatuses.Ended;
                }
                context.SaveChanges(); // Сохраняем изменения в базе данных
            }

            var eventData = (from ev in context.Events
                             join loc in context.Locations
                             on ev.LocationId equals loc.IdLocation into locGroup
                             from loc in locGroup.DefaultIfEmpty()
                             join st in context.Status
                                     on ev.StatusId equals st.IdStatus into statGroup
                             from stat in statGroup.DefaultIfEmpty()
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


        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var selectedItem = button.DataContext as ClassEvent;
            if (selectedItem == null) return;

            var context = EventEntities.GetContext();

            // Проверяем, является ли пользователь участником (IDRole == 3)
            if (_currentUser.IdRole != (int)UserRole.Participant)
            {
                MBClass.ErrorMB("Только участники могут записываться на мероприятия.");
                return;
            }

            // Получаем текущего сотрудника
            var employee = context.Employee.FirstOrDefault(emp => emp.UserId == _currentUser.IdUser);
            if (employee == null)
            {
                MBClass.ErrorMB("Не удалось определить профиль пользователя.");
                return;
            }

            // 1) Проверяем, не записан ли уже пользователь
            bool alreadyRegistered = context.Participants
                .Any(p => p.IdEvent == selectedItem.IdEvent && p.IdEmploee == employee.EmployeeId);
            if (alreadyRegistered)
            {
                MBClass.ErrorMB("Вы уже записаны на данное мероприятие.");
                return;
            }

            // 2) Проверяем вместимость
            int currentCount = context.Participants.Count(p => p.IdEvent == selectedItem.IdEvent);
            if (selectedItem.Capacity.HasValue && currentCount >= selectedItem.Capacity.Value)
            {
                MBClass.ErrorMB("Нет свободных мест на это мероприятие.");
                return;
            }

            // 3) Проверяем, нет ли у пользователя мероприятий в ближайшие 7 дней
            var userEvents = from part in context.Participants
                             join ev in context.Events on part.IdEvent equals ev.IdEvent
                             where part.IdEmploee == employee.EmployeeId
                             select ev;

            DateTime today = DateTime.Today;
            DateTime nextWeek = today.AddDays(3);

            bool hasConflict = userEvents.Any(ev => ev.DateStart >= today && ev.EndDate <= nextWeek);
            if (hasConflict)
            {
                MBClass.ErrorMB("У вас уже запланировано мероприятие в ближайшие 3 дня.");
                return;
            }

            foreach (var ev in userEvents)
            {
                if (ev.DateStart.HasValue && selectedItem.DateStart.HasValue)
                {
                    var diff = (ev.DateStart.Value - selectedItem.DateStart.Value).Duration();
                    if (diff.TotalDays <= 3)
                    {
                        MBClass.ErrorMB("Нельзя записаться на мероприятия, идущие с разницей менее 3 дней.");
                        return;
                    }
                }
            }

            // Получаем текущего сотрудника по Id пользователя
            var currentEmployee = context.Employee.FirstOrDefault(emp => emp.UserId == _currentUser.IdUser);
            if (currentEmployee == null)
            {
                MBClass.ErrorMB("Не удалось определить профиль пользователя.");
                return;
            }

            // Запись пользователя на мероприятие
            var newParticipant = new Participants
            {
                IdEvent = selectedItem.IdEvent,
                IdEmploee = currentEmployee.EmployeeId,
                IdStatus = (int)ParticipantsStatuses.SignedUp, // ID=3
                RegistrationDate = DateTime.Now
            };
            context.Participants.Add(newParticipant);



            // Обновляем статус мероприятия, если он не установлен в "Собирается"
            var eventToUpdate = context.Events.FirstOrDefault(ev => ev.IdEvent == selectedItem.IdEvent);
            if (eventToUpdate != null && eventToUpdate.StatusId != (int)EventStatuses.Collecting)
            {
                eventToUpdate.StatusId = (int)EventStatuses.Collecting;
            }

            context.SaveChanges();

            MBClass.InformationMB("Вы успешно записались на мероприятие!");

            // Обновляем список (чтобы пересчитать количество участников)
            LoadEvents();
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }
    }
}
