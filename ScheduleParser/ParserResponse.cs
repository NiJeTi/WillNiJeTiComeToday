using System;

namespace ScheduleParser
{
    [Serializable]
    public sealed class ParserResponse
    {
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime   { get; set; }

        public TimeSpan? AdditionalBeginTime { get; set; }
        public TimeSpan? AdditionalEndTime   { get; set; }

        public TimeSpan? Duration
        {
            get
            {
                if (BeginTime.HasValue && EndTime.HasValue)
                    return EndTime.Value - BeginTime.Value;

                BeginTime = null;
                EndTime   = null;

                return null;
            }
        }

        public TimeSpan? AdditionalDuration
        {
            get
            {
                if (AdditionalBeginTime.HasValue && AdditionalEndTime.HasValue)
                    return AdditionalEndTime.Value - AdditionalBeginTime.Value;

                AdditionalBeginTime = null;
                AdditionalEndTime   = null;

                return null;
            }
        }
    }
}