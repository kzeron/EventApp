using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EventApp.PageFolder.AddFolder
{
    /// <summary>
    /// Логика взаимодействия для AddEvent.xaml
    /// </summary>
    public partial class AddEvent : Page
    {
        public AddEvent()
        {
            InitializeComponent();
            LocationCb.ItemsSource = EventEntities.GetContext().Locations.ToList();
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Считываем данные
            var title = TitleTb.Text;
            var description = DescriptionTb.Text;
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;
            var selectedLocation = LocationCb.SelectedValue != null ? Int32.Parse(LocationCb.SelectedValue.ToString()) : (int?)null;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название.");
                return;
            }

            var context = EventEntities.GetContext();
            var newEvent = new Events
            {
               Title = title,
               Description = description,
               DateStart = startDate,
               EndDate = endDate,
               LocationId = selectedLocation
               // Здесь можно указать LocationId, OrganizerId, если нужно
            };

                context.Events.Add(newEvent);
                context.SaveChanges();
            

            MBClass.InformationMB("Мероприятие добавлено!");
            // Закрываем модальное окно
            // Закрываем модальное окно
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.CloseModal();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем модальное окно
            WindowMain mainWindow = Window.GetWindow(this) as WindowMain;
            if (mainWindow != null)
            {
                mainWindow.CloseModal();
            }
        }

    }
}
