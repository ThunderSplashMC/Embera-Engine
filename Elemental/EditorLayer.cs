using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Components;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using ImGuiNET;
using DevoidEngine.Elemental.Panels;
using DevoidEngine.Elemental.EditorUtils;
using DevoidEngine.Engine.Serializing.Converters;

using Newtonsoft.Json;
using OpenTK.Windowing.Common;

namespace DevoidEngine.Elemental
{
    class EditorLayer : Layer
    {
        public Scene EditorScene;

        public Texture EditorLogo = new Texture("Engine/EngineContent/icons/icon512.png");

        public static ConsolePanel ConsoleService;
        public DragDropService DragDropService;

        public List<Panel> EditorPanels = new List<Panel>();

        public override void OnAttach()
        {
            Renderer.ErrorLogging(true);
            EditorScene = new Scene();

            SandBoxSetup();

            // DragDrop Functionality

            DragDropService = new DragDropService();
            ConsoleService = new ConsolePanel();

            // Scene Init

            EditorScene.Init();

            // Create Panels
            EditorPanels.Add(new ViewportPanel());
            EditorPanels.Add(new ContentBrowserPanel());
            EditorPanels.Add(new GameObjectPanel());
            EditorPanels.Add(new MaterialPanel());
            EditorPanels.Add(ConsoleService);

            // On Init
            OnInitPanels();


            EditorScene.SetSceneState(Scene.SceneState.EditorPlay);
            SetEditorStyling();

        }

        public override void KeyDown(KeyboardKeyEventArgs keyboardevent)
        {
            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].OnKeyDown(keyboardevent);
            }
        }

        public void SandBoxSetup()
        {

        }

        public void ChangeScenes(Scene scene)
        {
            EditorScene = scene;
            scene.Init();
        }

        public override void OnDetach()
        {

        }

        float deltaTime;

        public override void GUIRender()
        {
            MenuItems();
            OnGUIRenderPanels();
        }

        public override void OnRender()
        {
            OnRenderPanels();
            EditorScene.OnRender();
        }

        public override void OnUpdate(float deltaTime)
        {
            OnUpdatePanels(deltaTime);
            EditorScene.OnUpdate(deltaTime);
            ErrorLog();
            this.deltaTime = deltaTime;
        }

        public override void OnResize(int width, int height)
        {
            OnResizePanels(width, height);
            EditorScene.OnResize(width, height);
        }

        public void SetScene(Scene scene)
        {
            EditorScene = scene;
        }

        

        public void SetEditorStyling()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f,1);
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


            style.FramePadding = new System.Numerics.Vector2(10,7);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;
            style.PopupRounding = 5f;


        }

        public void OnGUIRenderPanels()
        {
            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].OnGUIRender();
            }
        }

        public void OnUpdatePanels(float deltaTime)
        {
            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].OnUpdate(deltaTime);
            }
        }

        public void OnRenderPanels()
        {
            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].OnRender();
            }
        }

        public void OnInitPanels()
        {

            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].Editor = this;
                EditorPanels[i].OnInit();
            }
        }

        public void OnResizePanels(int width, int height)
        {
            for (int i = 0; i < EditorPanels.Count; i++)
            {
                EditorPanels[i].OnResize(width, height);
            }
        }

        public void MenuItems()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("About"))
                {

                    ImGui.Text("Devoid Engine V1.0");
                    ImGui.SameLine();
                    ImGui.Image((IntPtr)(EditorLogo.GetTexture()), new System.Numerics.Vector2(64, 64));
                    ImGui.Text("Release Type: Closed Beta");
                    ImGui.Text("Game Engine in C#");
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Exit"))
                {
                    Application.Close();
                }

                ImGui.EndMenuBar();
            }
        }

        void ErrorLog()
        {
            if (RendererUtils.ErrorInfo != string.Empty)
            {
                ConsoleService.LOG(RendererUtils.ErrorInfo);
            }
            RendererUtils.ErrorInfo = string.Empty;
        }
    }
}
