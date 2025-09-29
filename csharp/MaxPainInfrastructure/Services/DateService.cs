namespace MaxPainInfrastructure.Services
{
    public class DateService : IDateService
    {
        public DateTime YMDToDate(int value)
        {
            string d = value.ToString();
            string formatted = $"{d.Substring(4, 2)}/{d.Substring(6, 2)}/{d.Substring(0, 4)}";
            return Convert.ToDateTime(formatted);
        }

        public int DateToYMD(DateTime d)
        {
            return Convert.ToInt32(d.ToString("yyyyMMdd"));
        }

        public DateTime NextFriday()
        {
            DateTime dt = System.DateTime.Now;
            return NextFriday(dt);
        }

        public DateTime NextFriday(DateTime dt)
        {
            int dow = (int)dt.DayOfWeek;
            int numDays = 5 - dow;
            if (numDays < 0) numDays += 7;
            DateTime fri = dt.AddDays(numDays);
            return Convert.ToDateTime(fri.ToString("MM/dd/yyyy"));
        }

        public bool IsThirdFriday(DateTime dt)
        {
            DateTime fom = Convert.ToDateTime(dt.ToString("MM/01/yyyy"));
            int dow = (int)fom.DayOfWeek;
            int firstFriday = 5 - dow;
            if (firstFriday < 0) firstFriday += 7;
            DateTime friday = fom.AddDays(firstFriday + 14);
            return (dt == friday);
        }

        public DateTime GMTToEST(DateTime utc)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, easternZone);
        }

        public DateTime CurrentDateEST()
        {
            return GMTToEST(DateTime.UtcNow);
        }

        public DateTime ESTToGMT(DateTime est)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime someDateTimeInUtc = TimeZoneInfo.ConvertTimeToUtc(est, tz);
            return someDateTimeInUtc;
        }

        public DateTime ToSmallDateTime(DateTime dt)
        {
            if (dt < System.Data.SqlTypes.SqlDateTime.MinValue.Value) return System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            if (dt > System.Data.SqlTypes.SqlDateTime.MaxValue.Value) return System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
            return dt;
        }

        public DateTime UnixTimestampToDateTime(double unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }

        public double DateTimeToUnixTimestamp(string dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            return DateTimeToUnixTimestamp(dt);
        }

        public double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        private DateTime LocalToEST(DateTime localTime)
        {
            /*
            DateTime zoneTime = DateTime.SpecifyKind(localTime, DateTimeKind.Local);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(zoneTime, easternZone);
            */

            DateTime dt = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(dt, easternZone);

            return easternTime;
        }
    }
}
