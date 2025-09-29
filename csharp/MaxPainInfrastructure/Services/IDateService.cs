namespace MaxPainInfrastructure.Services
{
    public interface IDateService
    {
        public DateTime YMDToDate(int value);

        public int DateToYMD(DateTime d);

        public DateTime NextFriday();

        public DateTime NextFriday(DateTime dt);

        public bool IsThirdFriday(DateTime dt);

        public DateTime GMTToEST(DateTime utc);

        public DateTime CurrentDateEST();

        public DateTime ESTToGMT(DateTime est);

        public DateTime ToSmallDateTime(DateTime dt);

        public DateTime UnixTimestampToDateTime(double unixTime);

        public double DateTimeToUnixTimestamp(string dateTime);

        public double DateTimeToUnixTimestamp(DateTime dateTime);

    }
}
