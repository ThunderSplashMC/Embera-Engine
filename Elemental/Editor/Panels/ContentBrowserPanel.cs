using System;
using System.Collections.Generic;
using System.IO;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using ImGuiNET;
using Elemental.Editor.EditorUtils;
using DevoidEngine.Engine.GUI;
using System.Text;
using OpenTK.Windowing.Common;

namespace Elemental.Editor.Panels
{
    class ContentBrowserPanel : Panel
    {
        IntPtr FolderTexture;

        public ContentBrowserPanel()
        {
            FolderTexture = (IntPtr)(new Texture("Editor/Assets/folder-icn.png").GetTexture());
            FileTypes.AddGenericFileTypes();
        }

        string CurrentDir = "";

        float padding = 16.0f;
        float thumbnailSize = 85.0f;
        float panelWidth;
        int columnCount;
        float cellSize;
        public string DragSourcePath;

        public override void OnInit()
        {
            CurrentDir = Editor.PROJECT_ASSET_DIR;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (CurrentDir.Length < Editor.PROJECT_ASSET_DIR.Length) { CurrentDir = Editor.PROJECT_ASSET_DIR; }
        }

        public override void OnGUIRender()
        {
            #region
            ImGui.PushStyleColor(ImGuiCol.Button, System.Numerics.Vector4.Zero);

            ImGui.Begin($"{FontAwesome.ForkAwesome.FolderOpenO} Content Browser");

            //ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1));
            //ImGui.TextWrapped( Editor.Application. );
            //ImGui.PopStyleColor();

            panelWidth = ImGui.GetContentRegionAvail().X;
            cellSize = thumbnailSize + padding + ImGui.GetStyle().FramePadding.X;
            columnCount = (int)(panelWidth / cellSize);


            if (columnCount < 1)
                columnCount = 1;

            ImGui.Columns(columnCount, "", false);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
            foreach (string i in GetFolders())
            {
                ImGui.PushID("folder-" + i);

                string[] splitName = i.Split("\\");
                string fileName = splitName[splitName.Length - 1];

                if (ImGui.ImageButton(FolderTexture, new System.Numerics.Vector2(thumbnailSize, thumbnailSize)))
                {
                    CurrentDir = i;
                }

                ImGui.TextWrapped(fileName);
                ImGui.NextColumn();

                ImGui.PopID();
            }
            #endregion

            #region
            foreach (string i in GetFiles())
            {
                ImGui.PushID("file-" + i);

                string[] splitName = i.Split("\\");
                string fileName = splitName[splitName.Length - 1];
                string[] splitfileExtension = fileName.Split(".");
                string fileExtension = "";
                if (splitfileExtension.Length > 1)
                {
                    fileExtension = splitfileExtension[1];
                }

                if (ImGui.ImageButton(FileTypes.GetFileTypeIcon(fileExtension), new System.Numerics.Vector2(thumbnailSize, thumbnailSize)))
                {
                    if (fileExtension == "cs" || fileExtension == "txt")
                    {
                        //OpenTextFile(Path.Join(FileSystem.GetBasePath(), CurrentDir, fileName));
                    }
                    else
                    {
                        FileSystem.OpenWithDefaultProgram(i);
                    }

                }


                if (ImGui.BeginDragDropSource())
                {
                    Editor.DragDropService.AddDragFile(Path.GetFullPath(Path.Join(FileSystem.GetBasePath(), CurrentDir, fileName)));
                    ImGui.SetDragDropPayload("file", IntPtr.Zero, 0);
                    ImGui.Text(fileName);
                    ImGui.EndDragDropSource();
                }



                ImGui.TextWrapped(fileName);

                ImGui.NextColumn();

                ImGui.PopID();
            }
            ImGui.Columns(1);

            ImGui.PopStyleVar();


            ImGui.End();
            #endregion
            ImGui.Begin("Folder Hierarchy");


            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0, 0.5f));

            string backPath = Path.GetDirectoryName(CurrentDir);

            if (CurrentDir.Length > Editor.PROJECT_ASSET_DIR.Length)
            {
                if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.FolderOpen} Go Back", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X, 22)))
                {
                    CurrentDir = backPath;
                }
            }

            foreach (string i in GetFolders())
            {
                string[] splitName = i.Split("\\");
                string fileName = splitName[splitName.Length - 1];
                if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.Folder} {fileName}", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X, 22)))
                {
                    CurrentDir = i;
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();

        }

        public string[] GetFolders()
        {

            return Directory.GetDirectories(CurrentDir);
        }

        public string[] GetFiles()
        {
            Console.WriteLine(CurrentDir);
            Asset[] assets = Editor.AssetManager.GetAssets(CurrentDir);
            string[] assetPaths = new string[assets.Length];

            for (int i = 0; i < assets.Length; i++) 
            {

                assetPaths[i] = assets[i].path + "\\" + assets[i].name;

            }

            return assetPaths;
        }



        public override void OnKeyDown(KeyboardKeyEventArgs key)
        {

            base.OnKeyDown(key);
        }


    }
}
