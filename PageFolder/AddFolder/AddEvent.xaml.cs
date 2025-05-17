using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventApp.ClassFolder;
using EventApp.DataFolder;
using EventApp.WindowFolder;

namespace EventApp.PageFolder.AddFolder
{
    public partial class AddEvent : Page
    {
        private readonly EventEntities _ctx;

        public AddEvent()
        {
            InitializeComponent();
            _ctx = EventEntities.GetContext();
            LoadCities();
        }

        private void LoadCities()
        {
            CityCb.ItemsSource = _ctx.City.ToList();
        }

        private void CityCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityCb.SelectedValue is int cityId)
            {
                // Загружаем улицы выбранного города
                StreetCb.ItemsSource = _ctx.Street
                                          .Where(s => s.IdCity == cityId)
                                          .ToList();
                StreetCb.IsEnabled = true;
                HouseCb.IsEnabled = false;
                HouseCb.ItemsSource = null;
            }
        }

        private void StreetCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StreetCb.SelectedValue is int streetId)
            {
                // Загружаем номера домов для выбранной улицы
                HouseCb.ItemsSource = _ctx.Address
                                        .Where(a => a.IdStreet == streetId)
                                        .Distinct()
                                        .ToList();

                HouseCb.IsEnabled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (CityCb.SelectedValue == null ||
                StreetCb.SelectedValue == null ||
                HouseCb.SelectedValue == null)
            {
                MBClass.WarningMB("Выберите город, улицу и номер дома.");
                return;
            }

            // Получаем или создаём Address
            int selectedStreetId = (int)StreetCb.SelectedValue;
            string selectedHouse = HouseCb.SelectedValue.ToString();

            var address = _ctx.Address
                              .FirstOrDefault(a => a.IdStreet == selectedStreetId
                                                && a.HouseNumber == selectedHouse);
            if (address == null)
            {
                address = new Address
                {
                    IdStreet = selectedStreetId,
                    HouseNumber = selectedHouse
                };
                _ctx.Address.Add(address);
                _ctx.SaveChanges();
            }

            // 1) Считываем и проверяем NumberCab
            if (string.IsNullOrWhiteSpace(NumberCabTb.Text) ||
                !int.TryParse(NumberCabTb.Text, out int numberCab))
            {
                MBClass.WarningMB("Введите корректный номер кабинета.");
                return;
            }

            // 2) Считываем и проверяем Capacity
            if (string.IsNullOrWhiteSpace(CapacityTb.Text) ||
                !int.TryParse(CapacityTb.Text, out int capacity) ||
                capacity <= 0)
            {
                MBClass.WarningMB("Введите положительную вместимость.");
                return;
            }
            // Создаём новую локацию, привязываем к address
            var newLocation = new Locations
            {
                LocationName = $"{CityCb.Text}, {StreetCb.Text}, дом {HouseCb.Text}",
                NumberCab = numberCab,
                Capacity = capacity,
                IdAddress = address.IdAddress
            };
            _ctx.Locations.Add(newLocation);
            _ctx.SaveChanges();

            // Теперь создаём само мероприятие
            var newEvent = new Events
            {
                Title = TitleTb.Text,
                Description = DescriptionTb.Text,
                DateStart = StartDatePicker.SelectedDate,
                EndDate = EndDatePicker.SelectedDate,
                StatusId = (int)EventStatuses.Collecting
            };
            _ctx.Events.Add(newEvent);
            _ctx.SaveChanges();

            MBClass.InformationMB("Мероприятие добавлено!");
            // Закрываем модалку
            (Window.GetWindow(this) as WindowMain)?.CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as WindowMain)?.CloseModal();
        }
    }
}
