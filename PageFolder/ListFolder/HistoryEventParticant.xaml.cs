using EventApp.ClassFolder;
using EventApp.DataFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для HistoryEventParticant.xaml
    /// </summary>
    public partial class HistoryEventParticant : Page
    {
        private readonly int _employeeId;
        public HistoryEventParticant(int employeeId)
        {
            InitializeComponent();
            _employeeId= employeeId;
            LoadParticipantEvents();
        }
        private void LoadParticipantEvents()
        {
            var ctx = EventEntities.GetContext();

            // Проверяем, есть ли у этого сотрудника записи в Participants
            bool any = ctx.Participants.Any(p => p.IdEmploee == _employeeId);
            if (!any)
            {
                MBClass.InformationMB("У данного сотрудника еще нет ни одной записи на мероприятие.");
                ParticipantEventsList.ItemsSource = null;
                return;
            }

            var list = (from p in ctx.Participants
                        where p.IdEmploee == _employeeId
                        join ev in ctx.Events on p.IdEvent equals ev.IdEvent
                        join ps in ctx.Status on p.IdStatus equals ps.IdStatus
                        orderby p.RegistrationDate descending
                        select new
                        {
                            ev.Title,  // если есть
                            ev.DateStart,
                            ParticipantStatus = ps.NameStatus,
                            RegistrationDate = p.RegistrationDate
                        })
                       .ToList();

            ParticipantEventsList.ItemsSource = list;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Если вы храните NavigationService
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
