namespace OnlineBeratungstermin.Helpers
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logsDirectory;

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_logsDirectory);
        }

        public void Dispose()
        {
            // No cleanup needed
        }

        public FileLoggerProvider(string logsDirectory)
        {
            try
            {
                _logsDirectory = logsDirectory;
                EnsureDirectoryExists(_logsDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory: {ex}");
            }
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create log directory: {ex}");
                }
            }
        }
    }
}
