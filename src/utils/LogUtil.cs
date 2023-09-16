namespace mattbot.utils
{
    public class LogUtil
    {
        private const string LOG_TIME = "`[{0}]`";
        private const string EMOJI = " {1}";

        private const string LOG_FORMAT = LOG_TIME + EMOJI + " {2}";

        public static string LogFormat(DateTimeOffset time, string emoji, string content)
        {
            return string.Format(LOG_FORMAT, TimeF(time), emoji, content);
        }

        private static string TimeF(DateTimeOffset time)
        {
            return time.ToLocalTime().ToString("HH:mm:ss");
        }
    }
}
