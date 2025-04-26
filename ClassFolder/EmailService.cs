using System;
using System.Linq;
using EventApp.DataFolder;
using MailKit.Net.Smtp;
using MimeKit;

namespace EventApp.ClassFolder
{
    internal class EmailService
    {
        public static void SendRemindersForTomorrowEvents()
        {
            var tomorrow = DateTime.Today.AddDays(1);

            var context = EventEntities.GetContext();

            var tomorrowEvents = context.Events
                .Where(ev =>
                    ev.DateStart.HasValue &&
                    ev.StatusId != (int)EventStatuses.Canceled &&
                    ev.StatusId != (int)EventStatuses.Ended &&
                    ev.StatusId != (int)EventStatuses.OnGoing)
                .AsEnumerable() // Перенос обработки в память
                .Where(ev => ev.DateStart.Value.Date == tomorrow) // Теперь можно использовать Date
                .ToList();

            foreach (var ev in tomorrowEvents)
            {
                var participants = context.Participants
                    .Where(p => p.IdEvent == ev.IdEvent)
                    .Select(p => p.Employee)
                    .ToList();

                foreach (var user in participants)
                {
                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        SendReminderEmail(user.Email, ev);
                    }
                }
            }
        }


        private static void SendReminderEmail(string recipientEmail, Events ev)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EventApp", "from@example.com")); // Измените на реальный email отправителя
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = "Напоминание о мероприятии";

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $@"
Здравствуйте!

Напоминаем, что завтра ({ev.DateStart:dd.MM.yyyy}) начинается мероприятие '{ev.Title}'.
Описание: {ev.Description}
Место проведения (ID): {ev.LocationId}.

С уважением,
EventMaster"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.mailtrap.io", 2525, false);
                    client.Authenticate("a044ebbb37efaa", "c237ad173c8e64"); // Данные из MailTrap

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                MBClass.ErrorMB($"Ошибка при отправке письма на {recipientEmail}: {ex.Message}");
            }
        }
    }
}
