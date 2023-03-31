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

namespace DevoidLauncher.Launcher
{
    class MainLayer : Layer
    {
        public Main main;

        string PROJECT_NAME = "My Demo Project";

        public override void OnAttach()
        {
            SetEditorStyling();
        }

        public override void OnDetach()
        {
            base.OnDetach();
        }

        public override void GUIRender()
        {
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
    }
}
