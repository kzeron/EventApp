using EventApp.DataFolder;
using System.Linq;
using System.Windows.Controls;

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
            var context = EventEntities.GetContext();
            
            // Получаем список пользователей, записанных на мероприятие
            var participantsData = (from p in context.Participants
                                    join u in context.User on p.IdUser equals u.IdUser
                                    where p.IdEvent == _eventId
                                    select new
                                    {
                                        u.Login,
                                        u.Email,
                                        u.Phone,
                                        p.RegistrationDate
                                        }).ToList();

                ParticipantsDataGrid.ItemsSource = participantsData;
        }
    }
}

