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

namespace DevoidEngine.Elemental
{
    class EditorLayer : Layer
    {
        public Scene EditorScene;

        public static ConsolePanel ConsoleService;
        public DragDropService DragDropService;

        public List<Panel> EditorPanels = new List<Panel>();

        public override void OnAttach()
        {
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
            EditorPanels.Add(ConsoleService);

            // On Init

            OnInitPanels();

            EditorScene.SetSceneState(Scene.SceneState.EditorPlay);
            SetEditorStyling();
            
        }

        public void SandBoxSetup()
        {
            //PostEffectSettings.BloomSettings bloomSettings =  Renderer3D.GetBloomSettings();
            //bloomSettings.enabled = true;
            //Mesh[] meshes = ModelImporter.LoadModel("Engine/EngineContent/models/sphere.fbx");
            //Renderer3D.SetBloomSettings(bloomSettings);

            //for (int i = 0; i < 25; i++)
            //{
            //    GameObject Sobject = EditorScene.NewGameObject("SINOBJ" + i);
            //    Sobject.transform.position = new Vector3(i * 4f, (float)Math.Sin(i) * 4.5f, 0);
            //    Sobject.transform.rotation.X = 90f;
            //    MeshHolder mh = Sobject.AddComponent<MeshHolder>();



            //    mh.AddMeshes(meshes);

            //    MeshRenderer meshRenderer = Sobject.AddComponent<MeshRenderer>();

            //}
        }

        public override void OnDetach()
        {
        }

        
        public override void GUIRender()
        {
            //MenuItems();
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
            style.Colors[((int)ImGuiCol.Header)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.2f);
            style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderActive)] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 0.5f);

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.09f, 0.09f, 0.09f, 1);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.07f, 0.07f, 0.07f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);

            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);


            style.FramePadding = new System.Numerics.Vector2(10,7);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;


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
                if (ImGui.BeginMenu("Hello World"))
                {
                    ImGui.MenuItem("Hello World1");


                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }
    }
}
