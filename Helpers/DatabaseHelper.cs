using System;
using System.IO;

namespace CebuanaReFind.Helpers
{
    public class DatabaseHelper
    {
        private static readonly string appFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CebuanaReFind"
            );

        private static readonly string pathFile =
            Path.Combine(appFolder, "path.txt"); // stores selected folder

        private const string fileName = "CebuanaConnection.txt";

        // Save both connection and selected path
        public static void SaveConnection(string folderPath, string connectionString)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fullPath = Path.Combine(folderPath, fileName);

            File.WriteAllText(fullPath, connectionString);

            // Save the selected path for future use
            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            File.WriteAllText(pathFile, folderPath);
        }

        public static string LoadConnection()
        {
            if (!File.Exists(pathFile))
                return string.Empty;

            string folderPath = File.ReadAllText(pathFile);
            string fullPath = Path.Combine(folderPath, fileName);

            if (File.Exists(fullPath))
                return File.ReadAllText(fullPath);

            return string.Empty;
        }

        public static string GetSavedPath()
        {
            return File.Exists(pathFile) ? File.ReadAllText(pathFile) : string.Empty;
        }
    }
}