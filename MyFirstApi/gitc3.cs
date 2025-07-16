using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class LogFileProcessor
{
    private readonly string _inputFilePath;
    private readonly string _outputFilePath;
    private const string ErrorPattern = @"ERROR|EXCEPTION|FAILURE"; // Regex pattern for errors

    public LogFileProcessor(string inputFilePath, string outputFilePath)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath))
            throw new ArgumentException("Input file path cannot be null or empty.", nameof(inputFilePath));
        if (string.IsNullOrWhiteSpace(outputFilePath))
            throw new ArgumentException("Output file path cannot be null or empty.", nameof(outputFilePath));

        _inputFilePath = inputFilePath;
        _outputFilePath = outputFilePath;
    }

    /// <summary>
    /// Processes the log file asynchronously, counts errors, and writes a summary.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task ProcessLogFileAsync()
    {
        Console.WriteLine($"Starting log file processing for: {_inputFilePath}");
        var errorCounts = new Dictionary<string, int>();
        long totalLinesProcessed = 0;

        try
        {
            if (!File.Exists(_inputFilePath))
            {
                Console.WriteLine($"Error: Input file not found at '{_inputFilePath}'");
                return;
            }

            // Use StreamReader for efficient line-by-line reading
            using (var reader = new StreamReader(_inputFilePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    totalLinesProcessed++;
                    if (IsErrorLine(line))
                    {
                        // Simple categorization for demonstration
                        string errorType = ExtractErrorType(line);
                        if (!string.IsNullOrWhiteSpace(errorType))
                        {
                            errorCounts[errorType] = errorCounts.GetValueOrDefault(errorType) + 1;
                        }
                        else
                        {
                            errorCounts["UNKNOWN_ERROR"] = errorCounts.GetValueOrDefault("UNKNOWN_ERROR") + 1;
                        }
                    }

                    // Simulate some work being done per line if needed
                    // await Task.Delay(1); // Small delay for demonstration of async nature
                }
            }

            Console.WriteLine($"Finished reading log file. Total lines processed: {totalLinesProcessed}");
            await WriteProcessingSummaryAsync(errorCounts, totalLinesProcessed);
            Console.WriteLine($"Processing complete. Results written to: {_outputFilePath}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An I/O error occurred during file processing: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Access denied to file: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private bool IsErrorLine(string line)
    {
        return Regex.IsMatch(line, ErrorPattern, RegexOptions.IgnoreCase);
    }

    private string ExtractErrorType(string line)
    {
        // A more sophisticated regex could extract specific error codes or messages.
        // For simplicity, we'll just try to find the first matching keyword.
        var match = Regex.Match(line, ErrorPattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Value.ToUpper();
        }
        return "Generic Error"; // Default if no specific pattern found
    }

    /// <summary>
    /// Writes the processing summary to the output file asynchronously.
    /// </summary>
    private async Task WriteProcessingSummaryAsync(Dictionary<string, int> errorCounts, long totalLines)
    {
        try
        {
            using (var writer = new StreamWriter(_outputFilePath, append: false)) // Overwrite existing file
            {
                await writer.WriteLineAsync("--- Log File Processing Summary ---");
                await writer.WriteLineAsync($"Input File: {_inputFilePath}");
                await writer.WriteLineAsync($"Timestamp: {DateTime.Now}");
                await writer.WriteLineAsync($"Total Lines Processed: {totalLines}");
                await writer.WriteLineAsync("\n--- Error Breakdown ---");

                if (errorCounts.Any())
                {
                    foreach (var entry in errorCounts.OrderByDescending(e => e.Value))
                    {
                        await writer.WriteLineAsync($"- {entry.Key}: {entry.Value} occurrences");
                    }
                }
                else
                {
                    await writer.WriteLineAsync("No errors found.");
                }

                await writer.WriteLineAsync("\n--- End of Summary ---");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing summary to output file: {ex.Message}");
        }
    }

    // Example usage:
    public static async Task Main(string[] args)
    {
        // Create a dummy log file for testing
        string dummyLogFilePath = "sample_log.txt";
        string outputReportPath = "log_report.txt";

        if (File.Exists(dummyLogFilePath)) File.Delete(dummyLogFilePath);
        if (File.Exists(outputReportPath)) File.Delete(outputReportPath);

        await CreateDummyLogFile(dummyLogFilePath);

        var processor = new LogFileProcessor(dummyLogFilePath, outputReportPath);
        await processor.ProcessLogFileAsync();

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

    private static async Task CreateDummyLogFile(string path)
    {
        Console.WriteLine($"Creating dummy log file: {path}");
        using (var writer = new StreamWriter(path))
        {
            await writer.WriteLineAsync("INFO: Application started successfully.");
            await writer.WriteLineAsync("DEBUG: User 'admin' logged in.");
            await writer.WriteLineAsync("WARNING: Low disk space detected on drive C.");
            await writer.WriteLineAsync("ERROR: Database connection failed. (Connection Timeout)");
            await writer.WriteLineAsync("INFO: Processing user data for ID 123.");
            await writer.WriteLineAsync("DEBUG: Cache updated.");
            await writer.WriteLineAsync("ERROR: NullReferenceException at Line 45 in DataProcessor.cs.");
            await writer.WriteLineAsync("INFO: Report generation initiated.");
            await writer.WriteLineAsync("WARNING: API call responded with status 404.");
            await writer.WriteLineAsync("ERROR: Critical system FAILURE detected. (Service unavailable)");
            await writer.WriteLineAsync("DEBUG: Background task completed.");
            await writer.WriteLineAsync("INFO: User 'guest' logged out.");
            await writer.WriteLineAsync("ERROR: Invalid input parameter. (Validation Error)");
            await writer.WriteLineAsync("EXCEPTION: An unhandled exception occurred in thread X.");
            await writer.WriteLineAsync("INFO: Application shutting down.");
            for (int i = 0; i < 1000; i++) // Add more lines for a "bigger" file
            {
                if (i % 50 == 0) await writer.WriteLineAsync($"ERROR: Simulated error {i / 50}.");
                else await writer.WriteLineAsync($"INFO: Regular log entry {i}.");
            }
        }
        Console.WriteLine("Dummy log file created.");
    }
}