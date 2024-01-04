using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using Notification.Wpf;

namespace Unified_Project_Selector___Watcher_Service
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon notifyIcon;
        private FileSystemWatcher watcher;
        private string targetDirectory = @"C:\Users\Public\Documents\Unified Project Selector\watcher";
        private string settingDirectory = @"C:\Users\Public\Documents\Unified Project Selector\settings";
        private string tempDirectory = @"C:\Users\Public\Documents\Unified Project Selector\temp";
        private ToolStripMenuItem enabledMenuItem;
        private ToolStripMenuItem autoStartMenuItem;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create the target directory if it doesn't exist
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            
            if (!Directory.Exists(settingDirectory))
            {
                Directory.CreateDirectory(settingDirectory);
            }

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // Manage icon in system tray
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon("ups_icon.ico"); // Replace with your icon file
            notifyIcon.Visible = true; // Show the icon in the system tray

            notifyIcon.DoubleClick += (sender, e) =>
            {
                OpenInterface();
            };

            // Create a context menu
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // Add the "Open Interface" menu item
            ToolStripMenuItem openInterfaceMenuItem = new ToolStripMenuItem("Open Unified Project Selector");
            contextMenu.Items.Add(openInterfaceMenuItem);
            openInterfaceMenuItem.Click += (sender, e) => OpenInterface();

            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());

            // Add the "Enabled" menu item
            enabledMenuItem = new ToolStripMenuItem("Enabled");
            enabledMenuItem.CheckOnClick = true;
            enabledMenuItem.Checked = true;
            contextMenu.Items.Add(enabledMenuItem);

            // Add a click event handler to the "Enabled" menu item
            enabledMenuItem.Click += (sender, e) =>
            {
                // Update the settings
                WriteSettings();

                // Check if the "Enabled" menu item is checked
                if (enabledMenuItem.Checked)
                {
                    // The "Enabled" menu item was clicked and checked
                    autoStartMenuItem.Enabled = true;
                }
                else
                {
                    // The "Enabled" menu item was clicked and unchecked
                    autoStartMenuItem.Enabled = false;
                }
            };

            // Add the "Enabled" menu item
            autoStartMenuItem = new ToolStripMenuItem("Autostart");
            autoStartMenuItem.CheckOnClick = true;
            autoStartMenuItem.Checked = true;
            contextMenu.Items.Add(autoStartMenuItem);

            // Add a click event handler to the "Enabled" menu item
            autoStartMenuItem.Click += (sender, e) =>
            {
                WriteSettings();
            };

            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());

            // Add the "Watcher Directory" menu item
            ToolStripMenuItem watcherDirectoryMenuItem = new ToolStripMenuItem("Watcher Directory");
            contextMenu.Items.Add(watcherDirectoryMenuItem); // Insert after the separator

            // Add a click event handler to the menu item
            watcherDirectoryMenuItem.Click += (sender, e) =>
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    //folderBrowserDialog.Description = "Select the Watcher Directory";
                    folderBrowserDialog.SelectedPath = targetDirectory;
                    folderBrowserDialog.ShowNewFolderButton = true;

                    DialogResult result = folderBrowserDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        // User selected a folder
                        targetDirectory = folderBrowserDialog.SelectedPath;

                        // Update the settings
                        WriteSettings();

                        // Update the FileSystemWatcher with the new target directory
                        UpdateFileSystemWatcher(targetDirectory);
                    }
                }
            };

            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());

            // Add the "Exit" menu item
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            contextMenu.Items.Add(exitMenuItem);
            exitMenuItem.Click += (sender, eventArgs) =>
            {
                // Clean up the NotifyIcon and exit the application
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            };

            // Set the context menu to the notify icon
            notifyIcon.ContextMenuStrip = contextMenu;

            // Create and configure the FileSystemWatcher
            watcher = new FileSystemWatcher(targetDirectory);
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnFileCreated;
            ReadSettings();
        }
        
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(1000);
            var notificationManager = new NotificationManager();

            if (Directory.Exists(targetDirectory))
            {
                if (enabledMenuItem.Checked)
                {
                    string directoryPath = Path.GetDirectoryName(e.FullPath);
                    notificationManager.Show("New file detected", "\n Loading the file...");
                    int maxStableAttempts = 20; // Adjust as needed
                    int waitInterval = 1000; // Adjust as needed (milliseconds)
                    bool isFileStable = false;
                    
                    // Check for file size, waiting for finishing the copy (case of big project)
                    for (int i = 0; i < maxStableAttempts; i++)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                // If the FileStream can be opened, the file is not locked and can be considered stable
                                isFileStable = true;
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            // The file is still being written to; wait and try again
                            Thread.Sleep(waitInterval);
                        }
                    }

                    if (isFileStable)
                    {
                        // Continue with processing the file
                        string[] files = Directory.GetFiles(directoryPath);

                        foreach (string newFile in files)
                        {
                            if (autoStartMenuItem.Checked)
                            {
                                notificationManager.Show("New project detected", newFile + "\n\nAutostart enabled");
                                string project_id = FindProjectId(newFile);
                                Thread.Sleep(1000);
                                if (project_id != null)
                                {
                                    notificationManager.Show("Starting project id:", project_id);
                                    ExecCommands(newFile, true, project_id);
                                }
                            }
                            else
                            {
                                notificationManager.Show("New project detected", newFile + "\n\nAutostart disabled");
                                ExecCommands(newFile);
                            }
                        }
                    }
                    else
                    {
                        // Handle the case where the file is still not ready after waiting
                        notificationManager.Show("File not ready", $"The file {e.FullPath} is still being copied. Unable to process.");
                    }
                }
                else
                {
                    // The "Enabled" menu item is not checked, skip the action
                }
            }
            else
            {
                targetDirectory = @"C:\Users\Public\Documents\Unified Project Selector\watcher";
                WriteSettings();
            }
        }


        private void ReadSettings()
        {
            string configFile = @"C:\Users\Public\Documents\Unified Project Selector\settings\watcher.cfg";

            if (File.Exists(configFile))
            {
                string[] lines = File.ReadAllLines(configFile);

                foreach (string line in lines)
                {
                    if (line.StartsWith("Enabled="))
                    {
                        enabledMenuItem.Checked = bool.Parse(line.Substring(8));
                    }
                    else if (line.StartsWith("AutoStart="))
                    {
                        autoStartMenuItem.Checked = bool.Parse(line.Substring(10));
                    }
                }
            }
            else
            {
                WriteSettings();
            }
        }
        private void WriteSettings()
        {
            string configFile = @"C:\Users\Public\Documents\Unified Project Selector\settings\watcher.cfg";

            string[] lines = new string[]
            {
        "[Default]",
        "Enabled=" + enabledMenuItem.Checked,
        "AutoStart=" + autoStartMenuItem.Checked,
        "Target=" + targetDirectory
            };

            File.WriteAllLines(configFile, lines);
        }
        private void OpenInterface()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string programPath = Path.Combine(currentDirectory, "..", "Unified Project Selector.exe");

            if (File.Exists(programPath))
            {
                System.Diagnostics.Process.Start(programPath);
            }
            else
            {
                // Handle the case where the program doesn't exist
                var notificationManager = new NotificationManager();
                notificationManager.Show("No installation found at:", programPath);
            }
        }
        private void ExecCommands(string newFile, bool cmdStart = false, string projectID = "")
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
                // Specify the file path where you want to save the text file
                string filePath = @"C:\Users\Public\Documents\Unified Project Selector\temp\queuewatcher.txt";

                try
                {
                    // Empty the file by overwriting it with an empty string
                    File.WriteAllText(filePath, string.Empty);

                    if (cmdStart)
                    {
                        using (StreamWriter writer = File.CreateText(filePath))
                        {
                            writer.WriteLine("-quiet -c fulldownload \"" + newFile + "\"");
                            writer.WriteLine("-quiet -c start \"" + projectID + "\"");
                        }
                    }
                    else
                    {
                        
                        using (StreamWriter writer = File.CreateText(filePath))
                        {
                            writer.WriteLine("-quiet -c fulldownload \"" + newFile + "\"");
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

                    // Send the second batch command to the process
                    string batchCommand = "execqueuewatcher.bat";
                    process2.StandardInput.WriteLine(batchCommand);
                }
            }
        }
        private string FindProjectId(string zipFilePath)
        {
            var notificationManager = new NotificationManager();
            // Specify the name of the XML file within the ZIP archive
            string xmlFileName = "DownloadTask.xml";

            // Check if the ZIP file exists
            if (File.Exists(zipFilePath))
            {
                // Open the ZIP file
                using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                {
                    // Locate the XML file within the archive
                    ZipArchiveEntry xmlEntry = archive.GetEntry(xmlFileName);

                    if (xmlEntry != null)
                    {
                        // Open the XML file within the archive
                        using (Stream xmlStream = xmlEntry.Open())
                        using (StreamReader reader = new StreamReader(xmlStream))
                        {
                            // Load the XML document
                            XmlDocument doc = new XmlDocument();
                            doc.Load(reader);

                            // Find the element with the ProjectId attribute
                            XmlNode projectInfoNode = doc.SelectSingleNode("//ProjectInfo");

                            if (projectInfoNode != null)
                            {
                                // Get the ProjectId attribute value
                                string projectId = projectInfoNode.Attributes["ProjectId"].Value;

                                // Return the 'projectId' variable
                                return projectId;
                            }
                            else
                            {
                                notificationManager.Show("Error", "ProjectInfo element not found in the XML. Can't process further.");
                            }
                        }
                    }
                    else
                    {
                        notificationManager.Show("Error", "XML file not found within the ZIP archive. Use a proper offline package.");
                    }
                }
            }
            else
            {
                notificationManager.Show("Error", "ZIP file does not exist. Use a proper offline package.");
            }

            // Return null if the project ID was not found
            return null;
        }
        private async void DeleteWatcherFile(string file)
        { 
            try
            {
                // Attempt to delete the file
                File.Delete(file);
            }
            catch (IOException e)
            {
                // Handle any exceptions that occur, such as if the file does not exist or cannot be deleted
                System.Windows.MessageBox.Show("An error occurred: " + e.Message);
            }
        }
        private void UpdateFileSystemWatcher(string newDirectory)
        {
            var notificationManager = new NotificationManager();

            // Stop the current watcher to change its properties
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            notificationManager.Show("Setting update", "Changed default watcher directory to: \n" + newDirectory);

            // Create and configure a new FileSystemWatcher for the new directory
            watcher = new FileSystemWatcher(newDirectory);
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnFileCreated;
        }
    }
}