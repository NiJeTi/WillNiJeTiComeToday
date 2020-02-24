using Microsoft.Extensions.DependencyInjection;

using ScheduleParser;

using Serilog;

namespace TelegramBot
{
    internal static class ServicesExtensions
    {
        public static IServiceCollection AddScheduleParser(this IServiceCollection services, string scheduleFile)
        {
            var parserCore = new ParserCore();

            parserCore.SetUp($"Resources/{scheduleFile}");

            Log.Information("Parser setup successful");

            return services.AddSingleton(parserCore);
        }
    }
}