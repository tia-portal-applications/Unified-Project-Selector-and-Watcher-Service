using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using CsvHelper;
using CsvHelper.Configuration;
using System.Threading;
using System.Windows.Threading;

namespace Unified_Project_Selector
{
    public partial class MainWindow : System.Windows.Window
    {
        public ObservableCollection<ProjectData> Projects { get; set; }
        private XmlHelper xmlHelper;
        public MainWindow()
        {
            InitializeComponent();
            Projects = new ObservableCollection<ProjectData> { };
            dataGrid.ItemsSource = Projects;

            // Initialize the XmlHelper with the path to your XML file
            xmlHelper = new XmlHelper(@"C:\Users\Public\Documents\Unified Project Selector\temp\test.xml");
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

         public void TxtToCsv()
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
                    System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void OpenCsvToDataGrid()
        {
                string filePath = "C:\\Users\\Public\\Documents\\Unified Project Selector\\online\\output.csv";

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, // Indicate that the file doesn't have headers.
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<ProjectData>().ToList();
                    Projects.Clear(); // Clear the existing data
                    foreach (var record in records)
                    {
                        Projects.Add(record);
                    }
                }
        }

        private async Task<bool> CheckFileContentAsync(string filePath)
        {
            // This is the old stuff of the "Watcher Service" that is now a project on it's own.

            if (!File.Exists(filePath))
            {
                return false; // File doesn't exist
            }
            // Create a FileSystemWatcher to monitor changes to the file
            using (var watcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(filePath)))
            {
                watcher.Filter = System.IO.Path.GetFileName(filePath);
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                var tcs = new TaskCompletionSource<bool>();

                // Event handler for when the file is changed
                FileSystemEventHandler onChanged = (sender, e) =>
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        tcs.SetResult(true); // Signal that content has been added
                    }
                };

                watcher.Changed += onChanged;
                watcher.EnableRaisingEvents = true;

