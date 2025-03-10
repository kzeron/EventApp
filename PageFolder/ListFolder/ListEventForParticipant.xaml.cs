using EventApp.ClassFolder;
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
            var eventData = (from ev in context.Events
                             join loc in context.Locations
                             on ev.LocationId equals loc.IdLocation into locGroup
                             from loc in locGroup.DefaultIfEmpty()
                             
                             let participantCount = context.Participants
                             .Where(p => p.IdEvent == ev.IdEvent)
                             .Count()
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
                                 participantCount
                             }).OrderBy(e =>  e.IdEvent)
                             .ToList();
            _events.Clear();
            foreach(var item in eventData)
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
                    CurrentParticipants = item.participantCount
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

            // 1) Проверяем, не записан ли уже пользователь
            bool alreadyRegistered = context.Participants
                .Any(p => p.IdEvent == selectedItem.IdEvent
                && p.IdUser == _currentUser.IdUser);
            if (alreadyRegistered) 
            {
                MBClass.ErrorMB("Вы уже записаны на данное мероприятие");
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
            // Например, если user уже записан на мероприятие, которое стартует
            // +-7 дней от этого мероприятия

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
            var newParticipant = new Participants
            {
                IdEvent = selectedItem.IdEvent,
                IdUser = _currentUser.IdUser,
                IdStatus = (int)ParticipantsStatuses.SignedUp,
                RegistrationDate = DateTime.Now
            };
            context.Participants.Add(newParticipant);
            context.SaveChanges();

            MBClass.InformationMB("Вы успешно записались на мероприятие!");

            // Обновляем список (чтобы пересчитать количество участников)
            LoadEvents();
        }
    }
}
