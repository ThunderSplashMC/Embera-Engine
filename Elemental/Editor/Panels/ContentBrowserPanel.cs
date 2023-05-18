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
            FolderTexture = (IntPtr)(new Texture("Editor/Assets/folder-icn.png").GetRendererID());
            FileTypes.AddGenericFileTypes();
        }

        

        float padding = 16.0f;
        float thumbnailSize = 85.0f;
        float panelWidth;
        int columnCount;
        float cellSize;
        public string DragSourcePath;

        public override void OnInit()
        {
            Editor.CurrentDir = Editor.PROJECT_ASSET_DIR;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Editor.CurrentDir.Length < Editor.PROJECT_ASSET_DIR.Length) { Editor.CurrentDir = Editor.PROJECT_ASSET_DIR; }
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
                    Editor.CurrentDir = i;
                }

                ImGui.TextWrapped(fileName);
                ImGui.NextColumn();

                ImGui.PopID();
            }
            #endregion

            #region
            foreach (Asset i in GetAssets())
            {

                ImGui.PushID("file-" + i.name);
                if (ImGui.ImageButton(FileTypes.GetFileTypeIcon(i.ext.Substring(1)), new System.Numerics.Vector2(thumbnailSize - 20)))
                {

                }


                if (ImGui.BeginDragDropSource())
                {
                    Editor.DragDropService.AddDragFile(i.path);
                    ImGui.SetDragDropPayload("file", IntPtr.Zero, 0);
                    ImGui.Text(i.name);
                    ImGui.EndDragDropSource();
                }



                ImGui.TextWrapped(i.name);

                ImGui.NextColumn();

                ImGui.PopID();
            }
            ImGui.Columns(1);

            ImGui.PopStyleVar();


            ImGui.End();
            #endregion
            ImGui.Begin("Folder Hierarchy");


            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0, 0.5f));

            string backPath = Path.GetDirectoryName(Editor.CurrentDir);

            ImGui.TextDisabled($"{FontAwesome.ForkAwesome.Folder} Assets");
            //ImGui.TextWrapped($"{FontAwesome.ForkAwesome.Folder} Assets");

            if (Editor.CurrentDir.Length > Editor.PROJECT_ASSET_DIR.Length)
            {
                if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.FolderOpen} Go Back", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X, 22)))
                {
                    Editor.CurrentDir = backPath;
                }
            }

            foreach (string i in GetFolders())
            {
                string[] splitName = i.Split("\\");
                string fileName = splitName[splitName.Length - 1];
                if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.Folder} {fileName}", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X, 22)))
                {
                    Editor.CurrentDir = i;
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();

        }

        public string[] GetFolders()
        {

            return Directory.GetDirectories(Editor.CurrentDir);
        }

        public Asset[] GetAssets()
        {
            Asset[] assets = Editor.AssetManager.GetAssets(Editor.CurrentDir);

            return assets;
        }



        public override void OnKeyDown(KeyboardKeyEventArgs key)
        {

            base.OnKeyDown(key);
        }


    }
}
