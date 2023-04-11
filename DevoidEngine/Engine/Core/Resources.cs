using System;
using System.Collections.Generic;
using System.IO;

namespace DevoidEngine.Engine.Core
{
    public struct Resource
    {
        public string Name;
        public string Ext;
        public string Path;
        public string Type;

        public static explicit operator Texture(Resource x)
        {
            if (x.Type != "Texture") return null;

            return new Texture(x.Path);
        }

        public static explicit operator Shader(Resource x)
        {
            if (x.Type != "Shader") return null;

            string path = System.IO.Path.GetFullPath(x.Path);
            return new Shader(path);
        }

    }

    public class Resources
    {
        static List<Resource> ResourcePool = new List<Resource>();

        static Dictionary<string, string> KnownFileTypes = new Dictionary<string, string>()
        {
            { "png", "Texture" },
            { "jpg", "Texture" },
            { "dmesh", "Mesh" },
            { "vert", "Shader" },
            { "frag", "Shader" },
            { "txt", "text" }
        };

        public static void AddResourceToPool(string name, string path, string ext)
        {
            ResourcePool.Add(new Resource()
            {

                Name = name,
                Ext = ext,
                Path = path,
                Type = GetKnownType(ext),

            });
        }

        public static void ClearAll()
        {
            ResourcePool.Clear();
        }

        public static void RemoveFile(string file)
        {
            for (int i = 0; i < ResourcePool.Count; i++)
            {
                if (ResourcePool[i].Name == file)
                {
                    ResourcePool.Remove(ResourcePool[i]);
                }

            }
        }

        public static Resource? Load(string file)
        {
            for (int i = 0; i < ResourcePool.Count; i++) 
            {
                if (ResourcePool[i].Name == file)
                {
                    Console.Write(ResourcePool[i].Type);
                    return ResourcePool[i];
                }
            
            }

            return null;
        }

        static string GetKnownType(string ext)
        {
            string type;

            if (KnownFileTypes.ContainsKey(ext.Substring(1, ext.Length - 1)))
            {
                type = KnownFileTypes[ext.Substring(1, ext.Length - 1)];
            } else
            {
                type = "OTHER";
            }

            return type;
        }


    }
}
