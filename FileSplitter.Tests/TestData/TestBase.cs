namespace FileSplitter.Tests.TestData
{
    /// <summary>
    /// Base class for all tests that provides test directory management and cleanup.
    /// Ensures all test files and directories are properly cleaned up after each test.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected string TestDirectory { get; private set; }
        private readonly List<string> _createdFiles = new();
        private readonly List<string> _createdDirectories = new();

        protected TestBase()
        {
            // Create a unique test directory for each test
            TestDirectory = Path.Combine(Path.GetTempPath(), $"FileSplitterTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(TestDirectory);
            _createdDirectories.Add(TestDirectory);
        }

        /// <summary>
        /// Tracks a file for cleanup after the test completes.
        /// </summary>
        protected void TrackFile(string filePath)
        {
            if (!_createdFiles.Contains(filePath))
            {
                _createdFiles.Add(filePath);
            }
        }

        /// <summary>
        /// Tracks a directory for cleanup after the test completes.
        /// </summary>
        protected void TrackDirectory(string directoryPath)
        {
            if (!_createdDirectories.Contains(directoryPath))
            {
                _createdDirectories.Add(directoryPath);
            }
        }

        /// <summary>
        /// Gets the full path for a test file in the test directory.
        /// </summary>
        protected string GetTestFilePath(string fileName)
        {
            var filePath = Path.Combine(TestDirectory, fileName);
            TrackFile(filePath);
            return filePath;
        }

        /// <summary>
        /// Cleans up all tracked files and directories created during the test.
        /// </summary>
        public void Dispose()
        {
            // Clean up individual files
            foreach (var file in _createdFiles)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            // Clean up directories (in reverse order to handle nested directories)
            foreach (var directory in _createdDirectories.OrderByDescending(d => d.Length))
            {
                try
                {
                    if (Directory.Exists(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
