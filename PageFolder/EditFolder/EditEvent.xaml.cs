using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder.EditFolder
{
    /// <summary>
    /// Логика взаимодействия для EditEvent.xaml
    /// </summary>
    public partial class EditEvent : Page
    {
        private Events _event;

        public EditEvent(int eventId)
        {
            InitializeComponent();
            LoadEvent(eventId);
        }

        private void LoadEvent(int eventId)
        {
            var context = EventEntities.GetContext();
            _event = context.Events.FirstOrDefault(e => e.IdEvent == eventId);

            if (_event == null)
            {
                MBClass.ErrorMB("Мероприятие не найдено.");
                return;
            }

            // Заполняем поля
            TitleTb.Text = _event.Title;
            DescriptionTb.Text = _event.Description;
            StartDatePicker.SelectedDate = _event.DateStart;
            EndDatePicker.SelectedDate = _event.EndDate;

            LocationCb.ItemsSource = context.Locations.ToList();
            LocationCb.SelectedValue = _event.LocationId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_event == null)
            {
                MBClass.ErrorMB("Ошибка загрузки мероприятия.");
                return;
            }

            _event.Title = TitleTb.Text;
            _event.Description = DescriptionTb.Text;
            _event.DateStart = StartDatePicker.SelectedDate;
            _event.EndDate = EndDatePicker.SelectedDate;
            _event.LocationId = LocationCb.SelectedValue != null
                ? int.Parse(LocationCb.SelectedValue.ToString())
                : (int?)null;

            if (string.IsNullOrWhiteSpace(_event.Title))
            {
                MBClass.ErrorMB("Введите название мероприятия.");
                return;
            }

            EventEntities.GetContext().SaveChanges();

            MBClass.InformationMB("Изменения сохранены!");
            CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        private void CloseModal()
        {
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            mainWindow?.CloseModal();
        }
    }

}
