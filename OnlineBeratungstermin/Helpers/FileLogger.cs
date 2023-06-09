namespace OnlineBeratungstermin.Helpers
{
    public class FileLogger : ILogger
    {
        private readonly string _logsDirectory;

        public FileLogger(string logsDirectory)
        {
            _logsDirectory = logsDirectory;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null; // No scope support needed
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Enable logging for all log levels
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string logFilePath = Path.Combine(_logsDirectory, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

            try
            {
                string logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} [{logLevel}] {formatter(state, exception)}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log entry: {ex}");
            }
        }
    }
}
