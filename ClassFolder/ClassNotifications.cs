using System;


namespace EventApp.ClassFolder
{
    internal class ClassNotifications
    {
        public int IdNotification { get; set; }
        public int IdUser { get; set; }
        public int EventId { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
    }
}
