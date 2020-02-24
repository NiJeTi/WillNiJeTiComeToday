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

        private readonly string subjectName;

        private readonly long chatId;

        public string Response { get; private set; }

        public Check(ParserCore parserCore, string subjectName, long chatId)
        {
            this.parserCore = parserCore;

            this.subjectName = subjectName;

            this.chatId = chatId;

            Response = string.Empty;
        }

        public void Prepare() => Response = string.Format(LocalizationManager.GetLocalizedText("CheckCommandPrepare", chatId),
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
                    Response = string.Format(LocalizationManager.GetLocalizedText("NoWork", chatId),
                                             subjectName,
                                             parsedDate.Date.ToString(LocalizationManager.Culture.DateTimeFormat.ShortDatePattern));
                }
                else
                {
                    var responseBuilder = new StringBuilder();

                    responseBuilder.AppendLine(string.Format(LocalizationManager.GetLocalizedText("HasWork", chatId),
                                                             subjectName,
                                                             parsedDate.Date.ToString(LocalizationManager.Culture.DateTimeFormat.ShortDatePattern)));

                    responseBuilder.AppendLine(string.Format(LocalizationManager.GetLocalizedText("FromToFor", chatId),
                                                             TimeToShortString(schedule.BeginTime ?? throw new NullReferenceException()),
                                                             TimeToShortString(schedule.EndTime ?? throw new NullReferenceException()),
                                                             TimeToLongString(schedule.Duration.Value)));

                    if (schedule.AdditionalDuration != null)
                    {
                        responseBuilder.AppendLine(LocalizationManager.GetLocalizedText("And", chatId));

                        responseBuilder.AppendFormat(LocalizationManager.GetLocalizedText("WorkTimeAdditional", chatId),
                                                     TimeToShortString(schedule.AdditionalBeginTime ?? throw new NullReferenceException()),
                                                     TimeToShortString(schedule.AdditionalEndTime ?? throw new NullReferenceException()),
                                                     TimeToLongString(schedule.AdditionalDuration.Value));
                    }

                    Response = responseBuilder.ToString();
                }
            }
            else
            {
                Response = LocalizationManager.GetLocalizedText("NotADate", chatId);
            }
        }

        private static string TimeToShortString(TimeSpan time)
        {
            int hours   = time.Hours;
            int minutes = time.Minutes;

            return $"{hours:D2}:{minutes:D2}";
        }

        private string TimeToLongString(TimeSpan time)
        {
            var output = new StringBuilder();

            if (time.Hours != 0)
                output.AppendFormat(LocalizationManager.GetLocalizedText("Hours", chatId), time.Hours);

            if (time.Minutes != 0)
            {
                if (time.Hours != 0)
                    output.Append(' ');

                output.AppendFormat(LocalizationManager.GetLocalizedText("Minutes", chatId), time.Minutes);
            }

            return output.ToString().Trim();
        }
    }
}