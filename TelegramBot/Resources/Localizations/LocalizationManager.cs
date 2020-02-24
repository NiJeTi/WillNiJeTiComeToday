using System;
using System.Collections.Generic;
using System.Globalization;

namespace TelegramBot.Resources.Localizations
{
    internal static class LocalizationManager
    {
        public static CultureInfo Language { get; set; }
        public static CultureInfo Culture  { get; set; }

        static LocalizationManager()
        {
            Language = CultureInfo.CurrentCulture;
            Culture  = CultureInfo.CurrentCulture;
        }

        public static string GetLocalizedText(string name)
        {
            string localizedString = Localization.ResourceManager.GetString(name, Language) ??
                                     throw new KeyNotFoundException("There is no localized string with given name");

            return localizedString;
        }

        public static (bool result, DateTime parsedDate) TryParseDate(string dateString)
        {
            bool result = DateTime.TryParse(dateString, Culture, DateTimeStyles.None, out var parsedDate);

            return (result, parsedDate);
        }
    }
}