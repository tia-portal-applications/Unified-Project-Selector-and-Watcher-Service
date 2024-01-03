using System;
using System.IO;
using System.IO.Compression;

namespace Unified_Project_Selector
{
    public class ZipHelper
    {
        public static void CreateZipFile(string sourceDirectory, string destinationZipFile)
        {
            try
            {
                // Ensure the source directory exists
                if (!Directory.Exists(sourceDirectory))
                {
                    throw new DirectoryNotFoundException($"Source directory '{sourceDirectory}' not found.");
                }

                // Create the destination directory if it doesn't exist
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationZipFile));

                // Create a ZIP archive
                using (var zipArchive = ZipFile.Open(destinationZipFile, ZipArchiveMode.Create))
                {
                    AddDirectoryToZip(zipArchive, sourceDirectory, "");
                }

                Console.WriteLine($"Zip file created: {destinationZipFile}");

                // Delete all files and subdirectories inside the source directory
                DeleteFilesAndSubdirectories(sourceDirectory);

                Console.WriteLine($"Source directory '{sourceDirectory}' contents deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ZIP file or deleting source directory contents: {ex.Message}");
            }
        }

        private static void AddDirectoryToZip(ZipArchive archive, string sourceDirectory, string entryPrefix)
        {
            string[] files = Directory.GetFiles(sourceDirectory);
            string[] subDirectories = Directory.GetDirectories(sourceDirectory);

            // Add files in the current directory to the archive
            foreach (string file in files)
            {
                string entryName = Path.Combine(entryPrefix, Path.GetFileName(file));
                archive.CreateEntryFromFile(file, entryName);
            }

            // Recursively add subdirectories and their contents to the archive
            foreach (string subDirectory in subDirectories)
            {
                string entryName = Path.Combine(entryPrefix, Path.GetFileName(subDirectory));
                AddDirectoryToZip(archive, subDirectory, entryName);
            }
        }

        private static void DeleteFilesAndSubdirectories(string directoryPath)
        {
            foreach (string file in Directory.GetFiles(directoryPath))
            {
                File.Delete(file);
            }

            foreach (string subdirectory in Directory.GetDirectories(directoryPath))
            {
                DeleteFilesAndSubdirectories(subdirectory);
                Directory.Delete(subdirectory);
            }
        }
    }
}
