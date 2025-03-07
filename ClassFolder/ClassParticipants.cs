using System;

namespace EventApp.ClassFolder
{
    internal class ClassParticipants
    {
        public int IdParticipants { get; set; }
        public int IdEvent {  get; set; }
        public int IdUser { get; set; }
        public int IdStatus { get; set; }
        public DateTime RegistrationDate { get; set; }

        public enum ParticipantsStatuses
        {
            SignedUp = 3,      // Записан на мероприятие
            Absent = 4,        // Отсутствует
            Sick = 5,          // Болеет
            Gathering = 6,     // Сбор на мероприятие
            Started = 7,       // Началось
            InProgress = 8,    // Проходит
            Completed = 9,     // Завершилось
            Cancelled = 10     // Отменено
        }
    }
}
