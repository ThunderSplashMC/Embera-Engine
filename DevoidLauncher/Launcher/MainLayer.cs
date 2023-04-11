using DevoidEngine.Engine.Core;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using ImGuiNET;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using DevoidEngine.Engine.Utilities;
using Elemental;
using Elemental.Editor.EditorUtils;
using System.Text;
using System.Text.Json.Nodes;
using OpenTK.Mathematics;

namespace DevoidLauncher.Launcher
{
    class MainLayer : Layer
    {
        public enum Launcher_State
        {
            Projects,
            Setup
        }

        public Main main;

        string PROJECT_NAME = "My Demo Project";

        float padding = 16.0f;
        float thumbnailSize = 85.0f;
        float panelWidth;
        int columnCount;
        float cellSize;

        Launcher_State launcher_State = Launcher_State.Projects;

        Texture ProjectIcon;
        Texture DevoidBanner;

        string SELECTED_FILE;

        ImGuiWindowClassPtr WClass;

        public override void OnAttach()
        {
            ProjectIcon = new Texture("Launcher/Assets/project-icn.png");
            DevoidBanner = new Texture("Launcher/Assets/DevoidEngine-Banner(2).png");

            ImGui.LoadIniSettingsFromMemory(INI_DATA);

            SetEditorStyling();
        }

        public override void OnDetach()
        {
            base.OnDetach();
        }

        public override void GUIRender()
        {
            ImGuiWindowClass iClass = new ImGuiWindowClass();

            iClass.DockNodeFlagsOverrideSet = ImGuiDockNodeFlags.AutoHideTabBar;

            unsafe
            {
                ImGuiWindowClass* ptr = &iClass;

                WClass = new ImGuiWindowClassPtr(ptr);
            }

            if (launcher_State == Launcher_State.Projects)
            {
                ShowProjects();
            }
            if (launcher_State == Launcher_State.Setup)
            {
                ShowSetup();
            }
            
        }

        public void ShowProjects()
        {
            ImGui.SetNextWindowClass(WClass);
            bool val = false;
            ImGui.Begin("Projects", ref val, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDecoration);

            ImGui.Image((IntPtr)(DevoidBanner.GetTexture()), new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, 175));

