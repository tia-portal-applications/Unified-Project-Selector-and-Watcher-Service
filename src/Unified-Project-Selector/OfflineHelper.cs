using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Unified_Project_Selector
{
    public class OfflineHelper
    {
        public async static void OfflinePackageCreator(string sourceDirectory, string zipName, ProjectData PD)
        {            
            string destinationDirectory = @"C:\Users\Public\Documents\Unified Project Selector\temp";

            // Define the values for the attributes in ProjectInfo
            string projectId = PD.ProjectID; //eg: "9e718838-50f0-6e2d-6483-84c522750949";            
            string esProjectName = PD.ProjectName; //eg: "testProject";            
            string esDeviceName = PD.DeviceName; //eg: "hmi_rt_1";
            string rtProjectFolderName = esProjectName + ".PC-System_1[SIMATIC PC station - WinCC Unified PC RT]";   
            string rtProjectName = rtProjectFolderName;

            const string deviceType = "SIMATIC PC station - WinCC Unified PC RT";

            string version = MainWindow.ReadSettings(null, null);
            string deviceVersion = version;

            // Creates and add date to the name of the offline package
            DateTime CT_dateTime = DateTime.Now;
            string creationTime = CT_dateTime.ToString("dd/MM/yyyy HH:mm:ss");

            // This has to be checked with a real offline package if used with other versions than V18 update 2
            const string firmwareFileName = "";
            const string runtimeFileName = "";
            const string prcv = "4.1.2.0";
            const string rtcv = "0.0.0.0";
            string rtReleaseNo = version;
            const string imageReleaseNo = "0.0.0.0";

            // Define the directory path
            string directoryPath = destinationDirectory;

            // Create an instance of the XmlHelper class
            XmlHelper xmlHelper = new XmlHelper(destinationDirectory);

            string sourceZipDirectory = destinationDirectory;
            string destinationZipFile = @"C:\Users\Public\Documents\Unified Project Selector\offline\" + zipName;

            try
            {
                sourceDirectory = sourceDirectory.Replace(" ", "_");
                await Task.Run(() => RTManHelper.CopyConfigAndCurrentConfiguration(sourceDirectory, destinationDirectory));

                while (true)
                {
                    // Check if the directory is not empty
                    if (Directory.GetFiles(destinationDirectory).Any())
                    {

                        break;
                    }

                    // Delay to avoid busy-waiting
                    Thread.Sleep(1000);
                }

                string fileNameToCheck1 = "DownloadTask.xml";
                string fileNameToCheck2 = "Download.config";

                // Combine the directory path and file name to create the full file path
                string filePathToCheck1 = System.IO.Path.Combine(directoryPath, fileNameToCheck1);
                string filePathToCheck2 = System.IO.Path.Combine(directoryPath, fileNameToCheck2);

                // Check if the file exists
                if (File.Exists(filePathToCheck1) || File.Exists(filePathToCheck2))
                {
                    // Continue
                }
                else
                {
                    await Task.Run(() => xmlHelper.CreateStaticDownloadConfigXml(destinationDirectory));
                    await Task.Run(() => xmlHelper.CreateXmlDocument(destinationDirectory, projectId, esProjectName, esDeviceName, rtProjectFolderName, rtProjectName, deviceType, deviceVersion, creationTime, firmwareFileName, runtimeFileName, prcv, rtcv, rtReleaseNo, imageReleaseNo));
                }
                await Task.Run(() => ZipHelper.CreateZipFile(sourceZipDirectory, destinationZipFile));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        public static bool CheckName(string filePath, string searchString)
        {
            try
            {
                // Replace spaces with underscores in the file path
                filePath = filePath.Replace(" ", "_");

                if (!File.Exists(filePath))
                {
                    System.Windows.MessageBox.Show("The file does not exist.");
                    return false;
                }

                string fileContent = File.ReadAllText(filePath);

                if (fileContent.Contains(searchString))
                {
                    //System.Windows.MessageBox.Show("The string exists in the file.");
                    return true;
                }
                else
                {
                    //System.Windows.MessageBox.Show("The string does not exist in the file.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }
    }
}
