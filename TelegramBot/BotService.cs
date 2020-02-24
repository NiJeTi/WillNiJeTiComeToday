using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using ScheduleParser;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Display;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using TelegramBot.Commands;
using TelegramBot.Commands.Control;
using TelegramBot.Resources.Localizations;

namespace TelegramBot
{
    internal sealed class BotService : BackgroundService
    {
        private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}\n";

        private readonly ILogger logger;
        private readonly ILogger messageLogger;

        private readonly ITelegramBotClient bot;

        private readonly string   subjectName;
        private readonly string[] admins;
        private readonly string[] whitelistedUsers;

        private readonly ParserCore parserCore;

        private readonly Dictionary<long, ICommand> longPollQueue;

        private CancellationToken executionCancellationToken;

        public BotService(IConfiguration configuration, ITelegramBotClient bot, ParserCore parserCore)
        {
            logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.Console(LogEventLevel.Information, LogTemplate)
                     .WriteTo.File(new MessageTemplateTextFormatter(LogTemplate),
                                   "Logs/Bot.log",
                                   LogEventLevel.Information)
                     .CreateLogger();

            messageLogger = new LoggerConfiguration()
                            .Enrich.FromLogContext()
                            .WriteTo.File(new RenderedCompactJsonFormatter(),
                                          "Logs/MessageLog.json",
                                          LogEventLevel.Information,
                                          rollOnFileSizeLimit: true)
                            .CreateLogger();

            this.bot = bot;

            subjectName      = configuration.GetSection("TelegramBot:SubjectName").Get<string>();
            admins           = configuration.GetSection("TelegramBot:Admins").Get<string[]>();
            whitelistedUsers = configuration.GetSection("TelegramBot:WhitelistedUsers").Get<string[]>();

            this.parserCore = parserCore;

            longPollQueue = new Dictionary<long, ICommand>();

            Log.Information("Telegram bot service setup successful");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            executionCancellationToken = cancellationToken;

            bot.OnMessage       += OnMessage;
            bot.OnCallbackQuery += OnLanguageSelected;
            bot.StartReceiving(cancellationToken: executionCancellationToken);

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(1000, executionCancellationToken);

            bot.StopReceiving();
        }

        private async void OnMessage(object? sender, MessageEventArgs e)
        {
            var message = e.Message;

            if (whitelistedUsers.Contains(message.Chat.Username) || admins.Contains(message.Chat.Username))
            {
                if (longPollQueue.TryGetValue(message.Chat.Id, out var command))
                    await HandleCommand(command, message, message.Chat);
                else
                    await SwitchCommand(message);
            }
            else
            {
                LogUnknownMessage(message);

                await Response(message.Chat, LocalizationManager.GetLocalizedText("StrangerResponse", message.Chat.Id));
            }
        }

        private async Task SwitchCommand(Message message)
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                switch (message.Text)
                {
                    case "/today":
                        await InitializeCommand(new Check(parserCore, subjectName, message.Chat.Id), message, true, DateTime.Today);

                        break;

                    case "/tomorrow":
                        await InitializeCommand(new Check(parserCore, subjectName, message.Chat.Id), message, true, DateTime.Today.AddDays(1d).Date);

                        break;

                    case "/check":
                        await InitializeCommand(new Check(parserCore, subjectName, message.Chat.Id), message);

                        break;
                    case "/language":
                        var langugageSelector = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(LocalizationManager.GetLocalizedText("LanguageEng", message.Chat.Id), "en-US"),
                                InlineKeyboardButton.WithCallbackData(LocalizationManager.GetLocalizedText("LanguageRus", message.Chat.Id), "ru-RU")
                            }
                        });

                        await Response(message.Chat, LocalizationManager.GetLocalizedText("LanguageSelect", message.Chat.Id), langugageSelector);

                        break;
                    case "/update":
                        if (!admins.Contains(message.Chat.Username))
                            await Response(message.Chat, LocalizationManager.GetLocalizedText("NoPermission", message.Chat.Id));
                        else
                            await InitializeCommand(new UpdateSchedule(parserCore, bot, executionCancellationToken, message.Chat.Id), message);

                        break;
                    default:
                        await Response(message.Chat, LocalizationManager.GetLocalizedText("Usage", message.Chat.Id));

                        break;
                }
            }
            else
            {
                await Response(message.Chat, LocalizationManager.GetLocalizedText("Usage", message.Chat.Id));
            }
        }

        private async Task InitializeCommand(ICommand command, Message message, bool instantResponse = false, object? query = null)
        {
            logger.Information($"{GetUsername(message.Chat)} requested '{message.Text}'");

            command.Prepare();

            if (instantResponse)
            {
                if (query is null)
                    throw new ArgumentException("When using command with instant response, query should be passed too");

                await HandleCommand(command, query, message.Chat);
            }
            else
            {
                await Response(message.Chat, command.Response);

                longPollQueue.Add(message.Chat.Id, command);
            }
        }

        private async Task HandleCommand(ICommand command, object query, Chat chat)
        {
            try
            {
                command.Handle(query);

                await Response(chat, command.Response);

                longPollQueue.Remove(chat.Id);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Context has been successfully restored");
            }
        }

        private async void OnLanguageSelected(object? sender, CallbackQueryEventArgs e)
        {
            var query = e.CallbackQuery;

            LocalizationManager.SetLanguage(query.Message.Chat.Id, new CultureInfo(query.Data));

            await Response(query.Message.Chat, LocalizationManager.GetLocalizedText("LanguageSelected", query.Message.Chat.Id));
        }

        private async Task Response(Chat responseChat, string responseText, IReplyMarkup? replyMarkup = null)
        {
            await bot.SendTextMessageAsync(responseChat,
                                           responseText,
                                           ParseMode.Markdown,
                                           replyMarkup: replyMarkup,
                                           cancellationToken: executionCancellationToken);

            logger.Information($"Responded to {GetUsername(responseChat)}:\n{responseText}");
        }

        private void LogUnknownMessage(Message recievedMessage)
        {
            logger.Warning($"Message from {GetUsername(recievedMessage.Chat)} [UNKNOWN]");
            messageLogger.Warning("{@message}", recievedMessage);
        }

        private static string GetUsername(Chat user) => user.Username == null ? $"[id{user.Id}] {user.FirstName} {user.LastName}" : $"@{user.Username}";
    }
}