using NUnit.Framework;

using ScheduleParser;

using System;
using System.Globalization;
using System.IO;

namespace ScheduleParser_Testing
{
    [TestFixture]
    internal sealed class ParserCoreTests
    {
        private const string FirstScheduleFile  = "Schedule1.xlsx";
        private const string SecondScheduleFile = "Schedule2.xlsx";

        private const string FirstScheduleDay = "10.02.2020";
        private const string LastScheduleDay  = "31.05.2020";

        private const int TestingStartDay = 10;
        private const int TestingEndDay   = 21;
        private const int TestingMonth    = 2;
        private const int TestingYear     = 2020;

        private static readonly TimeSpan?[] _testingDurations =
        {
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(9),
            null,
            null,
            TimeSpan.FromHours(9),
            null,
            null,
            new TimeSpan(4, 50, 0),
            new TimeSpan(6, 20, 0),
            new TimeSpan(4, 40, 0),
            null,
            TimeSpan.FromHours(9),
        };

        private ParserCore parserCore;

        [SetUp]
        public void Setup()
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            parserCore = new ParserCore();
        }

        [Test, Order(1)]
        public void Preparation()
        {
            Assert.DoesNotThrow(SetupParser);

            Assert.AreEqual(DateTime.Parse(FirstScheduleDay), parserCore.FirstDay);
            Assert.AreEqual(DateTime.Parse(LastScheduleDay), parserCore.LastDay);
        }

        [Test]
        public void NewSchedule()
        {
            SetupParser();

            File.Copy(FirstScheduleFile, $"_{FirstScheduleFile}");

            Assert.DoesNotThrow(() => parserCore.SetUp(SecondScheduleFile));

            File.Move($"_{FirstScheduleFile}", FirstScheduleFile);
        }

        [Test]
        public void TwoWeeks()
        {
            SetupParser();

            for (int i = 0; i < TestingEndDay - TestingStartDay; i++)
            {
                var testingDate    = DateTime.Parse($"{TestingStartDay + i}.{TestingMonth}.{TestingYear}");
                var parserResponse = parserCore.GetScheduleForDay(testingDate);

                var validDuration = _testingDurations[i];

                string errorMessage = $"Duration and valid duration aren't equal [Week: {i % 7} | Day: {testingDate.DayOfWeek}]";

                Assert.AreEqual(validDuration, parserResponse.Duration, errorMessage);
                Assert.IsNull(parserResponse.AdditionalDuration, errorMessage);
            }
        }

        [Test]
        public void OutOfRangeDays()
        {
            SetupParser();

            var parserResponse = parserCore.GetScheduleForDay(DateTime.MinValue);

            Assert.IsNull(parserResponse.Duration);
            Assert.IsNull(parserResponse.AdditionalDuration);

            parserResponse = parserCore.GetScheduleForDay(DateTime.MaxValue);

            Assert.IsNull(parserResponse.Duration);
            Assert.IsNull(parserResponse.AdditionalDuration);
        }

        private void SetupParser() => parserCore.SetUp(FirstScheduleFile);
    }
}