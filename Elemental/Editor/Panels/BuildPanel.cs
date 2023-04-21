using System;
using System.Collections.Generic;
using ImGuiNET;
using Elemental.Editor.EditorUtils;
using DevoidEngine.Engine.Core;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using FontAwesome;

namespace Elemental.Editor.Panels
{
    class BuildPanel : Panel
    {

        Resource[] SceneList;

        public override void OnInit()
        {
            SceneList = Resources.GetResourcesOfType("Scene");
        }

        bool isDragging = false;
        int dragItem = -1;

        public override void OnGUIRender()
        {
            //ImGui.ShowDemoWindow();
            if (ImGui.Begin("Build"))
            {
                ImGui.Text("Scene Build Hierarchy");

                ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.7f));

                ImGui.BeginListBox("##Scene Hier", new System.Numerics.Vector2(-1, 400));
             

                ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f));

                for (int i = 0; i < SceneList.Length; i++)
                {
                    Vector2 cursorPos = ImGui.GetCursorPos();
                    ImGui.Button(SceneList[i].Name, new System.Numerics.Vector2(-1, 30));


                    if (ImGui.BeginDragDropSource())
                    {
                        dragItem = i;
                        ImGui.SetDragDropPayload("scene-reorder", IntPtr.Zero, 0);
                        ImGui.Button(SceneList[i].Name, new System.Numerics.Vector2(200, 30));
                        ImGui.EndDragDropSource();
                    }
                    if (ImGui.BeginDragDropTarget())
                    {
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        {
                            Resource scene = SceneList[i];
                            SceneList[i] = SceneList[dragItem];
                            SceneList[dragItem] = scene;
                            ImGui.AcceptDragDropPayload("scene-reorder");
                            ImGui.EndDragDropTarget();
                        }
                    }

                    if (ImGui.GetWindowWidth() > 200)
                    {
                        Vector2 newCursorPos = ImGui.GetCursorPos();
                        ImGui.SetCursorPos(new Vector2(cursorPos.X + 7, cursorPos.Y + 7));
                        ImGui.TextDisabled(ForkAwesome.ArrowsV);
                        ImGui.SetCursorPos(newCursorPos);
                    }
                }

                ImGui.PopStyleColor();

                ImGui.EndListBox();

                if (ImGui.BeginDragDropTarget())
                {
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        DragFileItem dfi = Editor.DragDropService.GetDragFile();
                        if (dfi.fileextension == ".scene")
                        {
                            Resource? res = Resources.Load(dfi.fileName);
                            if (res != null)
                            {
                                SceneList = SceneList.Concat(new Resource[] { (Resource)res }).ToArray<Resource>();
                            }
                        }

                    }
                    ImGui.EndDragDropTarget();
                }

                ImGui.PopStyleColor();

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

        float timeElapsed = 0f;

        public override void OnUpdate(float deltaTime)
        {
            return;
            if (timeElapsed > 5)
            {
                SceneList = Resources.GetResourcesOfType("Scene");
                timeElapsed = 0;
            }
            timeElapsed += deltaTime;
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

            using (FileStream fs = File.Create(Editor.PROJECT_BUILD_DIR + "\\Content\\scenetable.devoid"))
            {
                JsonObject sceneJson = new JsonObject();
                JsonArray sceneArray = new JsonArray();
                for (int i = 0; i < SceneList.Length; i++)
                {
                    sceneArray.Add(new JsonObject()
                    {
                        { "Order", i },
                        { "ScenePath", "Content\\" + SceneList[i].Name },
                        { "SceneName", SceneList[i].Name }
                    });
                }

                sceneJson.Add("Scenes", sceneArray);

                string dataasstring = sceneJson.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
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
