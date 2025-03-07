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
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace EventApp.PageFolder.ListFolder
{
    /// <summary>
    /// Логика взаимодействия для ListEvent.xaml
    /// </summary>
    public partial class ListEvent : Page
    {
        private ObservableCollection<ClassEvent> _events;
        public ListEvent()
        {
            InitializeComponent();
            _events = new ObservableCollection<ClassEvent>();
            LoadEvents();
        }
        private void LoadEvents()
        {
            var context = EventEntities.GetContext();

            var eventData =(from ev in context.Events
                            join loc in context.Locations
                                    on ev.LocationId equals loc.IdLocation into locGroup
                            from loc in locGroup.DefaultIfEmpty()
                            select new
                            {
                                ev.IdEvent,
                                ev.Title,
                                ev.Description,
                                ev.DateStart,
                                ev.EndDate,
                                locId = loc != null ? loc.IdLocation : (int?)null,
                                locName = loc != null ? loc.LocationName : null,
                                address = loc != null ? loc.Address : null,
                                loc.NumberCab,
                                capacity = loc != null ? loc.Capacity : (int?)null,
                                ev.OrganizerId
                            })
                            .OrderBy(e => e.IdEvent)
                            .ToList();
            _events.Clear();

            foreach (var item in eventData)
            {
                _events.Add(new ClassEvent
                {
                    IdEvent = item.IdEvent,
                    Title = item.Title,
                    Description = item.Description,
                    DateStart = item.DateStart,
                    EndDate = item.EndDate,
                    LocationId = item.locId,
                    LocationName = item.locName,
                    Address = item.address,
                    NumberCab = item.NumberCab,
                    Capacity = item.capacity,
                    OrganizerId = item.OrganizerId
                });
            }
        }
    }
}
