using System.ComponentModel;
using System.Threading;
using System.Windows;
using System;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Unified_Project_Selector
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();

        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string filePath = @"C:\Program Files\Siemens\Automation\WinCCUnified\bin\output.txt";
            GenOutput();
            Thread.Sleep(1000);

            while (true)
            {
                // Create a FileInfo object to get file information
                FileInfo fileInfo = new FileInfo(filePath);

                // Get the file size in bytes
                long fileSizeInBytes = fileInfo.Length;

                // Check if the file size is not 0
                if (fileSizeInBytes > 0)
                {
                    texteToCsv();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        textInfo.Text = "Converting file formats...";
                    });                    
                    break;
                }

                // Sleep for a while to avoid busy-waiting
                Thread.Sleep(1000);
            }

            // Continue with other background tasks
            Thread.Sleep(1000);

            Application.Current.Dispatcher.Invoke(() =>
            {
                textInfo.Text = "Loading resources...";
            });

            AddFalse();
            Thread.Sleep(1000);

            // Report progress or perform other background tasks
            (sender as BackgroundWorker).ReportProgress(100);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;

            if (progressBar.Value == 100)
            {
                MainWindow mywindow = new MainWindow();
                Close();
                mywindow.ShowDialog();
            }
        }
        public void GenOutput()
        {
            // Create a new process start info
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Create and start the process
            using (Process process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                // Send the first batch command to the process
                string batchCommand1 = "cd C:\\Program Files\\Siemens\\Automation\\WinCCUnified\\bin";
                process.StandardInput.WriteLine(batchCommand1);

                // Send the second batch command to the process
                string batchCommand2 = "SIMATICRuntimeManager.exe -s -c projectlist -o";
                process.StandardInput.WriteLine(batchCommand2);
            }
        }
        private static string GetValueFromLine(string line, string prefix)
        {
            int startIndex = line.IndexOf(prefix);
            if (startIndex >= 0)
            {
                startIndex += prefix.Length;
                return line.Substring(startIndex).Trim();
            }
            else
            {
                // Handle the case where the prefix is not found in the line
                return string.Empty;
            }
        }
        public void texteToCsv()
        {
            string inputFilePath = "C:\\Program Files\\Siemens\\Automation\\WinCCUnified\\bin\\output.txt";
            string outputFilePath = "C:\\Users\\Public\\Documents\\Unified Project Selector\\online\\output.csv";

            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                StringBuilder csvOutput = new StringBuilder();

                int entryNumber = 0;
                string projectName = "";
                string deviceName = "";
                string projectType = "";
                string projectID = "";
                string autostart = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // This line appears to indicate a new project entry, so we skip it.
                        continue;
                    }
                    else if (line.StartsWith(" Project name:"))
                    {
                        entryNumber++;
                        projectName = GetValueFromLine(line, "Project name:");
                    }
                    else if (line.StartsWith(" Device name:"))
                    {
                        deviceName = GetValueFromLine(line, "Device name:");
                    }
                    else if (line.StartsWith(" Project type:"))
                    {
                        projectType = GetValueFromLine(line, "Project type:");
                    }
                    else if (line.StartsWith(" Project ID:"))
                    {
                        projectID = GetValueFromLine(line, "Project ID:");
                    }
                    else if (line.StartsWith(" Autostart:"))
                    {
                        autostart = GetValueFromLine(line, "Autostart:");

                        // Once all fields are collected, add them to the CSV output
                        if (entryNumber > 0)
                        {
                            csvOutput.AppendLine($"{entryNumber},{projectName},{deviceName},{projectType},{projectID},{autostart}");
                        }
                    }
                }

                File.WriteAllText(outputFilePath, csvOutput.ToString());

                // For debug
                // System.Windows.MessageBox.Show("File processed successfully. Output saved as 'output.csv'", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // For debug
                // System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void AddFalse()
        {
            string filePath = "C:\\Users\\Public\\Documents\\Unified Project Selector\\online\\output.csv";

            try
            {
                // Read all lines from the selected CSV file.
                string[] lines = File.ReadAllLines(filePath);

                // Modify each line to add "false" at the beginning.
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = "false," + lines[i];
                }

                // Write the modified lines back to the same file.
                File.WriteAllLines(filePath, lines);

                // For debug
                // System.Windows.MessageBox.Show("Added 'false' to the beginning of each row in the CSV file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // For debug
                // System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Manage system folders
            string mainDirectoryPath = @"C:\Users\Public\Documents\Unified Project Selector\";
            string[] subdirectoriesToClear = { "online", "temp" };
            string[] subDirectoriesToCreate = { "online", "offline", "settings", "temp" };

            // Check if the main directory exists
            if (!Directory.Exists(mainDirectoryPath))
            {
                // Create the main directory if it doesn't exist
                Directory.CreateDirectory(mainDirectoryPath);

                // Create all subdirectories
                foreach (string subdirectory in subDirectoriesToCreate)
                {
                    string subdirectoryPath = System.IO.Path.Combine(mainDirectoryPath, subdirectory);
                    Directory.CreateDirectory(subdirectoryPath);
                }
            }
            else
            {
                foreach (string subdirectory in subDirectoriesToCreate)
                {
                    string subdirectoryPath = System.IO.Path.Combine(mainDirectoryPath, subdirectory);
                    if (!Directory.Exists(subdirectoryPath))
                    {
                        Directory.CreateDirectory(subdirectoryPath);
                    }
                    else
                    {
                        continue;
                    }
                }
                    // Delete the contents of specified subdirectories
                    foreach (string subdirectory in subdirectoriesToClear)
                {
                    string subdirectoryPath = System.IO.Path.Combine(mainDirectoryPath, subdirectory);
                    if (Directory.Exists(subdirectoryPath))
                    {
                        DirectoryInfo subDirectoryInfo = new DirectoryInfo(subdirectoryPath);
                        foreach (FileInfo file in subDirectoryInfo.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo subSubDirectory in subDirectoryInfo.GetDirectories())
                        {
                            subSubDirectory.Delete(true);
                        }
                    }
                }
            }

            string configFile = @"C:\Users\Public\Documents\Unified Project Selector\settings\ups.cfg";
            if (!File.Exists(configFile))
            {
                MainWindow.WriteSettingsInit();
            }
        }
    }
}