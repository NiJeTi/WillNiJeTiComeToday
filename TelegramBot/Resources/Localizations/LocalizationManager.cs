using System;
using System.Collections.Generic;
using System.Globalization;

namespace TelegramBot.Resources.Localizations
{
    internal static class LocalizationManager
    {
        public static CultureInfo DefaultLanguage { get; set; }
        public static CultureInfo Culture         { get; set; }

        private static readonly Dictionary<long, CultureInfo> _languagePreferences;

        static LocalizationManager()
        {
            DefaultLanguage = CultureInfo.CurrentCulture;
            Culture         = CultureInfo.CurrentCulture;

            _languagePreferences = new Dictionary<long, CultureInfo>();
        }

        public static void SetLanguage(long chatId, CultureInfo language)
        {
            if (!_languagePreferences.TryAdd(chatId, language))
                _languagePreferences[chatId] = language;
        }

        public static string GetLocalizedText(string name, long chatId)
        {
            string localizedString;

            if (_languagePreferences.TryGetValue(chatId, out var language))
            {
                localizedString = Localization.ResourceManager.GetString(name, language) ??
                                  throw new KeyNotFoundException("There is no localized string with given name");
            }
            else
            {
                localizedString = Localization.ResourceManager.GetString(name, DefaultLanguage) ??
                                  throw new KeyNotFoundException("There is no localized string with given name");
            }

            return localizedString;
        }

        public static (bool result, DateTime parsedDate) TryParseDate(string dateString)
        {
            bool result = DateTime.TryParse(dateString, Culture, DateTimeStyles.None, out var parsedDate);

            return (result, parsedDate);
        }
    }
}