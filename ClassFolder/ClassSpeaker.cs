using System.Windows.Media.Imaging;

namespace EventApp.ClassFolder
{
    public class ClassSpeaker
    {
        public int IdUser { get; set; }
        public int IdSpeaker { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public int IDRole { get; set; }
        public Statuses Statuses { get; set; }
        public string StatusName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public BitmapImage Photo { get; set; }

        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
    }
}
