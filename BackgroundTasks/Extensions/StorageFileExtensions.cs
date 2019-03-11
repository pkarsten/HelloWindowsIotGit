using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RWPBGTasks
{
    /// <summary>
    /// Extension for Storage Files
    /// </summary>
    public static class StorageFileExtensions
    {
        public static string GetDirectory(this StorageFile file)
        {
            return file.Path.Replace("\\" + file.Name, "");
        }
        //public static string GetDirectory(this StorageFile file, string folderPath)
        //{
        //    return file.Path.Replace("\\" + folderPath, "");
        //}

        public static string GetAbsPath(this StorageFile file, string storageFolder)
        {
            return file.Path.Replace(storageFolder + "\\", "");
        }
        public static string GetAbsFolderPath(string libFolder, string picFolder)
        {
            return picFolder.Replace(libFolder, "");

        }
        public static bool LibFolderContainsTheImage(string libFolder, string picFolder)
        {
            return picFolder.Contains(libFolder);
        }
    }
}
