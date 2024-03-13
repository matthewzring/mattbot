namespace mattbot.utils;

public class LogUtil
{
    private static readonly string LOG_TIME = "`[{0}]`";
    private static readonly string EMOJI = " {1}";

    private static readonly string LOG_FORMAT = LOG_TIME + EMOJI + " {2}";

    public static string LogFormat(DateTimeOffset time, string emoji, string content)
    {
        return String.Format(LOG_FORMAT, TimeF(time), emoji, content);
    }

    private static string TimeF(DateTimeOffset time)
    {
        return time.ToLocalTime().ToString("HH:mm:ss");
    }
}
