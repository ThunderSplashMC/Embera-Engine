using System;
using System.Collections.Generic;
using ImGuiNET;
using Elemental.Editor.EditorUtils;
using DevoidEngine.Engine.Core;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;

namespace Elemental.Editor.Panels
{
    class BuildPanel : Panel
    {

        public override void OnInit()
        {

        }

        public override void OnGUIRender()
        {
            if (ImGui.Begin("Build"))
            {
                UI.BeginPropertyGrid("##BLD_OPT");

                UI.BeginProperty("Export your Game!");

                if (UI.DrawButton("Export"))
                {
                    Build();
                }

                UI.EndProperty();

                UI.EndPropertyGrid();

                ImGui.End();
            }
        }

        public void Build()
        {
            JsonObject jObject = AssetManager.CreateResourceTable();
            if (!Directory.Exists(Editor.PROJECT_BUILD_DIR + "\\Content"))
            {
                Directory.CreateDirectory(Editor.PROJECT_BUILD_DIR + "\\Content");
            }

            using (FileStream fs = File.Create(Editor.PROJECT_BUILD_DIR + "\\Content\\resourcetable.devoid"))
            {
                string dataasstring = jObject.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);
            }

            AssetManager.SaveAllResourceAsFile(Editor.PROJECT_BUILD_DIR);

            string EngineAssembly = Path.Combine(AppContext.BaseDirectory, "DevoidEngine.dll");
            string UserProject = Path.Combine(Editor.PROJECT_DIRECTORY, "MyProject.csproj");
            string RunnerProject = Path.Combine(AppContext.BaseDirectory, "DevoidPlayer", "DevoidPlayer.csproj");
            string EngineFiles = Path.Combine(AppContext.BaseDirectory, "Engine");

            string CSPROJ_TEMP2 = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n   <TargetFramework>net7.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n<ItemGroup><Reference Include=\"DevoidEngine\"><HintPath>{EngineAssembly}</HintPath></Reference></ItemGroup>\r\n</Project>";


            using (FileStream fs = File.Create(UserProject))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(CSPROJ_TEMP2);
                fs.Write(info, 0, info.Length);
            }

            ProjectUtils.BuildVSProject(Editor.PROJECT_DIRECTORY);

            string UserProjectDll = Path.Combine(Editor.PROJECT_BUILD_DIR, "MyProject.dll");

            string CSPROJ_TEMP = $"<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <PropertyGroup>\r\n    <OutputType>Exe</OutputType>\r\n    <TargetFramework>net7.0</TargetFramework>\r\n    <ImplicitUsings>enable</ImplicitUsings>\r\n    <Nullable>enable</Nullable>\r\n  </PropertyGroup>\r\n<ItemGroup><Reference Include=\"DevoidEngine\"><HintPath>{EngineAssembly}</HintPath></Reference></ItemGroup>\r\n<ItemGroup><Reference Include=\"MyProject\"><HintPath>{UserProjectDll}</HintPath></Reference></ItemGroup>\r\n</Project>";
           
            using (FileStream fs = File.Create(RunnerProject))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(CSPROJ_TEMP);
                fs.Write(info, 0, info.Length);
            }

            if (!Directory.Exists(Path.Combine(Editor.PROJECT_BUILD_DIR, "Engine")))
            {
                Directory.CreateDirectory(Path.Combine(Editor.PROJECT_BUILD_DIR, "Engine"));
            }

            ProjectUtils.CopyFilesRecursively(EngineFiles, Path.Combine(Editor.PROJECT_BUILD_DIR, "Engine"));
            ProjectUtils.BuildRunnerProject(RunnerProject, Editor.PROJECT_BUILD_DIR);

        }

    }
}
