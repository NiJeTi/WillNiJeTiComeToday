using ScheduleParser;

using System;
using System.IO;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Types;

using TelegramBot.Resources.Localizations;

using File = System.IO.File;

namespace TelegramBot.Commands.Control
{
    internal sealed class UpdateSchedule : ICommand
    {
        private const string FileSavePath = "Resources/";

        private readonly ParserCore         parserCore;
        private readonly ITelegramBotClient bot;
        private readonly CancellationToken  cancellationToken;

        private readonly long chatId;

        public string Response { get; private set; }

        public UpdateSchedule(ParserCore parserCore, ITelegramBotClient bot, CancellationToken cancellationToken, long chatId)
        {
            this.parserCore        = parserCore;
            this.bot               = bot;
            this.cancellationToken = cancellationToken;

            this.chatId = chatId;
            
            Response = string.Empty;
        }

        public void Prepare()
        {
            Response = LocalizationManager.GetLocalizedText("FileRequest", chatId);
        }

        public void Handle(object query)
        {
            if (!(query is Message))
                throw new NotSupportedException("Only message queries supported");

            var message = (Message) query;

            if (message.Document != null)
            {
                if (Path.GetExtension(message.Document.FileName).ToLower() != ".xlsx")
                {
                    Response = LocalizationManager.GetLocalizedText("FileWrongType", chatId);

                    return;
                }

                var    recievedFile = bot.GetFileAsync(message.Document.FileId, cancellationToken).GetAwaiter().GetResult();
                string filePath     = FileSavePath + message.Document.FileName;

                using (var fileSaveStream = File.Open(filePath, FileMode.Create))
                    bot.DownloadFileAsync(recievedFile.FilePath, fileSaveStream, cancellationToken).GetAwaiter().GetResult();

                parserCore.SetUp(filePath);

                Response = LocalizationManager.GetLocalizedText("ScheduleUpdated", chatId);
            }
            else
            {
                Response = LocalizationManager.GetLocalizedText("NoDocument", chatId);
            }
        }
    }
}