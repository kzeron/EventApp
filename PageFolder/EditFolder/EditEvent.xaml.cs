using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EventApp.DataFolder;
using EventApp.ClassFolder;
using EventApp.WindowFolder;

namespace EventApp.PageFolder.EditFolder
{
    public partial class EditEvent : Page
    {
        private readonly EventEntities _ctx = EventEntities.GetContext();
        private Events _event;
        private Address _address;
        private Locations _location;

        public EditEvent(int eventId)
        {
            InitializeComponent();
            LoadCities();
            LoadEvent(eventId);
        }

        private void LoadCities()
        {
            CityCb.ItemsSource = _ctx.City.ToList();
        }

        private void CityCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityCb.SelectedValue is int cityId)
            {
                StreetCb.ItemsSource = _ctx.Street.Where(s => s.IdCity == cityId).ToList();
                StreetCb.IsEnabled = true;
                HouseCb.ItemsSource = null;
                HouseCb.IsEnabled = false;
            }
        }

        private void StreetCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StreetCb.SelectedValue is int streetId)
            {
                // список домов — просто строки
                HouseCb.ItemsSource = _ctx.Address
                                       .Where(a => a.IdStreet == streetId)
                                       .Select(a => a.HouseNumber)
                                       .Distinct()
                                       .ToList();
                HouseCb.IsEnabled = true;
            }
        }

        private void LoadEvent(int eventId)
        {
            _event = _ctx.Events.FirstOrDefault(ev => ev.IdEvent == eventId);
            if (_event == null)
            {
                MBClass.ErrorMB("Мероприятие не найдено.");
                return;
            }

            // 1) Простые поля
            TitleTb.Text = _event.Title;
            DescriptionTb.Text = _event.Description;
            StartDatePicker.SelectedDate = _event.DateStart;
            EndDatePicker.SelectedDate = _event.EndDate;

            // 2) Адрес/локация
            if (_event.LocationId.HasValue)
            {
                _location = _ctx.Locations
                                .FirstOrDefault(l => l.IdLocation == _event.LocationId.Value);

                if (_location != null && _location.IdAddress.HasValue)
                {
                    _address = _ctx.Address
                                   .FirstOrDefault(a => a.IdAddress == _location.IdAddress.Value);
                }
            }

            if (_address != null)
            {
                // выставляем город → улицу → дом
                var street = _ctx.Street.FirstOrDefault(s => s.IdStreet == _address.IdStreet);
                if (street != null)
                {
                    CityCb.SelectedValue = street.IdCity;
                    StreetCb.SelectedValue = street.IdStreet;
                    HouseCb.SelectedItem = _address.HouseNumber;
                }
            }

            // 3) Кабинет и вместимость
            if (_location != null)
            {
                NumberCabTb.Text = _location.NumberCab.ToString();
                CapacityTb.Text = _location.Capacity.ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTb.Text))
            {
                MBClass.WarningMB("Введите название мероприятия.");
                return;
            }

            // --- валидация ---
            if (string.IsNullOrWhiteSpace(TitleTb.Text))
            {
                MBClass.WarningMB("Введите название.");
                return;
            }

            if (CityCb.SelectedValue == null ||
                StreetCb.SelectedValue == null ||
                HouseCb.SelectedItem == null)
            {
                MBClass.WarningMB("Укажите город, улицу и дом.");
                return;
            }

            if (!int.TryParse(NumberCabTb.Text, out int numCab))
            {
                MBClass.WarningMB("Номер кабинета должен быть числом.");
                return;
            }

            if (!int.TryParse(CapacityTb.Text, out int cap) || cap <= 0)
            {
                MBClass.WarningMB("Вместимость должна быть положительным числом.");
                return;
            }

            // --- создание/поиск Address ---
            int selStreet = (int)StreetCb.SelectedValue;
            string selHouse = (string)HouseCb.SelectedItem;

            if (_address == null ||
                _address.IdStreet != selStreet ||
                _address.HouseNumber != selHouse)
            {
                _address = _ctx.Address
                               .FirstOrDefault(a => a.IdStreet == selStreet && a.HouseNumber == selHouse)
                           ?? new Address { IdStreet = selStreet, HouseNumber = selHouse };

                if (_address.IdAddress == 0)
                {
                    _ctx.Address.Add(_address);
                    _ctx.SaveChanges();
                }
            }

            // --- создание/поиск Locations ---
            if (_location == null ||
                _location.IdAddress != _address.IdAddress ||
                _location.NumberCab != numCab ||
                _location.Capacity != cap)
            {
                _location = _ctx.Locations
                               .FirstOrDefault(l => l.IdAddress == _address.IdAddress
                                                 && l.NumberCab == numCab
                                                 && l.Capacity == cap)
                           ?? new Locations
                           {
                               IdAddress = _address.IdAddress,
                               NumberCab = numCab,
                               Capacity = cap,
                               LocationName = $"{CityCb.Text}, {StreetCb.Text}, дом {selHouse}"
                           };

                if (_location.IdLocation == 0)
                {
                    _ctx.Locations.Add(_location);
                    _ctx.SaveChanges();
                }
            }

            // --- сохраняем мероприятие ---
            _event.Title = TitleTb.Text;
            _event.Description = DescriptionTb.Text;
            _event.DateStart = StartDatePicker.SelectedDate;
            _event.EndDate = EndDatePicker.SelectedDate;
            _event.LocationId = _location.IdLocation;

            _ctx.SaveChanges();

            MBClass.InformationMB("Изменения сохранены!");
            (Window.GetWindow(this) as WindowMain)?.CloseModal();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.GetWindow(this) as WindowMain)?.CloseModal();
        }
    }
}