            ImGui.Separator();

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.7f));

            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.0f);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(5,5));

            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5f);


            ImGui.BeginChild("##PRJS", new System.Numerics.Vector2(-1, 330), true, ImGuiWindowFlags.AlwaysUseWindowPadding);

            panelWidth = ImGui.GetContentRegionAvail().X;
            cellSize = thumbnailSize + padding + ImGui.GetStyle().FramePadding.X;
            columnCount = (int)(panelWidth / cellSize);


            if (columnCount < 1)
                columnCount = 1;

            ImGui.Columns(columnCount, "", false);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);

            for (int i = 0; i < 10; i++)
            {
                ImGui.PushID("folder-" + i);

                if (ImGui.ImageButton((IntPtr)(ProjectIcon.GetTexture()), new System.Numerics.Vector2(thumbnailSize, thumbnailSize)))
                {

                }

                ImGui.TextWrapped("Project " + i);
                ImGui.NextColumn();
            }

            ImGui.PopID();

            ImGui.EndChild();

            ImGui.PopStyleVar(4);

            ImGui.PopStyleColor();

            if (ImGui.Button("Create Project", new System.Numerics.Vector2(-1, 30)))
            {
                launcher_State = Launcher_State.Setup;
            }

            if (ImGui.Button("Open From Directory", new System.Numerics.Vector2(-1, 30)))
            {
                OpenFileDialog();
                //Console.WriteLine(SELECTED_FILE);
                LoadProject(main.PROJECT_FILE_PATH);
            }


            ImGui.End();
        }

        public void LoadProject(string basePath)
        {
            string fileContentCache;

            using (StreamReader reader = new StreamReader(basePath, Encoding.UTF8))
            {
                fileContentCache = reader.ReadToEnd();
            }

            JsonObject json = JsonObject.Parse(fileContentCache).AsObject();

            string project_Path = (string)json["Project Directory"];

            Console.WriteLine(project_Path);

            main.PROJECT_DIRECTORY = project_Path;
            main.EDITOR_STARTED = true;
        }

        public void ShowSetup()
        {
            ImGui.SetNextWindowClass(WClass);
            ImGui.Begin("Setup Project");

            ImGui.Text("Project Name");

            ImGui.InputText("##PRJ-NAME", ref PROJECT_NAME, 128);

            ImGui.Separator();

            ImGui.Text("Project Directory");

            ImGui.InputText("##PRJ-DIR", ref main.PROJECT_DIRECTORY, 128);

            ImGui.SameLine();

            if (ImGui.Button("Browse"))
            {
                OpenFolderDialog();
                if (FileSystem.GetDirsFromBase(main.PROJECT_DIRECTORY).Length != 0 || FileSystem.GetFilesFromBase(main.PROJECT_DIRECTORY).Length != 0)
                {
                    main.PROJECT_DIRECTORY = ".";
                }
            }

            if (ImGui.Button("Create Project"))
            {
                main.EDITOR_STARTED = true;
                main.PROJECT_FILE_PATH = ProjectUtils.CreateProject(main.PROJECT_DIRECTORY, PROJECT_NAME);
            }
            ImGui.End();
        }

        public override void KeyDown(KeyboardKeyEventArgs keyboardevent)
        {
            base.KeyDown(keyboardevent);
        }

        public void OpenFolderDialog()
        {
            Thread thread = new Thread(() => {

                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                // Show open file dialog box
                bool? result = dialog.ShowDialog() == CommonFileDialogResult.Ok ? true : false;

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    string filename = dialog.FileName;
                    main.PROJECT_DIRECTORY = filename;
                }

            });
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
        }

        public void OpenFileDialog()
        {
            Thread thread = new Thread(() => {

                var dialog = new CommonOpenFileDialog();

                dialog.EnsureFileExists = true;
                dialog.DefaultExtension = "dprj";

                // Show open file dialog box
                bool? result = dialog.ShowDialog() == CommonFileDialogResult.Ok ? true : false;

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    main.PROJECT_FILE_PATH = dialog.FileName;
                }

            });
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
        }

        public void SetEditorStyling()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[((int)ImGuiCol.WindowBg)] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);
            style.Colors[((int)ImGuiCol.Header)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderActive)] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 0.5f);

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.09f, 0.09f, 0.09f, 1);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.07f, 0.07f, 0.07f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);

            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);


            style.FramePadding = new System.Numerics.Vector2(10, 7);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;
            style.PopupRounding = 5f;
        }

        static string INI_DATA = "[Window][Projects]\r\nPos=0,0\r\nSize=800,600\r\nCollapsed=0\r\nDockId=0x00000008,0\r\n[Window][Setup Project]\r\nPos=0,0\r\nSize=800,600\r\nCollapsed=0\r\nDockId=0x00000008,0\r\n\r\n[Table][0xE8F5176F,2]\r\nColumn 0  Weight=1.4392\r\nColumn 1  Weight=0.5608\r\n\r\n[Docking][Data]\r\nDockSpace         ID=0x33675C32 Window=0x5B816B74 Pos=0,0 Size=800,600 Split=X Selected=0x90DEF4FD\r\n  DockNode        ID=0x00000001 Parent=0x33675C32 SizeRef=1409,1031 Split=X\r\n    DockNode      ID=0x00000002 Parent=0x00000001 SizeRef=323,1031 Selected=0x510084CD\r\n    DockNode      ID=0x00000004 Parent=0x00000001 SizeRef=1084,1031 Split=Y\r\n      DockNode    ID=0x00000008 Parent=0x00000004 SizeRef=401,404 CentralNode=1 Selected=0xAC57D6BD\r\n      DockNode    ID=0x00000003 Parent=0x00000004 SizeRef=1517,625 Split=X Selected=0x6BFAD16C\r\n        DockNode  ID=0x00000006 Parent=0x00000003 SizeRef=322,481 Selected=0x6BFAD16C\r\n        DockNode  ID=0x00000007 Parent=0x00000003 SizeRef=760,481 Selected=0xAE98FAB7\r\n  DockNode        ID=0x00000005 Parent=0x33675C32 SizeRef=509,1031 Selected=0xEB63DFD6";
    }
}
