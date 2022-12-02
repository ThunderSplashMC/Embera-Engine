using System;
using System.Collections.Generic;
using DevoidEngine.Engine.GUI;
using OpenTK.Mathematics;
using ImGuiNET;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using DevoidEngine.Elemental.EditorUtils;
using DevoidEngine.Engine.Components;

// REMOVE!!!
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Elemental.Panels
{
    class ViewportPanel : Panel
    {
        private EditorCamera editorCamera;
        public int ViewportTexture;
        private float prev_h, prev_w;
        private PostEffectSettings.BloomSettings bloomSettings;

        private Texture SampleTex;
        private Texture LightComponentTex;
        private Texture CameraComponentTex;


        public override void OnInit()
        {
            ViewportTexture = RenderGraph.CompositePass.GetColorAttachment(0);
            bloomSettings = Renderer3D.GetBloomSettings();
            editorCamera = new EditorCamera(MathHelper.DegreesToRadians(45.0f), (int)Editor.Application.GetWindowSize().X, (int)Editor.Application.GetWindowSize().Y, 1000f, 0.1f);
            Renderer3D.SetCamera(editorCamera.Camera);

            Guizmo3D.Init((int)Editor.Application.GetWindowSize().X, (int)Editor.Application.GetWindowSize().Y, RenderGraph.CompositePass);

            SampleTex = new Texture("Elemental/Assets/folder-icn.png");
            SampleTex.ChangeFilterType(MagMinFilterTypes.Nearest);
            LightComponentTex = new Texture("Elemental/Assets/LightComponentTexture.png");
            LightComponentTex.ChangeFilterType(MagMinFilterTypes.Linear);
            CameraComponentTex = new Texture("Elemental/Assets/CameraComponentTexture.png");
            CameraComponentTex.ChangeFilterType(MagMinFilterTypes.Linear);

            //{
            //    GameObject Omoli = Editor.EditorScene.NewGameObject("Omoli");
            //    SpriteRenderer _2DSpriteRenderer = Omoli.AddComponent<SpriteRenderer>();
            //_2DSpriteRenderer.Texture = new Texture("Engine/EngineContent/models/textures/omoli.png");
            //}

            //{
            //    GameObject Luci = Editor.EditorScene.NewGameObject("Luci");
            //    SpriteRenderer _2DSpriteRenderer = Luci.AddComponent<SpriteRenderer>();
            //    _2DSpriteRenderer.Texture = new Texture("Engine/EngineContent/models/textures/luci.png");
            //}

            //FontTex = FontLoader.LoadFace("Elemental/Assets/Fonts/OpenSans.ttf");
        }

        public void SetContext(EditorLayer editor)
        {
            Editor = editor;
        }

        public override void OnGUIRender()
        {

            if (Editor.EditorScene.GetSceneState() != Scene.SceneState.Play)
            {
                DrawGuizmos();
            }


            // Play Puase Menu

            //ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 0, 0, 1));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
            ImGui.Begin($"{FontAwesome.ForkAwesome.FirstOrder} Game View", ImGuiWindowFlags.NoBackground);
            DevoidGUI.Image((IntPtr)ViewportTexture, new Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 32));
            HandleDragDrop();


            ImGui.SetItemAllowOverlap();
            DrawViewportTools();


            if (prev_h != DevoidGUI.GetWindowHeight() || prev_w != DevoidGUI.GetWindowWidth())
            {
                Renderer2D.Resize((int)ImGui.GetWindowWidth(), (int)ImGui.GetWindowHeight());
                editorCamera.SetViewportSize((int)ImGui.GetWindowWidth(), (int)ImGui.GetWindowHeight());
                Editor.EditorScene.OnResize((int)ImGui.GetWindowWidth(), (int)ImGui.GetWindowHeight());
                
                prev_w = DevoidGUI.GetWindowWidth();
                prev_h = DevoidGUI.GetWindowHeight();
            }

            if (Editor.EditorScene.GetSceneState() != Scene.SceneState.Play)
            {
                editorCamera.IsSelected = ImGui.IsWindowFocused();
            }

            if (ImGui.BeginDragDropTarget())
            {

            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {

            }

            ImGui.End();

            ImGui.PopStyleVar();

            ImGui.Begin($"{FontAwesome.ForkAwesome.LineChart} Engine Details");

            if (ImGui.CollapsingHeader("Engine Performance"))
            {
                ImGui.TreePush();
                ImGui.Text("FrameTime: " + deltaTime + " s");
                ImGui.Text("Draw Time: " + (Renderer3D.PerformanceData.DRAW_BUFFER_END - Renderer3D.PerformanceData.DRAW_BUFFER_START) + " ms");
                ImGui.Text("PostProcess Time: " + (Renderer3D.PerformanceData.POSTPROCESS_END - Renderer3D.PerformanceData.POSTPROCESS_START) + " ms");
                ImGui.Text("Mesh Count: " + Mesh.TotalMeshCount);
                ImGui.Text("DrawCalls: " + Renderer3D.GetRendererData().DrawCalls);
                ImGui.TreePop();
            }

            ImGui.End();

            ImGui.Begin($"{FontAwesome.ForkAwesome.Book} Scene Details");

            DevoidGUI.DrawTextField("GameObjects", Editor.EditorScene.GetSceneRegistry().GetGameObjectCount().ToString());
            if (DevoidGUI.DrawButtonField("ReCompile All Shaders", "ReCompile"))
            {
                for (int i = 0; i < Shader.Shaders.Count; i++)
                {
                    Shader.Shaders[i].ReCompile();
                }
            }

            ImGui.End();

            ImGui.Begin("PostProcess Profile");

            if (ImGui.CollapsingHeader("PostProcess Effects"))
            {
                ImGui.TreePush();
                if (ImGui.CollapsingHeader("Bloom"))
                {
                    ImGui.TreePush();
                    DevoidGUI.DrawCheckboxField("Enabled", ref bloomSettings.enabled);
                    DevoidGUI.DrawFloatField("Intensity", ref bloomSettings.BloomIntensity);
                    DevoidGUI.DrawFloatField("Exposure", ref bloomSettings.CameraIntensity);
                    DevoidGUI.DrawFloatField("FilterRadius", ref bloomSettings.FilterRadius);
                    ImGui.TreePop();

                    Renderer3D.SetBloomSettings(bloomSettings);
                }
                ImGui.TreePop();
            }

            ImGui.End();

            ImGui.Begin("EditorNode");

            ImGui.End();

        }

        string GetSceneActionIcon()
        {
            return Editor.EditorScene.GetSceneState() == Scene.SceneState.EditorPlay ? FontAwesome.ForkAwesome.Play : FontAwesome.ForkAwesome.Pause;
        }

        public void DrawViewportTools()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            float size = 22;

            ImGui.SetCursorPosX(size);
            ImGui.SetCursorPosY(size + 22);

            if (DevoidGUI.Button(FontAwesome.ForkAwesome.Camera, new Vector2(size + 10, size)))
            {
                editorCamera.projectionType = editorCamera.projectionType == 0 ? 1 : 0;
                editorCamera.UpdateProjection();
            }

            ImGui.SetCursorPosX((ImGui.GetWindowContentRegionMax().X * 0.5f) - (size * 0.5f));
            ImGui.SetCursorPosY(size + 22);
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(.21f, .21f, .21f, 0.7f));
            if (DevoidGUI.Button(GetSceneActionIcon(), new Vector2(size + 10, size)))//ImGui.ImageButton(GetSceneActionIcon(), new System.Numerics.Vector2(size, size)))
            {
                if (Editor.EditorScene.GetSceneState() == Scene.SceneState.Play)
                {
                    Editor.EditorScene.SetSceneState(Scene.SceneState.EditorPlay);
                    Renderer3D.SetCamera(editorCamera.Camera);
                    editorCamera.IsSelected = true;
                }
                else
                {
                    editorCamera.IsSelected = false;
                    Editor.EditorScene.SetSceneState(Scene.SceneState.Play);
                }
            }

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);
        }

        void DrawGuizmos()
        {
            Guizmo3D.Begin(editorCamera);

            //Guizmo3D.DrawGrid();

            GameObject[] gameObjects = Editor.EditorScene.GetSceneRegistry().GetAllGameObjects();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                Component[] components = gameObjects[i].GetAllComponents();
                for (int x = 0; x < components.Length; x++)
                {
                    if (components[x].GetType() == typeof(LightComponent))
                    {
                        Guizmo3D.DrawGuizmo(LightComponentTex, gameObjects[i].transform.position);
                    }
                    if (components[x].GetType() == typeof(CameraComponent))
                    {
                        Guizmo3D.DrawGuizmo(CameraComponentTex, gameObjects[i].transform.position);
                    }
                }
            }

            Guizmo3D.End();
        }

        void HandleDragDrop()
        {
            if (ImGui.BeginDragDropTarget())
            {
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    ProcessDragDrop(Editor.DragDropService.GetDragFile());
                }
                ImGui.EndDragDropTarget();
            }
        }

        void ProcessDragDrop(DragFileItem item)
        {
            switch (item.fileextension)
            {
                case ".fbx":
                    HandleDropMeshFile(item.path);
                    break;
                case ".glb":
                    HandleDropMeshFile(item.path);
                    break;
                case ".gltf":
                    HandleDropMeshFile(item.path);
                    break;
            }
        }

        void HandleDropMeshFile(string path)
        {
            Mesh[] meshes = ModelImporter.LoadModel(path);
            if (meshes == null ||meshes.Length == 0) { return; }
            GameObject DropObject = Editor.EditorScene.NewGameObject(meshes[0].name);
            MeshHolder MeshHolder = DropObject.AddComponent<MeshHolder>();
            MeshHolder.AddMeshes(meshes);
            DropObject.AddComponent<MeshRenderer>();
        }

        float deltaTime;

        public override void OnUpdate(float dt)
        {
            this.deltaTime = dt;
            //Console.WriteLine("FPS: " + 1 / dt);
            editorCamera.Update(dt);

        }

        Vector2 ROT = Vector2.Zero;

        public override void OnRender() {

            //viewportGrid.Render(editorCamera.Camera);
        }

        public EditorCamera GetEditorCamera()
        {
            return editorCamera;
        }
    }
}
