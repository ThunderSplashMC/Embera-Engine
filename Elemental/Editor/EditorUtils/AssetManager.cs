using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }
}
