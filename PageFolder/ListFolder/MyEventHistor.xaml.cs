using EventApp.ClassFolder;
using EventApp.DataFolder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static EventApp.ClassFolder.ClassParticipants;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для MyEventHistor.xaml
    /// </summary>
    public partial class MyEventHistor : Page
    {
        private readonly EventEntities _ctx = EventEntities.GetContext();
        private readonly int _employeeId;
        public MyEventHistor()
        {
            InitializeComponent();

            var session = ClassSaveSassion.LoadSession();
            if (session == null) throw new InvalidOperationException("Сессия не найдена");

            var emp = _ctx.Employee.FirstOrDefault(e => e.UserId == session.IdUser);
            if (emp == null) throw new InvalidOperationException("Профиль не найден");

            _employeeId = emp.EmployeeId;
            LoadHistory();
        }
        private void LoadHistory()
        {
            var list = (from p in _ctx.Participants
                        where p.IdEmploee == _employeeId
                        join ev in _ctx.Events on p.IdEvent equals ev.IdEvent
                        join ps in _ctx.Status on p.IdStatus equals ps.IdStatus
                        orderby ev.DateStart descending
                        select new
                        {
                            p.IdParticipants,
                            ev.Title,
                            ev.DateStart,
                            RegistrationDate = p.RegistrationDate,
                            RegistrationStatusName = ps.NameStatus,
                            CanUnregister = ev.DateStart > DateTime.Now
                        })
                       .ToList();

            HistoryGrid.ItemsSource = new ObservableCollection<dynamic>(list);
        }


        private void Unregister_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку и привязанный к ней анонимный объект
            var btn = (Button)sender;
            dynamic item = btn.DataContext;
            int partId = item.IdParticipants;

            // Находим запись в базе
            var p = _ctx.Participants.Find(partId);
            if (p == null) return;

            // Удаляем запись — это освободит место в списке участников
            _ctx.Participants.Remove(p);
            _ctx.SaveChanges();

            // Перезагружаем грид
            LoadHistory();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
