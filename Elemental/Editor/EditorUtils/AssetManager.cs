using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Elemental.Editor.EditorUtils
{
    struct Asset
    {
        public string name;
        public string path;
        public string ext;
    }

    class AssetManager
    {
        public List<Asset> Assets = new List<Asset>();

        public string AssetDir;

        public AssetManager(string AssetDir) 
        { 
            this.AssetDir = AssetDir;

            LoadAllAssets(AssetDir);
        }


        public void LoadAllAssets(string basePath)
        {
            string[] Files = Directory.GetFiles(basePath);

            foreach (string File in Files) 
            {

                Asset asset = new Asset()
                {
                    name = Path.GetFileName(File),
                    path = File,
                    ext = Path.GetExtension(File)
                };

                Assets.Add(asset);
            }

            string[] Folders = Directory.GetDirectories(basePath);
            foreach (string Folder in Folders)
            {
                LoadAllAssets(Folder);
            }

        }

        public void LoadToResources()
        {
            for (int i = 0; i < Assets.Count; i++)
            { 
            
                Asset asset = Assets[i];
                Resources.AddResourceToPool(asset.name, asset.path, asset.ext);
            
            }
        }

        public Asset[] GetAssets(string path)
        {
            List<Asset> assets= new List<Asset>();

            for (int i = 0; i < Assets.Count; i++)
            {
                if (Path.GetDirectoryName(Assets[i].path) == path)
                {
                    assets.Add(Assets[i]);
                }
            }

            return assets.ToArray();
        }

        public static JsonObject CreateResourceTable()
        {
            List<Resource> ResourcePool = Resources.GetPool();

            JsonObject jsonObject = new JsonObject();

            string Asset_Dir = "Content";


            for (int i = 0; i < ResourcePool.Count; i++)
            {
                Resource res = ResourcePool[i];

                JsonObject fileObject = new JsonObject();

                if (!jsonObject.ContainsKey(res.Name))
                {
                    jsonObject.Add(res.Name, Asset_Dir + "\\" + res.Name);
                }
                continue;

                if (Resources.GetKnownType(res.Ext) != "OTHER")
                {
                    jsonObject.Add(res.Name, Asset_Dir + "\\" + Path.GetFileNameWithoutExtension(res.Name) + ".asset");
                }
                else
                {
                    jsonObject.Add(res.Name, Asset_Dir + "\\" + res.Name);
                }
            }

            return jsonObject;
        }

        public static void SaveAllResourceAsFile(string buildDir)
        {
            List<Resource> ResourcePool = Resources.GetPool();

            for (int i = 0; i < ResourcePool.Count; i++)
            {
                Resource res = ResourcePool[i];

                JsonObject fileObject = new JsonObject();

                if (File.Exists(buildDir + "\\Content\\" + res.Name)) File.Delete(buildDir + "\\Content\\" + res.Name);
                File.Copy(res.Path, buildDir + "\\Content\\" + res.Name);
                continue;

                if (Resources.GetKnownType(res.Ext) != "OTHER" || Resources.GetKnownType(res.Ext) != "Mesh")
                {
                    if (File.Exists(buildDir + "\\Content\\" + Path.GetFileNameWithoutExtension(res.Path) + ".asset")) File.Delete(buildDir + "\\Content\\" + Path.GetFileNameWithoutExtension(res.Path) + ".asset");
                    File.Copy(res.Path, buildDir + "\\Content\\" + Path.GetFileNameWithoutExtension(res.Path) + ".asset");
                }
                else
                {
                    if (File.Exists(buildDir + "\\Content\\" + res.Name)) File.Delete(buildDir + "\\Content\\" + res.Name);
                    File.Copy(res.Path, buildDir + "\\Content\\" + res.Name);
                }
            }
        }

    }
}
