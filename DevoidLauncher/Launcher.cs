using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Windowing;
using OpenTK.Windowing.Common;
using DevoidEngine.Engine.Imgui;
using ImGuiNET;
using System.Numerics;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.DevoidLauncher
{
    class Launcher
    {
        Window Window;
        ImguiLayer imguiLayer;
        IntPtr ProjectNotFoundIcon;

        public void Init()
        {
            WindowSpecification windowSpecification = new WindowSpecification()
            {
                height = 600,
                width = 800,
                fullscreen = false,
                Vsync = true,
                isCentered = true,
                title = "Devoid Launcher",
                iconPath = "Engine/EngineContent/icons/icon512-stroke.png"
            };

            Window = new DevoidEngine.Engine.Windowing.Window(ref windowSpecification);
            Window.Load += OnInit;
            Window.UpdateFrame += Loop;
            Window.Run();
        }

        public void OnInit()
        {
            ProjectNotFoundIcon = (IntPtr)(new DevoidEngine.Engine.Core.Texture("DevoidLauncher/Assets/ProjectNotFound.png").GetTexture());

            imguiLayer = new ImguiLayer();
            imguiLayer.InitIMGUI(Window);
            SetupStyle();
        }

        string ProjectName = "My New Game";
        string DirLocation = "D:/Programming/Devoid/DevoidEngine/Engine/EngineContent/models";
        bool create = false;

        public void Loop(FrameEventArgs args)
        {

            imguiLayer.Begin((float)args.Time);

            ImGui.Begin("Create A Project");

            ImGui.Text("Project Name      ");
            ImGui.SameLine();
            ImGui.InputText("##ptn", ref ProjectName, 32);

            ImGui.InvisibleButton("##placeholder", new Vector2(0, 3));

            ImGui.Separator();

            ImGui.InvisibleButton("##placeholder", new Vector2(0, 20));

            ImGui.Text("Project Directory ");
            ImGui.SameLine();
            ImGui.InputText("##dirloc", ref DirLocation, 64);
            ImGui.SameLine();
            ImGui.Button("...");

            ImGui.InvisibleButton("##placeholder", new Vector2(0, 3));

            ImGui.Separator();

            ImGui.InvisibleButton("##placeholder", new Vector2(0, 20));


            ImGui.Text("Your Projects");


            ImGui.BeginChildFrame(1, new Vector2(-1, 300));

            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.01f, 0.2f));

            create = DrawProject("Project1", "Today");

            ImGui.PopStyleVar();

            ImGui.EndChildFrame();



            Vector2 Pos = new Vector2(ImGui.GetWindowWidth() * 0.8f, ImGui.GetWindowHeight() * 0.9f);

            ImGui.SetCursorPos(Pos);

            if (ImGui.Button("Cancel"))
            {
                Quit();
            }
            ImGui.SameLine();
            if (ImGui.Button("Create"))
            {
                create = true;
            }


            ImGui.End();

            imguiLayer.End();


            imguiLayer.OnUpdate((float)args.Time);
            imguiLayer.OnRender();

            if (create)
            {
                StartEngine();
            }

        }

        bool click;

        public bool DrawProject(string name, string date)
        {

            click = ImGui.ImageButton(ProjectNotFoundIcon, new Vector2(75, 75));
            ImGui.SameLine();
            click |= ImGui.Button($"{name}\n\nLast Modified At: {date}", new Vector2(-1, 89));


            return click;
        }

        public void Quit()
        {
            Window.Close();
        }

        public void StartEngine()
        {
            Window.Close();
            Window.Dispose();
            Elemental.ElementalApp elementalApp = new Elemental.ElementalApp(DirLocation);
        }

        public void SetupStyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[((int)ImGuiCol.WindowBg)] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);
            style.Colors[((int)ImGuiCol.Header)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.2f);
            style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderActive)] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 0.5f);

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.09f, 0.09f, 0.09f, 1);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.07f, 0.07f, 0.07f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);

            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);


            style.FramePadding = new System.Numerics.Vector2(10, 7);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;
        }
    }
}
