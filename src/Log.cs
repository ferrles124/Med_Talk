using StardewModdingAPI;

namespace MedTalk
{
    public static class Log
    {
        private static IMonitor _monitor;

        public static void Initialize(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public static void Debug(string message) => _monitor?.Log(message, LogLevel.Debug);
        public static void Info(string message) => _monitor?.Log(message, LogLevel.Info);
        public static void Warn(string message) => _monitor?.Log(message, LogLevel.Warn);
        public static void Error(string message) => _monitor?.Log(message, LogLevel.Error);
    }
}
