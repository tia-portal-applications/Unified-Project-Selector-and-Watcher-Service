using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Unified_Project_Selector
{
    public class XmlHelper
    {
        public string xmlFilePath;

        public XmlHelper(string filePath)
        {
            xmlFilePath = filePath;
        }

        public void CreateXmlDocument(
            string directoryPath,
            string projectId,
            string esProjectName,
            string esDeviceName,
            string rtProjectFolderName,
            string rtProjectName,
            string deviceType,
            string deviceVersion,
            string creationTime,
            string firmwareFileName,
            string runtimeFileName,
            string prcv,
            string rtcv,    
            string rtReleaseNo,
            string imageReleaseNo)
        {
            try
            {
                // Create the XmlWriterSettings to omit the encoding
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true // Optionally, you can set indentation for a well-formatted XML
                };

                // Create a new XDocument with the custom XmlWriterSettings
                XDocument xDoc = new XDocument();

                // Create the XML declaration with version 1.0
                xDoc.Declaration = new XDeclaration("1.0", null, null);

                // Create the root element with namespaces
                XNamespace xsd = "http://www.w3.org/2001/XMLSchema";
                XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                XElement rootElement = new XElement("DownloadTask",
                    new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi)
                );

                // Create the ProjectInfo element with the specified attributes
                XElement projectInfoElement = new XElement("ProjectInfo",
                    new XAttribute("ProjectId", projectId), // Use the string value directly
                    new XAttribute("ESProjectName", esProjectName),
                    new XAttribute("ESDeviceName", esDeviceName),
                    new XAttribute("RtProjectFolderName", rtProjectFolderName),
                    new XAttribute("RtProjectName", rtProjectName),
                    new XAttribute("DeviceType", deviceType),
                    new XAttribute("DeviceVersion", deviceVersion),
                    new XAttribute("CreationTime", creationTime),
                    new XAttribute("FirmwareFileName", firmwareFileName),
                    new XAttribute("RuntimeFileName", runtimeFileName),
                    new XAttribute("PRCV", prcv),
                    new XAttribute("RTCV", rtcv),
                    new XAttribute("RTReleaseNo", rtReleaseNo),
                    new XAttribute("ImageReleaseNo", imageReleaseNo)
                );

                // Add the ProjectInfo element to the root element
                rootElement.Add(projectInfoElement);

                // Get the directory's full path to calculate relative paths
                string directoryFullPath = Path.GetFullPath(directoryPath);

                // Iterate through the files in the directory and its subdirectories
                IEnumerable<string> files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    // Calculate the relative path by removing the directory's full path
                    string relativePath = filePath.Substring(directoryFullPath.Length);

                    // Normalize the relative path to ensure it uses forward slashes
                    relativePath = relativePath.Replace('\\', '/');

                    // Remove the leading '/' character if it exists
                    if (relativePath.StartsWith("/"))
                    {
                        relativePath = relativePath.Substring(1);
                    }

                    // Create a FullDownload element for each file with the relative path
                    XElement fullDownloadElement;

                    if (Path.GetFileName(filePath).Equals("UMCData.json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Special case for "UMCData.json"
                        fullDownloadElement = new XElement("FullDownload",
                            new XAttribute("type", "ADMINDATA"),
                            relativePath);
                    }
                    else
                    {
                        // Regular FullDownload element
                        fullDownloadElement = new XElement("FullDownload", relativePath);
                    }

                    rootElement.Add(fullDownloadElement);
                }

                // Add the root element to the XDocument
                xDoc.Add(rootElement);

                string filePathFile = Path.Combine(directoryPath, "DownloadTask.xml");
                // Create a custom XmlWriter with the specified settings
                using (XmlWriter xmlWriter = XmlWriter.Create(filePathFile, settings))
                {
                    // Write the root element to the custom XmlWriter
                    xDoc.WriteTo(xmlWriter);
                }

                //Console.WriteLine("XML document created and saved successfully.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating XML document: " + ex.Message);
            }
        }
        public void CreateStaticDownloadConfigXml(string outputDirectory)
        {
            try
            {
                // Ensure the output directory exists; create it if not
                Directory.CreateDirectory(outputDirectory);

                // Combine the directory path and file name to get the full file path
                string filePath = Path.Combine(outputDirectory, "Download.config");

                // Create the XmlWriterSettings to specify encoding
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = System.Text.Encoding.UTF8
                };

                // Create a custom XmlWriter with the specified settings
                using (XmlWriter xmlWriter = XmlWriter.Create(filePath, settings))
                {
                    // Start the document and write the root element
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("ServiceAndCommissioning");

                    // Write the Download element
                    xmlWriter.WriteStartElement("Download");

                    // Write the useradmin element with attributes
                    xmlWriter.WriteStartElement("useradmin");
                    xmlWriter.WriteAttributeString("keep", "0");
                    xmlWriter.WriteAttributeString("force", "0");
                    xmlWriter.WriteEndElement(); // Close useradmin

                    xmlWriter.WriteEndElement(); // Close Download
                    xmlWriter.WriteEndElement(); // Close ServiceAndCommissioning

                    // End the document
                    xmlWriter.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating Static Download.config XML file: " + ex.Message);
            }
        }
    }
}