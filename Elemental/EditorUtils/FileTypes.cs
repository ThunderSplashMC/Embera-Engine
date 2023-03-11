using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Elemental.EditorUtils
{
    class FileTypes
    {
        static bool begun = false;

        public struct FileType
        {
            public string typeName;
            public IntPtr IconTexture;
        }

        static IntPtr defaultIcon;
        static List<FileType> fileTypes = new List<FileType>();

        public static IntPtr GetFileTypeIcon(string name)
        {
            for (int i = 0; i < fileTypes.Count; i++)
            {
                if (fileTypes[i].typeName == name)
                {
                    return fileTypes[i].IconTexture;
                }
            }
            return defaultIcon;
        }

        static void AddFileIcon(string fileExtension, string pathToIcon)
        {
            IntPtr texture = (IntPtr)(new Texture(pathToIcon).GetTexture());
            fileTypes.Add(new FileType() { IconTexture = texture, typeName = fileExtension });
        }

        public static void AddGenericFileTypes()
        {
            if (begun) { return; } else { begun = true; }

            defaultIcon = (IntPtr)(new Texture("Elemental/Assets/file-icn.png").GetTexture());
            AddFileIcon("ttf", "Elemental/Assets/font-file-icn.png");
            AddFileIcon("fbx", "Elemental/Assets/fbx-file-icn.png");
        }

    }
}
