using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventApp.ClassFolder
{
    public class ClassUser
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public int IDRole { get; set; }
        public Statuses Statuses { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
    public enum Role
    {
        Admin,
        Teacher,
        Participant
    }
    public enum Statuses
    {
        Working = 1,
        Fired = 2,

    }
}
