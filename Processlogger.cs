using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class Processlogger
{
    static void Main()
    {
        // Create a log folder (if it doesn't exist)
        string logFolder = "Logs";
        Directory.CreateDirectory(logFolder);

        // Get the current date in the format "yyyyMMdd"
        string currentDate = DateTime.Now.ToString("yyyy_MM_dd");
        string logFileName = $"process_log_{currentDate}.txt";
        string logFilePath = Path.Combine(logFolder, logFileName);

        Console.WriteLine("Process logger started. Press Ctrl+C to stop.");

        // Register a Ctrl+C handler to stop the logger gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent the process from exiting immediately
            Console.WriteLine("Process logger stopped.");
        };

        while (true)
        {
            try
            {
                LogProcesses(logFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(1000); // Adjust the interval as needed
        }
    }

    static void LogProcesses(string logFilePath)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    TimeSpan elapsedTime = DateTime.Now - process.StartTime;
                    string processInfo = $"Process ID: {process.Id}\n" +
                                        $"Name: {process.ProcessName}\n" +
                                        $"Path: {GetProcessPath(process)}\n" +
                                        $"Start Time: {process.StartTime}\n" +
                                        $"Elapsed Time: {elapsedTime}\n" +
                                        $"CPU Usage: {process.TotalProcessorTime}\n" +
                                        $"Memory Usage: {process.PrivateMemorySize64} bytes\n";

                    if (Is64BitProcess(process))
                    {
                        string logEntry = $"64-bit Process: {process.ProcessName}\n{processInfo}";
                        Console.WriteLine(logEntry);
                        writer.WriteLine(logEntry);
                    }
                    else
                    {
                        string logEntry = $"32-bit (x86) Process: {process.ProcessName}\n{processInfo}";
                        Console.WriteLine(logEntry);
                        writer.WriteLine(logEntry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }

    static bool Is64BitProcess(Process process)
    {
        if (Environment.Is64BitOperatingSystem)
        {
            try
            {
                string fileName = process.MainModule?.FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName.ToLower().Contains("system32");
                }
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    static string GetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName ?? "Path not available";
        }
        catch
        {
            return "Path not available";
        }
    }
}
