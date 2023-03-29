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

        public override void OnInit()
        {

        }

        string CurrentDir = ".\\";

        float padding = 16.0f;
        float thumbnailSize = 85.0f;
        float panelWidth;
        int columnCount;
        float cellSize;
        public string DragSourcePath;

        public override void OnGUIRender()
        {

            ImGui.PushStyleColor(ImGuiCol.Button, System.Numerics.Vector4.Zero);

            ImGui.Begin($"{FontAwesome.ForkAwesome.FolderOpenO} Content Browser");

            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f,0.5f,0.5f,1));
            ImGui.TextWrapped(Path.GetFullPath(CurrentDir));
            ImGui.PopStyleColor();

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
                    CurrentDir = FileSystem.RemoveBaseFromPath(i);
                }
                
                ImGui.TextWrapped(fileName);
                ImGui.NextColumn();

                ImGui.PopID();
            }

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
                       OpenTextFile(Path.Join(FileSystem.GetBasePath(), CurrentDir, fileName));
                    }
                    else
                    {
                        FileSystem.OpenWithDefaultProgram(Path.Join(FileSystem.GetBasePath(), CurrentDir, fileName));
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

            ImGui.Begin("Folder Hierarchy");
            

            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0, 0.5f));

            if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.FolderOpen} Go Back", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X, 22)))
            {
                CurrentDir = FileSystem.GetBackPath(CurrentDir);
            }
            foreach (string i in GetFolders())
            {
                string[] splitName = i.Split("\\");
                string fileName = splitName[splitName.Length - 1];
                if (DevoidGUI.Button($"{FontAwesome.ForkAwesome.Folder} {fileName}", new OpenTK.Mathematics.Vector2(ImGui.GetContentRegionMax().X,22)))
                {
                    CurrentDir = FileSystem.RemoveBaseFromPath(i);
                }
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor();

            DisplayFileEditor();

        }

        string lastDirScan;
        string[] lastDirList;

        public string[] GetFolders()
        {
            if (lastDirScan == CurrentDir)
            {
                return lastDirList;
            }

            lastDirList = FileSystem.GetDirsFromBase(CurrentDir);
            lastDirScan = CurrentDir;

            return lastDirList;
        }

        string[] lastFileList;
        string lastDirFileScan;

        public string[] GetFiles()
        {
            if (lastDirFileScan == CurrentDir)
            {
                return lastFileList;
            }

            lastFileList = FileSystem.GetFilesFromBase(CurrentDir);
            lastDirFileScan = CurrentDir;

            return lastDirList;
        }

        string OpenedTXTFileContent = "";
        string openedPath = "";

        void OpenTextFile(string path)
        {
            openedPath = path;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                OpenedTXTFileContent = reader.ReadToEnd();
            }
        }

        void SaveTextFile()
        {
            using (StreamWriter writer = new StreamWriter(openedPath))
            {
                writer.Write(OpenedTXTFileContent);
            }
        }

        void DisplayFileEditor()
        {
            ImGui.Begin("Script Editor");


            ImGui.InputTextMultiline("##scripteditor", ref OpenedTXTFileContent, 20000, new System.Numerics.Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 75));


            if (ImGui.Button("Save"))
            {
                SaveTextFile();
            }

            ImGui.End();
        
        }

        

        public override void OnKeyDown(KeyboardKeyEventArgs key)
        {

            base.OnKeyDown(key);
        }


    }
}
