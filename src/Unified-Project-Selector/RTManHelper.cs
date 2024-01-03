using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unified_Project_Selector
{
    public class RTManHelper
    {
        public static void CopyFolder(string sourceFolder, string destinationFolder)
        {
            try
            {
                // Check if the source folder exists
                if (!Directory.Exists(sourceFolder))
                {
                    System.Windows.MessageBox.Show("Source folder does not exist.");
                    return;
                }

                // Create the destination folder if it doesn't exist
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Get the files in the source folder
                string[] files = Directory.GetFiles(sourceFolder);

                // Copy each file to the destination folder
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string destPath = System.IO.Path.Combine(destinationFolder, fileName);
                    File.Copy(file, destPath, true); 
                }

                // Get the subdirectories in the source folder
                string[] subDirectories = Directory.GetDirectories(sourceFolder);

                // Recursively copy each subdirectory
                foreach (string subDirectory in subDirectories)
                {
                    string subDirectoryName = System.IO.Path.GetFileName(subDirectory);
                    string destSubDirectory = System.IO.Path.Combine(destinationFolder, subDirectoryName);
                    CopyFolder(subDirectory, destSubDirectory);
                }
            }
            catch (Exception ex)
            {
                // For debug
                // System.Windows.MessageBox.Show($"Error copying folder: {ex.Message}");
            }
        }
        public static void CopyFolderContents(string sourceFolder, string destinationFolder)
        {
            try
            {
                // Check if the source folder exists
                if (!Directory.Exists(sourceFolder))
                {
                    System.Windows.MessageBox.Show("Source folder does not exist.");
                    return;
                }

                // Create the destination folder if it doesn't exist
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Get the files in the source folder
                string[] files = Directory.GetFiles(sourceFolder);

                // Copy each file to the destination folder
                foreach (string file in files)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string destPath = System.IO.Path.Combine(destinationFolder, fileName);
                    File.Copy(file, destPath, true); 
                }

                // Get the subdirectories in the source folder
                string[] subDirectories = Directory.GetDirectories(sourceFolder);

                // Recursively copy each subdirectory
                foreach (string subDirectory in subDirectories)
                {
                    string subDirectoryName = System.IO.Path.GetFileName(subDirectory);
                    string destSubDirectory = System.IO.Path.Combine(destinationFolder, subDirectoryName);
                    CopyFolder(subDirectory, destSubDirectory);
                }
            }
            catch (Exception ex)
            {
                // For debug
                // System.Windows.MessageBox.Show($"Error copying folder contents: {ex.Message}");
            }
        }
        public static void CopyConfigAndCurrentConfiguration(string sourceDirectory, string destinationDirectory)
        {
            string configSourceFolder = System.IO.Path.Combine(sourceDirectory, "config");
            string currentConfigSourceFolder = System.IO.Path.Combine(sourceDirectory, "currentConfiguration");
            string destinationConfigFolder = System.IO.Path.Combine(destinationDirectory, "config");

            // Copy the "config" folder to the root of the destination directory
            CopyFolder(configSourceFolder, destinationConfigFolder);

            // Copy the contents of the "currentConfiguration" folder to the root of the destination directory
            CopyFolderContents(currentConfigSourceFolder, destinationDirectory);

            // For debug
            // System.Windows.MessageBox.Show("Config and currentConfiguration copied successfully.");
        }
        public static void FullDownloadZip(string zipPath)
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
                process.Start();

                // Send the first batch command to the process
                string batchCommand1 = "cd C:\\Program Files\\Siemens\\Automation\\WinCCUnified\\bin";
                process.StandardInput.WriteLine(batchCommand1);

                string batchCommand2 = "SIMATICRuntimeManager.exe -s -quiet -c fulldownload " + zipPath;
                process.StandardInput.WriteLine(batchCommand2);

            }
        }
    }
}
