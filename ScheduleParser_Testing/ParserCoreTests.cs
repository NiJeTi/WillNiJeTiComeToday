using NUnit.Framework;

using ScheduleParser;

using System;
using System.Globalization;

namespace ScheduleParser_Testing
{
    [TestFixture]
    internal sealed class ParserCoreTests
    {
        private const string FirstScheduleFile  = "SampleFiles\\Schedule1.xlsx";
        private const string SecondScheduleFile = "SampleFiles\\Schedule2.xlsx";

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

        [Test, Order(3)]
        public void NewSchedule()
        {
            SetupParser();

            Assert.DoesNotThrow(() => parserCore.SetUp(SecondScheduleFile));
        }

        private void SetupParser() => parserCore.SetUp(FirstScheduleFile);

        [Test, Order(2)]
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
    }
}