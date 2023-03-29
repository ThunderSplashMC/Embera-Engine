using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace Elemental.Editor.EditorUtils
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

            defaultIcon = (IntPtr)(new Texture("Editor/Assets/file-icn.png").GetTexture());
            AddFileIcon("ttf", "Editor/Assets/font-file-icn.png");
            AddFileIcon("fbx", "Editor/Assets/fbx-file-icn.png");
            AddFileIcon("cs", "Editor/Assets/script-icn.png");
            AddFileIcon("dscene", "Editor/Assets/scene-icn.png");
            AddFileIcon("scene", "Editor/Assets/scene-icn.png");
        }

    }
}
