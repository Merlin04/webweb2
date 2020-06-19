using System;
using System.IO;
using System.Text;

namespace BlazorEditor.Utils
{
    public class MiscUtils
    {
        public static string MapPath(string path)
        {
            if (path == "/")
            {
                return Directory.GetCurrentDirectory();
            }
            string newPath = path;
            if(newPath[0] == '/')
            {
                newPath = newPath.Substring(1);
            }
            return Path.GetFullPath(newPath.Replace("~/", "")).Replace("~\\", "");
        }
        
        public static string UnmapPath(string path)
        {
            return path.Replace(Path.GetFullPath("wwwroot"), String.Empty).Substring(1);
        }

        public static bool IsDirectoryTraversal(string path)
        {
            return Path.GetFullPath(path) != path;
        }
        
        public static string Base64Encode(string plainText) {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static void EmptyDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            di.EnumerateFiles().ForEachInEnumerable(file => file.Delete());
            di.EnumerateDirectories().ForEachInEnumerable(dir => dir.Delete(true));
        }
    }
}