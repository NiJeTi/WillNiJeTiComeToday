using ScheduleParser;

using System;
using System.Text;

using Telegram.Bot.Types;

using TelegramBot.Resources.Localizations;

namespace TelegramBot.Commands
{
    internal sealed class Check : ICommand
    {
        private readonly ParserCore parserCore;

        public string Response { get; private set; }

        public Check(ParserCore parserCore)
        {
            this.parserCore = parserCore;

            Response = string.Empty;
        }

        public void Prepare() => Response = string.Format(LocalizationManager.GetLocalizedText("CheckCommandPrepare"),
                                                          LocalizationManager.Culture.DateTimeFormat.ShortDatePattern);

        public void Handle(object query)
        {
            DateTime parsedDate;
            bool     dateParseResult;

            switch (query)
            {
                case Message message:
                    (dateParseResult, parsedDate) = LocalizationManager.TryParseDate(message.Text);

                    break;
                case DateTime date:
                    parsedDate      = date;
                    dateParseResult = true;

                    break;
                default:
                    throw new NotSupportedException("This type of query isn't supported in this command");
            }

            if (dateParseResult)
            {
                var schedule = parserCore.GetScheduleForDay(parsedDate);

                if (schedule.Duration is null)
                {
                    Response = string.Format(LocalizationManager.GetLocalizedText("NoWork"),
                                             parsedDate.Date.ToString(LocalizationManager.Culture.DateTimeFormat.ShortDatePattern));
                }
                else
                {
                    var responseBuilder = new StringBuilder();

                    responseBuilder.AppendLine(string.Format(LocalizationManager.GetLocalizedText("HasWork"),
                                                             parsedDate.Date.ToString(LocalizationManager.Culture.DateTimeFormat.ShortDatePattern)));

                    responseBuilder.AppendLine(string.Format(LocalizationManager.GetLocalizedText("FromToFor"),
                                                             TimeToShortString(schedule.BeginTime ?? throw new NullReferenceException()),
                                                             TimeToShortString(schedule.EndTime ?? throw new NullReferenceException()),
                                                             TimeToLongString(schedule.Duration.Value)));

                    if (schedule.AdditionalDuration != null)
                    {
                        responseBuilder.AppendLine(LocalizationManager.GetLocalizedText("And"));

                        responseBuilder.AppendFormat(LocalizationManager.GetLocalizedText("WorkTimeAdditional"),
                                                     TimeToShortString(schedule.AdditionalBeginTime ?? throw new NullReferenceException()),
                                                     TimeToShortString(schedule.AdditionalEndTime ?? throw new NullReferenceException()),
                                                     TimeToLongString(schedule.AdditionalDuration.Value));
                    }

                    Response = responseBuilder.ToString();
                }
            }
            else
            {
                Response = LocalizationManager.GetLocalizedText("NotADate");
            }
        }

        private static string TimeToShortString(TimeSpan time)
        {
            int hours   = time.Hours;
            int minutes = time.Minutes;

            return $"{hours:D2}:{minutes:D2}";
        }

        private static string TimeToLongString(TimeSpan time)
        {
            var output = new StringBuilder();

            if (time.Hours != 0)
                output.AppendFormat(LocalizationManager.GetLocalizedText("Hours"), time.Hours);

            if (time.Minutes != 0)
            {
                if (time.Hours != 0)
                    output.Append(' ');

                output.AppendFormat(LocalizationManager.GetLocalizedText("Minutes"), time.Minutes);
            }

            return output.ToString().Trim();
        }
    }
}