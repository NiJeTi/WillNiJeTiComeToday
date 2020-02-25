using System;
using System.Data;
using System.IO;

using ExcelDataReader;

namespace ScheduleParser
{
    public sealed class ParserCore
    {
        private const int MondayOffset = 10;
        private const int RowsOffset   = 1;

        private const int DayWidth = 4;

        private const int ConstraintDaysColumnIndex = 3;
        private const int FirstDayRowIndex          = 33;
        private const int LastDayRowIndex           = 34;

        private string    scheduleFileName;
        private DataTable schedule;

        public DateTime FirstDay { get; private set; }
        public DateTime LastDay  { get; private set; }

        public void SetUp(string scheduleFileName)
        {
            if (!string.IsNullOrEmpty(this.scheduleFileName) && scheduleFileName != this.scheduleFileName)
                File.Delete(this.scheduleFileName);

            this.scheduleFileName = scheduleFileName;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream          = File.Open(scheduleFileName, FileMode.Open, FileAccess.Read);
            using var excelDataReader = ExcelReaderFactory.CreateReader(stream);

            schedule = excelDataReader.AsDataSet().Tables[0];

            FirstDay = (DateTime) schedule.Rows[FirstDayRowIndex][ConstraintDaysColumnIndex];
            LastDay  = (DateTime) schedule.Rows[LastDayRowIndex][ConstraintDaysColumnIndex];
        }

        public ParserResponse GetScheduleForDay(DateTime day)
        {
            var response = new ParserResponse();

            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday || day < FirstDay || day > LastDay)
                return response;

            (int row, int column) = GetDayCoordinates(ref day);

            response.BeginTime = ReadTime(ref row, ref column)?.TimeOfDay;
            response.EndTime   = ReadTime(ref row, ref column)?.TimeOfDay;

            response.AdditionalBeginTime = ReadTime(ref row, ref column)?.TimeOfDay;
            response.AdditionalEndTime   = ReadTime(ref row, ref column)?.TimeOfDay;

            return response;
        }

        private DateTime? ReadTime(ref int row, ref int column)
        {
            if (schedule.Rows[row][column] is DBNull)
                return null;

            return (DateTime) schedule.Rows[row][column++];
        }

        private (int row, int column) GetDayCoordinates(ref DateTime day)
        {
            int daysWeekRow = GetDayWeekNumber(ref day) + RowsOffset;
            int daysColumn  = MondayOffset + ((int) day.DayOfWeek - 1) * DayWidth;

            return (daysWeekRow, daysColumn);
        }

        private int GetDayWeekNumber(ref DateTime day) => (day - FirstDay).Days / 7;
    }
}