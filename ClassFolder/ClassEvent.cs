using System;


namespace EventApp.ClassFolder
{
    internal class ClassEvent
    {
        public int IdEvents { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LocationId { get; set; }
        public int? OrganizerId { get; set; }
        public int EventTrainerID { get; set; }
        public int EventID { get; set; }
        public int TrainerID { get; set; }
        public int? StatusId { get; set; }
        public int UserId { get; set; }
        public int IdEvent { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public int? NumberCab { get; set; }
        public int? Capacity { get; set; }
        public int CurrentParticipants { get; set; }
    }
    public enum EventStatuses
    {
        Collecting = 6,
        Start = 7,
        Passed = 8,
        Ended = 9,
        Canceled = 10
    }
}
