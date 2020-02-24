using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;

using System;
using System.Globalization;
using System.Threading.Tasks;

using Telegram.Bot;

using TelegramBot.Resources.Localizations;

namespace TelegramBot
{
    internal static class Startup
    {
        private const string LogTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}\n{Exception}";

        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .WriteTo.Console(LogEventLevel.Information, LogTemplate)
                         .WriteTo.File(new MessageTemplateTextFormatter(LogTemplate),
                                       "Logs/Application.log",
                                       LogEventLevel.Information)
                         .CreateLogger();

            Log.Information("APPLICATION STARTING...");

            var builder = new HostBuilder()
                          .UseSystemd()
                          .ConfigureAppConfiguration((context, configuration) => configuration.AddJsonFile("Properties/appsettings.json"))
                          .ConfigureServices((context, services) =>
                          {
                              services.AddOptions()
                                      .AddHostedService<BotService>()
                                      .AddSingleton<ITelegramBotClient>(new TelegramBotClient(context.Configuration["TelegramBot:ApiToken"]))
                                      .AddScheduleParser(context.Configuration["Parser:ScheduleFile"]);

                              LocalizationManager.Language = new CultureInfo(context.Configuration["Localization:Language"]);
                              LocalizationManager.Culture  = new CultureInfo(context.Configuration["Localization:Culture"]);
                          });

            using var host = builder.Build();

            try
            {
                await host.StartAsync();

                Log.Information("APPLICATION STARTED");
                Log.Information("Press Ctrl+C to stop");
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "APPLICATION FAILED TO START");
            }

            try
            {
                await host.WaitForShutdownAsync();

                Log.Information("APPLICATION FINISHED");
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}