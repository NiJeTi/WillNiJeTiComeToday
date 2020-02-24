using NUnit.Framework;

using ScheduleParser;

using System;

namespace ScheduleParser_Testing
{
    [TestFixture]
    internal sealed class ParserResponseTests
    {
        private const int DefaultBegin = 10;
        private const int DefaultEnd   = 18;

        private const int FullTime = 8;

        private ParserResponse parserResponse;

        private TimeSpan begin;
        private TimeSpan end;
        private TimeSpan duration;

        [SetUp]
        public void Setup()
        {
            parserResponse = new ParserResponse();

            begin    = TimeSpan.FromHours(DefaultBegin);
            end      = TimeSpan.FromHours(DefaultEnd);
            duration = TimeSpan.FromHours(FullTime);
        }

        [Test]
        public void Duration()
        {
            parserResponse.BeginTime = begin;
            parserResponse.EndTime   = end;

            Assert.AreEqual(parserResponse.Duration, duration);
        }

        [Test]
        public void AdditionalDuration()
        {
            parserResponse.AdditionalBeginTime = begin;
            parserResponse.AdditionalEndTime   = end;

            Assert.AreEqual(duration, parserResponse.AdditionalDuration);
        }

        [Test]
        public void TimeZeroing()
        {
            parserResponse.BeginTime = TimeSpan.Zero;

            var nullDuration = parserResponse.Duration;

            Assert.IsNull(nullDuration);
            Assert.IsNull(parserResponse.BeginTime);
        }

        [Test]
        public void AdditionalTimeZeroing()
        {
            parserResponse.AdditionalBeginTime = begin;

            var nullDuration = parserResponse.AdditionalDuration;

            Assert.IsNull(nullDuration);
            Assert.IsNull(parserResponse.AdditionalBeginTime);
        }
    }
}