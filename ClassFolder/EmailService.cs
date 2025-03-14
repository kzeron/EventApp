using System;
using System.Threading.Tasks;
using EventApp.DataFolder;
using MailKit.Net.Smtp;
using MimeKit;

namespace EventApp.ClassFolder
{
    internal class EmailService
    {
        //private readonly EventEntities _context;
        //private readonly EmailService _emailService;

        //public NotificationService(EventEntities context, EmailService emailService)
        //{
        //    _context = context;
        //    _emailService = emailService;
        //}

        //public async Task SendEventRemindersAsync()
        //{
        //    DateTime now = DateTime.Now;
        //    DateTime threshold = now.AddMinutes(30); // За 30 минут до начала

        //    // Найти мероприятия, начинающиеся в ближайшие 30 минут
        //    var upcomingEvents = await _context.Events
        //        .Where(e => e.StartTime >= now && e.StartTime <= threshold)
        //        .ToListAsync();

        //    foreach (var eventItem in upcomingEvents)
        //    {
        //        // Найти организатора мероприятия
        //        var organizer = await _context.User.FindAsync(eventItem.OrganizerId);
        //        if (organizer != null)
        //        {
        //            await SendReminder(eventItem, organizer);
        //        }

        //        // Найти всех участников мероприятия
        //        var participants = await _context.EventParticipants
        //            .Where(ep => ep.EventId == eventItem.Id)
        //            .Select(ep => ep.User)
        //            .ToListAsync();

        //        foreach (var participant in participants)
        //        {
        //            await SendReminder(eventItem, participant);
        //        }
        //    }
        //}

        //private async Task SendReminder(Events eventItem, User user)
        //{
        //    string subject = $"Скоро начнется мероприятие: {eventItem.Title}";
        //    string body = $"Привет, {user.Name}! Напоминаем, что мероприятие \"{eventItem.Title}\" начнется {eventItem.StartTime:G}.";

        //    bool isSent = await _emailService.SendEmailAsync(user.Email, subject, body);

        //    if (isSent)
        //    {
        //        var notification = new Notification
        //        {
        //            IdUser = user.Id,
        //            EventId = eventItem.Id,
        //            MessageType = "EventReminder",
        //            Message = body,
        //            SentDate = DateTime.Now
        //        };

        //        _context.Notifications.Add(notification);
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }
}
