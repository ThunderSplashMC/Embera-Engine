using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Nodes;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Core
{
    public interface IResource
    {

    }

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

        public static explicit operator Mesh[](Resource x)
        {
            if (x.Type != "Mesh") return null;

            string path = System.IO.Path.GetFullPath(x.Path);

            if (path.Split(".")[1] == "asset")
            {
                path = path.Split(".")[0] + x.Ext;
            }

            Console.WriteLine("NEWPATH");
            Console.WriteLine(path);

            ModelImporter.ModelData[] Data = ModelImporter.LoadModel(path);

            if (Data == null) return null;

            List<Mesh> meshes = new List<Mesh>();

            for (int i = 0; i < Data.Length; i++)
            {
                meshes.Add(Data[i].mesh);
            }

            return meshes.ToArray();
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
            { "txt", "text" },
            { "fbx", "Mesh" },
            { "gltf", "Mesh" },
            { "obj", "Mesh" },
            { "scene", "Scene" },
            { "cs", "Script" }
        };

        public static List<Resource> GetPool()
        {
            return ResourcePool;
        }

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
                    return ResourcePool[i];
                }

            }

            return null;
        }

        public static Resource[] GetResourcesOfType(string type)
        {
            List<Resource> resources = new List<Resource>();

            for (int i = 0; i < ResourcePool.Count; i++)
            {
                if (ResourcePool[i].Type == type)
                {
                    resources.Add(ResourcePool[i]);
                }

            }

            return resources.ToArray();
        }

        public static bool TryLoad(string value, [NotNullWhen(true)] out Resource? result)
        {
            for (int i = 0; i < ResourcePool.Count; i++)
            {
                if (ResourcePool[i].Name == value)
                {
                    result = ResourcePool[i];
                    return true;
                }

            }
            result = null;
            return false;
        }

        public static string GetKnownType(string ext)
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
