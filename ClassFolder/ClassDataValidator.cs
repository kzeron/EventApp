using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EventApp.ClassFolder
{
    internal class ClassDataValidator
    {
        public static bool IsPurchaseBeforeWarranty(DateTime startDate, DateTime endDate)
        {
            return startDate <= endDate;
        }
        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            string pattern = @"^\+?[0-9]+$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        // Проверка: Валидность email (допустимые домены)
        public static bool IsEmailValid(string email)
        {
            string pattern = @"^[^@\s]+@(?:gmail\.com|yandex\.ru|mail\.ru)$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
    }
}
