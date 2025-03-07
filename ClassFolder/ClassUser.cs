
namespace EventApp.ClassFolder
{
    public class ClassUser
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public int IDRole { get; set; }
        public Statuses Statuses { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int IdTrainer { get; set; }
    }
    public enum UserRole
    {
        Admin = 1,
        Teacher = 2,
        Participant = 3
    }
    public enum Statuses
    {
        Working = 1,
        Fired = 2,

    }
}
