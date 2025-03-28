﻿using System;
using System.Text.RegularExpressions;

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