                var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5)); // Adjust the timeout duration as needed

                // Wait for either content to be added or a timeout
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                // Remove the event handler
                watcher.Changed -= onChanged;

                if (completedTask == tcs.Task)
                {
                    return true; // Content was added
                }
                else
                {
                    // Handle the case where the timeout occurred
                    return false;
                }
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenCsvToDataGrid();    
        }

        private void GenOutput_Click(object sender, RoutedEventArgs e)
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
        private void readProjects_Click(object sender, RoutedEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Select the input text file";

            if (openFileDialog.ShowDialog() == true)
            {
                string inputFilePath = openFileDialog.FileName;
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

                    System.Windows.MessageBox.Show("File processed successfully. Output saved as 'output.csv'", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }
            
        private void TransferSelected_Click(object sender, RoutedEventArgs e)
        {
            // Transfer selected items to the ListBox
            foreach (ProjectData project in Projects)
            {
                if (project.IsSelected)
                {
                    // Check if an item is selected
                    if (optionsListBox.SelectedItem != null)
                    {
                        if (project.ProjectType == "Simulation")
                        {
                            // Get the selected ListBoxItem content
                            string selectedItemContent = ((ListBoxItem)optionsListBox.SelectedItem).Content.ToString();
                            
                            // For debug
                            // System.Windows.MessageBox.Show($"Selected option: {selectedItemContent}");

                            listBox.Items.Add("-sim -c " + selectedItemContent + " " + project.ProjectID); // Customize what you want to add to the ListBox
                        }
                        else
                        {
                            // Get the selected ListBoxItem content
                            string selectedItemContent = ((ListBoxItem)optionsListBox.SelectedItem).Content.ToString();

                            // For debug
                            //System.Windows.MessageBox.Show($"Selected option: {selectedItemContent}");

                            listBox.Items.Add("-c " + selectedItemContent + " " + project.ProjectID); // Customize what you want to add to the ListBox
                        }


                    }
                }
            }
            UnselectAll_Click(null, null);
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection changes here if needed
        }

        private void OpenCsvFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, // Indicate that the file doesn't have headers.
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<ProjectData>().ToList();
                    Projects.Clear(); // Clear the existing data
                    foreach (var record in records)
                    {
                        Projects.Add(record);
                    }
                }
            }
        }

        public void AddFalseButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a file dialog to select the CSV file.
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Select a CSV File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

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

                    System.Windows.MessageBox.Show("Added 'false' to the beginning of each row in the CSV file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TransferAll_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear();
            foreach (ProjectData project in Projects)
            {
                if (project.ProjectType == "Simulation")
                {
                    // Get the selected ListBoxItem content
                    string selectedItemContent = ((ListBoxItem)optionsListBox.SelectedItem).Content.ToString();

                    // For debug
                    // System.Windows.MessageBox.Show($"Selected option: {selectedItemContent}");

                    listBox.Items.Add("-sim -c " + selectedItemContent + " " + project.ProjectID); 
                }
                else
                {
                    // Get the selected ListBoxItem content
                    string selectedItemContent = ((ListBoxItem)optionsListBox.SelectedItem).Content.ToString();

                    // For debug
                    // System.Windows.MessageBox.Show($"Selected option: {selectedItemContent}");

                    listBox.Items.Add("-c " + selectedItemContent + " " + project.ProjectID); 
                }
            }
            UnselectAll_Click(null, null);
        }
        private void RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.Clear(); // Clear all items from the ListBox
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_OnOff.SelectedItem != null)
            {
                if (ComboBox_OnOff.SelectedItem is ComboBoxItem selectedItem)
                {
                    // If the selected item is a ComboBoxItem, extract its content
                    string selectedContent = selectedItem.Content?.ToString();
                    if (!string.IsNullOrEmpty(selectedContent))
                    {
                        // For debug
                        // System.Windows.MessageBox.Show($"Selected item1: {selectedContent}");
                        if (!string.IsNullOrEmpty(selectedContent))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (selectedContent == "Offline") // Selection is "Offline"
                                {
                                    foreach (var item in lb_OfflineConfig.Items)
                                    {
                                        ListBoxItem listBoxItem = lb_OfflineConfig.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                                        if (listBoxItem != null)
                                        {
                                            listBoxItem.IsSelected = true;
                                        }
                                    }
                                }
                                else // Selection is "Online"
                                {
                                    foreach (ProjectData project in Projects)
                                    {
                                        project.IsSelected = true;
                                    }
                                }
                            });
                        }
                    }
                }
                else if (ComboBox_OnOff.SelectedItem is string selectedValue)
                {
                    // If the selected item is already a string, use it directly
                }
            }
        }

        private void UnselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBox_OnOff.SelectedItem != null)
            {
                if (ComboBox_OnOff.SelectedItem is ComboBoxItem selectedItem)
                {
                    // If the selected item is a ComboBoxItem, extract its content
                    string selectedContent = selectedItem.Content?.ToString();
                    if (!string.IsNullOrEmpty(selectedContent))
                    {
                        //System.Windows.MessageBox.Show($"Selected item1: {selectedContent}");
                        if (!string.IsNullOrEmpty(selectedContent))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (selectedContent == "Offline") // Selection is "Offline"
                                {
                                    foreach (var item in lb_OfflineConfig.Items)
                                    {
                                        ListBoxItem listBoxItem = lb_OfflineConfig.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                                        if (listBoxItem != null)
                                        {
                                            listBoxItem.IsSelected = false;
                                        }
                                    }
                                }
                                else // Selection is "Online"
                                {
                                    foreach (ProjectData project in Projects)
                                    {
                                        project.IsSelected = false;
                                    }
                                }
                            });
                        }
                    }
                }
                else if (ComboBox_OnOff.SelectedItem is string selectedValue)
                {
                    // If the selected item is already a string, use it directly
                }
            }
        }

        private async void ReloadData_Click(object sender, RoutedEventArgs e)
        {
            // Hide the ReloadRect before starting the background tasks
            Dispatcher.Invoke(() =>
            {
                rld_Data.Visibility = Visibility.Hidden;
                ReloadRect.Visibility = Visibility.Hidden;
            });
            Projects.Clear();
            string filePath = @"C:\Program Files\Siemens\Automation\WinCCUnified\bin\output.txt";

            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    ReloadRect.Visibility = Visibility.Visible;
                    rld_Data.Visibility = Visibility.Visible;
                });

                GenOutput();
                Populate_Offline_LB();
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
                        // Continue with your action after the file size is greater than or equal to 1 KB
                        TxtToCsv();
                        break; 
                    }

                    // Sleep for a while to avoid busy-waiting
                    Thread.Sleep(1000);
                }

                // Continue with other background tasks
                Thread.Sleep(1000);

                // Show the ReloadRect and update the UI
                Dispatcher.Invoke(() =>
                {
                    ReloadRect.Visibility = Visibility.Hidden;
                    rld_Data.Visibility = Visibility.Hidden;
                    AddFalse();
                    OpenCsvToDataGrid();
                });
            });
        }

        private void StartSelection(object sender, RoutedEventArgs e)
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
                string batchCommand2 = "SIMATICRuntimeManager.exe -s -quiet -sim -c start " + listBox.Items[0].ToString();
                process.StandardInput.WriteLine(batchCommand2);
            }
            listBox.Items.Remove(listBox.Items[0]);
        }
        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            List<string> itemsToRemove = new List<string>();

            for (int i = listBox.SelectedItems.Count - 1; i >= 0; i--)
            {
                string item = listBox.SelectedItems[i] as string;
                if (item != null)
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (string item in itemsToRemove)
            {
                listBox.Items.Remove(item);
            }
        }

        private void ExecuteQueue_Click(object sender, RoutedEventArgs e)
        {
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
                try
                {
                    // Specify the path where you want to create the .bat file
                    string batFilePath = @"C:\Users\Public\Documents\Unified Project Selector\temp\execqueue.bat";

                    if (!System.IO.File.Exists(batFilePath))
                    {



                        // Specify the entire content of the .bat file as a plain text string
                        string batFileContent = @"
@echo off
setlocal enabledelayedexpansion
cd ""C:\Users\Public\Documents\Unified Project Selector\temp""
for /f ""delims="" %%a in (queue.txt) do (
    set ""data=%%a""
    echo Executing !data!
    echo.
    ""C:\Program Files\Siemens\Automation\WinCCUnified\bin\SIMATICRuntimeManager.exe"" -s !data!
    timeout 3
)";
                        // Write the content to the .bat file
                        File.WriteAllText(batFilePath, batFileContent);
                    }

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error: " + ex.Message);
                }

                // Specify the file path where you want to save the text file
                string filePath = @"C:\Users\Public\Documents\Unified Project Selector\temp\queue.txt";

                try
                {
                    // Empty the file by overwriting it with an empty string
                    File.WriteAllText(filePath, string.Empty);

                    // Create or overwrite the text file
                    using (StreamWriter writer = File.CreateText(filePath))
                    {
                        // Loop through the ListBox items and write them to the file
                        foreach (var item in listBox.Items)
                        {
                            writer.WriteLine(item.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Create and start the process
                using (Process process2 = new Process { StartInfo = processStartInfo })
                {
                    process2.Start();

                    // Send the first batch command to the process
                    string batchCommand1 = "cd C:\\Users\\Public\\Documents\\Unified Project Selector\\temp";
                    process2.StandardInput.WriteLine(batchCommand1);

                    // Send the second batch command to the process
                    string batchCommand2 = "execqueue.bat";
                    process2.StandardInput.WriteLine(batchCommand2);
                }
            }
            listBox.Items.Clear();
        }
        private void create_offline_pckg_click(object sender, RoutedEventArgs e)
        {
            foreach (ProjectData project in Projects)
            {
                if (project.IsSelected)
                {
                    // Check if an item is selected
                    if (optionsListBox.SelectedItem != null)
                    {
                        if (project.ProjectType == "Simulation")
                        {
                            System.Windows.MessageBox.Show("Selection is simulation, not supported!");
                        }
                        else
                        {
                            string sourceDir = @"C:\ProgramData\SCADAProjects\" + project.DeviceName;
                            string checkDir = sourceDir + @"\currentConfiguration\system\ProjectCharacteristics.rdf";

                            if (OfflineHelper.CheckName(checkDir, project.ProjectID))
                            {
                                DateTime dateTime = DateTime.Now;
                                string formattedDateTime = dateTime.ToString("dd-MM-yyyy_HH-mm-ss");
                                string zipName = "[" + project.ProjectName + " - " + project.DeviceName + "] - " + formattedDateTime + ".zip";
                                OfflineHelper.OfflinePackageCreator(sourceDir, zipName, project);
                                System.Windows.MessageBox.Show("Created offline package:\n \n" + zipName);
                            }
                            else
                            {
                                // This means you might have more than one folder in SCADAProjects with the name of your project, like HMI_RT_1-1, *-2, *-3, etc. So the tool canno't resolve which one to use (currently).
                                System.Windows.MessageBox.Show("Seems that there is no match of the ID with this project name:\n" + project.DeviceName + "\n\nIf you have a folder with the name " + project.DeviceName + " with an increment, change the folder name so it matches the Device Name accordingly.");
                            }
                        }
                    }
                }
            }
            UnselectAll_Click(null, null);
        }




        private void Populate_Offline_LB()
        {
            // Specify the folder path where the .zip files are located
            string folderPath = @"C:\Users\Public\Documents\Unified Project Selector\offline";

            // Get a list of .zip files in the folder
            string[] zipFiles = Directory.GetFiles(folderPath, "*.zip");

            // Create a list to store file names along with their last modified date
            List<Tuple<string, DateTime>> fileData = new List<Tuple<string, DateTime>>();

            foreach (string zipFile in zipFiles)
            {
                DateTime lastModified = File.GetLastWriteTime(zipFile);
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(zipFile);
                fileData.Add(Tuple.Create(fileNameWithoutExtension, lastModified));
            }

            // Sort the list by last modified date in descending order (newest first)
            fileData.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            // Use the Dispatcher to update the ListBox on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Clear the ListBox
                lb_OfflineConfig.Items.Clear();

                // Add sorted items to the ListBox
                foreach (var tuple in fileData)
                {
                    lb_OfflineConfig.Items.Add(tuple.Item1);
                }
            });
        }

        private void btn_TransferConfig_Click(object sender, RoutedEventArgs e)
        {
            foreach (string item in lb_OfflineConfig.SelectedItems)
            {
                // Add the string directly to the ListBox without creating ListBoxItem objects
                listBox.Items.Add("-c fulldownload " + "\"C:\\Users\\Public\\Documents\\Unified Project Selector\\offline\\" + item + ".zip\"");
            }
            UnselectAll_Click(null, null);
        }

        private void btn_Exit(object sender, RoutedEventArgs e)
        {
            // Exit the app
            System.Windows.Application.Current.Shutdown();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag the Main Window
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void dblClick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Open working directory
            if (e.ClickCount == 2)
            {
                Process.Start("explorer.exe", @"C:\Users\Public\Documents\Unified Project Selector");
            }
        }

        private void btn_MinWindow(object sender, RoutedEventArgs e)
        {
            // Minimize the app
            WindowState = WindowState.Minimized;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_OnOff.SelectedItem != null)
            {
                if (ComboBox_OnOff.SelectedItem is ComboBoxItem selectedItem)
                {
                    // If the selected item is a ComboBoxItem, extract its content
                    string selectedContent = selectedItem.Content?.ToString();
                    if (!string.IsNullOrEmpty(selectedContent))
                    {
                        // For debug
                        // System.Windows.MessageBox.Show($"Selected item1: {selectedContent}");
                        if (!string.IsNullOrEmpty(selectedContent))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (selectedContent == "Offline")
                                {
                                    lb_OfflineConfig.Visibility = Visibility.Visible;
                                    Populate_Offline_LB();
                                    btn_TransferConfig.IsEnabled = !btn_TransferConfig.IsEnabled;
                                    btn_DeleteOffPckg.IsEnabled = !btn_DeleteOffPckg.IsEnabled;
                                    btn_CrtOffPckg.IsEnabled = !btn_CrtOffPckg.IsEnabled;
                                    btn_xfSel.IsEnabled = !btn_xfSel.IsEnabled;
                                    btn_xfAll.IsEnabled = !btn_xfAll.IsEnabled;
                                    optionsListBox.IsEnabled = !optionsListBox.IsEnabled;
                                    rct_Online.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x9A, 0x94, 0x94));
                                    lbl_Online.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x9A, 0x94, 0x94));
                                    rct_Offline.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                                    lbl_Offline.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));

                                }
                                else
                                {
                                    lb_OfflineConfig.Visibility = Visibility.Hidden;
                                    btn_TransferConfig.IsEnabled = !btn_TransferConfig.IsEnabled;
                                    btn_DeleteOffPckg.IsEnabled = !btn_DeleteOffPckg.IsEnabled;
                                    btn_CrtOffPckg.IsEnabled = !btn_CrtOffPckg.IsEnabled;
                                    btn_xfSel.IsEnabled = !btn_xfSel.IsEnabled;
                                    btn_xfAll.IsEnabled = !btn_xfAll.IsEnabled;
                                    optionsListBox.IsEnabled = !optionsListBox.IsEnabled;
                                    rct_Offline.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x9A, 0x94, 0x94));
                                    lbl_Offline.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x9A, 0x94, 0x94));
                                    rct_Online.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                                    lbl_Online.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                                }
                            });
                        }
                    }
                }
                else if (ComboBox_OnOff.SelectedItem is string selectedValue)
                {
                    // If the selected item is already a string, use it directly
                }
            }
        }

        private void btn_DeleteOffPckg_Click(object sender, RoutedEventArgs e)
        {
            // Get all selected items in the ListBox.
            var selectedItems = lb_OfflineConfig.SelectedItems;

            // Check if any items are selected.
            if (selectedItems.Count > 0)
            {
                // Specify the directory where your files are located.
                string directoryPath = @"C:\Users\Public\Documents\Unified Project Selector\offline\"; // Update this with your directory path.

                try
                {
                    foreach (var selectedItem in selectedItems)
                    {
                        string fileName = selectedItem.ToString() + ".zip";
                        string filePath = System.IO.Path.Combine(directoryPath, fileName);

                        // Check if the file exists before deleting.
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            System.Windows.MessageBox.Show($"File '{fileName}' has been deleted.");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show($"File '{fileName}' does not exist.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select one or more items from the list to delete.");
            }
            Populate_Offline_LB();
            UnselectAll_Click(null, null);
        }

        private void WatcherService_Click(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string programPath = System.IO.Path.Combine(currentDirectory, "Watcher Service", "Unified Project Selector - Watcher Service.exe");

            if (File.Exists(programPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = programPath,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(programPath), // Set the working directory
                };

                System.Diagnostics.Process.Start(startInfo);
            }
        }
        public static string ReadSettings(object sender, RoutedEventArgs e)
        {
            string configFile = @"C:\Users\Public\Documents\Unified Project Selector\settings\ups.cfg";
            
            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile);
                
                foreach (string line in lines)
                {
                    
                    if (line.StartsWith("Version="))
                    {
                        string[] parts = line.Split('='); // Split the string into parts using '=' as a delimiter
                        
                        if (parts.Length == 2)
                        {
                            string version = parts[1]; // Get the second part of the array
                            return version;
                        }
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("No config file detected! \nCreating default config file.");
                WriteSettingsInit();
            }
            return null;
        }
        public static void WriteSettingsInit()
        {
            string configFile = @"C:\Users\Public\Documents\Unified Project Selector\settings\ups.cfg";

            // This will need to be checked if used with other version than 18 update 2.
            string[] lines = new string[]
            {
        "[Default]",
        "Version=" + "18.0.0.2",
            };

            File.WriteAllLines(configFile, lines);
        }

        private void MenuSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Replace "path/to/your/file.txt" with the actual path to your file
                string filePath = "C:\\Users\\Public\\Documents\\Unified Project Selector\\settings\\ups.cfg";


                if (System.IO.File.Exists(filePath))
                {
                    // Use Process.Start to open the file with the default associated text editor
                    Process.Start("notepad.exe", filePath);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("File not found.", "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error: {ex.Message}", "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _About_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/tia-portal-applications/Unified-Project-Selector-and-Watcher-Service";
            try
            {
                // Use ProcessStartInfo to configure the process before starting
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // For debug
                // System.Windows.Forms.MessageBox.Show($"Error opening URL: {ex.Message}");
            }
        }
    }   
}