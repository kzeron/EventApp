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
        private User _currentUser;
        public ListEventForParticipant()
        {
            InitializeComponent();
            _events = new ObservableCollection<ClassEvent>();
            _currentUser = ClassSaveSassion.LoadSession();
            LoadEvents();
        }

        private void LoadEvents()
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

            // 1) Проверяем, не записан ли уже пользователь
            bool alreadyRegistered = context.Participants
                .Any(p => p.IdEvent == selectedItem.IdEvent && p.IdUser == _currentUser.IdUser);
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

            // 3) Проверяем, нет ли у пользователя мероприятия в ближайшие 7 дней
            var userEvents = from part in context.Participants
                             join ev in context.Events on part.IdEvent equals ev.IdEvent
                             where part.IdUser == _currentUser.IdUser
                             select ev;
            foreach (var ev in userEvents)
            {
                if (ev.DateStart.HasValue && selectedItem.DateStart.HasValue)
                {
                    var diff = (ev.DateStart.Value - selectedItem.DateStart.Value).Duration();
                    if (diff.TotalDays <= 7)
                    {
                        MBClass.ErrorMB("Нельзя записаться на мероприятия, идущие с разницей менее 7 дней.");
                        return;
                    }
                }
            }

            // Запись пользователя на мероприятие
            var newParticipant = new Participants
            {
                IdEvent = selectedItem.IdEvent,
                IdUser = _currentUser.IdUser,
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
            string searchText = SearchTextBox.Text.ToLower();

            var filteredEvents = _events.Where(ev =>
                ev.Title.ToLower().Contains(searchText) ||
                ev.Description.ToLower().Contains(searchText) ||
                (ev.LocationName != null && ev.LocationName.ToLower().Contains(searchText)) ||
                ev.DateStart?.ToString("dd.MM.yyyy").Contains(searchText) == true // Проверяем на null
            ).ToList();

            EventsListBox.ItemsSource = new ObservableCollection<ClassEvent>(filteredEvents);
        }
    }
}
