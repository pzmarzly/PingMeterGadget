namespace PingMeterGadget
{
    public static class ProgramConfig
    {
        // %appdata%/Ping Meter/pingmeter1.jsn
        public const string defaultFoldername = "Ping Meter";
        public const string defaultFileExtension = "cfg";

        // delay on Process.* actions
        public const short timeClone = 50;
        public const short timeExit = 250;

        // delay between unsuccessful pings
        /// <summary>Timeout after f.e. timeout</summary>
        public const short timeError = 2500;
        /// <summary>Timeout after exception - usually when DNS service is not avaible.</summary>
        public const short timeException = 2500;

        // Ping limits
        public const short limitOK = 80;
        public const short limitWarn = 150;
        public const short limitSlow = 150;
    }

    public class UserConfig
    {
        public bool firstConfig = true;
        public bool eachTimeConfig = true;
        public string favServer = "google.com";
        public bool escapeToExit = true;
        public bool showId = true;
        public bool rememberLocation = true;
        public bool alwaysOnTop = true;
        public int opacity = 80;
        public int timeNormal = 1000;
        public int timeHigh = 500;

        public double lastX = 50;
        public double lastY = 50;
    }
}
