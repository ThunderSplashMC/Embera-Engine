using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace DevoidEngine.Engine.Utilities
{
    public class FileSystem
    {

        private static string basePath;

        public static void SetBasePath(string path)
        {
            basePath = path;
        }

        public static string GetBasePath()
        {
            return basePath;
        }

        public static string[] GetDirsFromBase(string path)
        {
            return Directory.GetDirectories(basePath + "/" + path);
        }

        public static string[] GetFilesFromBase(string path)
        {
            return Directory.GetFiles(basePath + "/" + path);
        }

        public static string RemoveBaseFromPath(string path)
        {
            return path.Remove(0, basePath.Length);
        }

        public static string GetBackPath(string path)
        {
            return Path.Combine(path, "..\\");
        }

        public static void OpenWithDefaultProgram(string path)
        {
            using Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

    }
}
