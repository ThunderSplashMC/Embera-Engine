using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Elemental.Editor.EditorUtils
{
    public class ProjectUtils
    {
        string path;
        string fileContentCache;

        JsonObject jsonContentCache;

        public void LoadFile(string ProjectFilePath)
        {
            this.path = ProjectFilePath;

            using (StreamReader reader = new StreamReader(ProjectFilePath, Encoding.UTF8))
            {
                fileContentCache = reader.ReadToEnd();
            }

            jsonContentCache = JsonObject.Parse(fileContentCache).AsObject();
        }

        public void Reload()
        {
            LoadFile(path);
        }

        public string GetProjectBasePath()
        {
            return (string)jsonContentCache["Project Directory"];
        }

        public string GetProjectName()
        {
            return (string)jsonContentCache["Project Name"];
        }

        public static string CreateProject(string basePath, string name)
        {
            using (FileStream fs = File.Create(basePath + "/Project.dprj"))
            {
                JsonObject ProjectJson = new JsonObject();

                ProjectJson.Add("Project Name", name);
                ProjectJson.Add("Project Directory", basePath);

                string dataasstring = ProjectJson.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);
            }

            Directory.CreateDirectory(basePath + "/Assets");
            Directory.CreateDirectory(basePath + "/Builds");

            return basePath + "/Project.dprj";
        }

    }
}
