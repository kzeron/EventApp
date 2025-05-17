using EventApp.ClassFolder;
using EventApp.DataFolder;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static EventApp.ClassFolder.ClassParticipants;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для PageParticipants.xaml
    /// </summary>
    public partial class PageParticipants : Page
    {
        private int _eventId;
        public PageParticipants(int eventId)
        {
            InitializeComponent();
            _eventId = eventId;
            LoadParticipants();
        }
        private void LoadParticipants()
        {
            var ctx = EventEntities.GetContext();

            var now = DateTime.Now;
            var toMarkAbsent = (from p in ctx.Participants
                                join ev in ctx.Events on p.IdEvent equals ev.IdEvent
                                where p.IdEvent == _eventId
                                      && p.IdStatus == (int)ParticipantsStatuses.SignedUp
                                      && ev.DateStart < now
                                select p).ToList();

            if (toMarkAbsent.Any())
            {
                foreach (var p in toMarkAbsent)
                    p.IdStatus = (int)ParticipantsStatuses.Absent;
                ctx.SaveChanges();
            }

            var participantsData = (from p in ctx.Participants
                                    join emp in ctx.Employee on p.IdEmploee equals emp.EmployeeId
                                    join u in ctx.Users on emp.UserId equals u.IdUser
                                    join ps in ctx.Status on p.IdStatus equals ps.IdStatus
                                    join ev in ctx.Events on p.IdEvent equals ev.IdEvent
                                    where p.IdEvent == _eventId
                                    select new ClassParticipants
                                    {
                                        IdParticipants = p.IdParticipants,
                                        Name = emp.Name,
                                        Surname = emp.Surname,
                                        Patronymic = emp.Patronymic,
                                        Email = emp.Email,
                                        Phone = emp.Phone,
                                        IdEvent = p.IdEvent ?? -1,
                                        Title = ev.Title ?? "Без названия",
                                        StartEvent = ev.DateStart ?? DateTime.MinValue,
                                        IdUser = u.IdUser,
                                        IdStatus = p.IdStatus ?? -1,
                                        NameStatus = ps.NameStatus ?? "Неизвестно",
                                        RegistrationDate = p.RegistrationDate ?? DateTime.MinValue
                                    }).ToList();

            ParticipantsDataGrid.ItemsSource = participantsData;
        }


        private void MarkPresent_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus(sender, (int)ParticipantsStatuses.Attended);
        }

        private void MarkSick_Click(object sender, RoutedEventArgs e)
        {
            ChangeStatus(sender, (int)ParticipantsStatuses.Sick);
        }

        /// <summary>
        /// Универсальный метод смены статуса выбранного участника.
        /// </summary>
        /// <param name="menuItemSender">это тот же самый sender из MenuItem_Click</param>
        /// <param name="newStatusId">новый статус участника</param>
        private void ChangeStatus(object menuItemSender, int newStatusId)
        {
            // 1) Получаем MenuItem и проверяем
            var menuItem = menuItemSender as MenuItem;
            if (menuItem == null) return;

            // 2) Родитель — ContextMenu
            var cm = menuItem.Parent as ContextMenu;
            if (cm == null) return;

            // 3) PlacementTarget — строка DataGrid
            var row = cm.PlacementTarget as DataGridRow;
            if (row == null) return;

            // 4) Из строки достаём наш DTO
            var data = row.Item as ClassParticipants;
            if (data == null) return;

            // 5) Берём ключ
            int participantId = data.IdParticipants;

            // 6) Меняем статус в БД
            using (var ctx = EventEntities.GetContext())
            {
                var participant = ctx.Participants.Find(participantId);
                if (participant == null) return;

                participant.IdStatus = newStatusId;
                ctx.SaveChanges();
            }

            // 7) Обновляем грид
            LoadParticipants();
        }



    }
}

