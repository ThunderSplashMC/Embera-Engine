using System;
using System.Collections.Generic;
using DevoidEngine.Engine.GUI;
using OpenTK.Mathematics;
using ImGuiNET;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using Elemental.Editor.EditorUtils;
using DevoidEngine.Engine.Components;
using System.IO;
using System.Text;
using DevoidEngine.Engine.Serializing;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Elemental.Editor.Panels
{
    class ViewportPanel : Panel
    {

        private EditorRendererPass EditorRendererPass;
        private EditorOutlinePass EditorOutlinePass;
        private VoxelTracer VoxelTracer;

        private EditorCamera editorCamera;
        public int ViewportTexture;
        private float prev_h, prev_w;
        //private PostEffectSettings.BloomSettings bloomSettings;

        private Texture SampleTex;
        private Texture LightComponentTex;
        private Texture CameraComponentTex;


        public override void OnInit()
        {
            ViewportTexture = RenderGraph.CompositeBuffer.GetColorAttachment(0);
            editorCamera = new EditorCamera(MathHelper.DegreesToRadians(45.0f), (int)Editor.Application.GetWindowSize().X, (int)Editor.Application.GetWindowSize().Y, 1000f, 0.1f);
            Renderer.SetCamera(editorCamera.Camera);

            Guizmo3D.Init((int)Editor.Application.GetWindowSize().X, (int)Editor.Application.GetWindowSize().Y, RenderGraph.CompositeBuffer);

            SampleTex = new Texture("Editor/Assets/folder-icn.png");
            SampleTex.ChangeFilterType(FilterTypes.Nearest);
            LightComponentTex = new Texture("Editor/Assets/LightComponentTexture.png");


            LightComponentTex.ChangeFilterType(FilterTypes.Linear);
            CameraComponentTex = new Texture("Editor/Assets/CameraComponentTexture.png");
            CameraComponentTex.ChangeFilterType(FilterTypes.Linear);

            EditorRendererPass = new EditorRendererPass();
            EditorOutlinePass = new EditorOutlinePass();
            VoxelTracer = new VoxelTracer();

            Renderer3D.AddRenderPass(EditorRendererPass);
            Renderer3D.AddRenderPass(EditorOutlinePass);
            Renderer3D.AddRenderPass(VoxelTracer);
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
            ImGui.Begin($"{MaterialIconFont.MaterialDesign.Landscape} Game View", ImGuiWindowFlags.NoBackground);
            DevoidGUI.Image((IntPtr)ViewportTexture, new Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 32));
            HandleObjectSelect();
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

            ImGui.Begin($"{FontAwesome.ForkAwesome.InfoCircle} Engine Details");

            if (ImGui.CollapsingHeader("Engine Performance"))
            {
                ImGui.TreePush();
                ImGui.Text("FPS: " + 1 / deltaTime);
                ImGui.Text("FrameTime: " + deltaTime + " s");
                ImGui.Text("Mesh Count: " + Mesh.TotalMeshCount);
                ImGui.Text("DrawCalls: " + RenderGraph.Renderer_3D_DrawCalls);
                ImGui.Text("Render Passes: " + Renderer3D.GetPassCount());
                ImGui.TreePop();
            }

            ImGui.End();

            ImGui.Begin($"{FontAwesome.ForkAwesome.InfoCircle} Scene Details");

            DevoidGUI.DrawTextField("GameObjects", Editor.EditorScene.GetSceneRegistry().GetGameObjectCount().ToString());
            if (DevoidGUI.DrawButtonField("ReCompile All Shaders", "ReCompile"))
            {

                Console.WriteLine(Shader.Shaders.Count);
                for (int i = 0; i < Shader.Shaders.Count; i++)
                {
                    Shader.Shaders[i].ReCompile();
                }
            }

            if (DevoidGUI.DrawButtonField("Serialize Scene", "Serialize"))
            {

                using (FileStream fs = File.Create(Editor.Application.GetWorkingDirectory() + "/DevoidScene.scene"))
                {
                    string dataasstring = Serializer.Serialize(Editor.EditorScene).ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
                    byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                    fs.Write(info, 0, info.Length);
                }

                using (StreamReader reader = new StreamReader(Editor.Application.GetWorkingDirectory() + "/DevoidScene.scene", Encoding.UTF8))
                {
                    Editor.ChangeScenes(Deserializer.Deserialize(reader.ReadToEnd()));
                }
            }

            ImGui.End();

            ImGui.Begin("Engine/Editor Settings");

            if (ImGui.CollapsingHeader("EditorCamera"))
            {
                ImGui.TreePush();
                float fov = editorCamera.Fov;
                DevoidGUI.DrawFloatField("FieldOfView", ref fov, 0.5f, 30, 90);
                DevoidGUI.DrawColor3Control("Clear Color", ref editorCamera.Camera.ClearColor);
                editorCamera.Fov = fov;

                ImGui.TreePop();
            }
            if (ImGui.CollapsingHeader("EditorOutline"))
            {
                ImGui.TreePush();
                DevoidGUI.DrawFloatField("Outline Width", ref EditorOutlinePass.outlineWidth, 0.1f);

                ImGui.TreePop();
            }

            if (ImGui.CollapsingHeader("Renderer Settings"))
            {
                ImGui.TreePush();

                UI.BeginPropertyGrid("TONEMAP_SETTINGS");

                UI.BeginProperty("Render Mode");

                FieldInfo field = typeof(RenderGraph).GetField("RenderMode", BindingFlags.Public | BindingFlags.Static);

                UI.DrawEnumField(field, null);

                UI.EndProperty();

                UI.EndPropertyGrid();

                ImGui.TreePop();
            }


            if (ImGui.CollapsingHeader("PostProcess Effects"))
            {
                ImGui.TreePush();
                if (ImGui.CollapsingHeader("Bloom"))
                {
                    ImGui.TreePush();
                    DevoidGUI.DrawCheckboxField("Enabled", ref RenderGraph.BLOOM);
                    DevoidGUI.DrawFloatField("Intensity", ref RenderGraph.BloomRenderer.bloomStr);
                    DevoidGUI.DrawFloatField("Exposure", ref RenderGraph.BloomRenderer.bloomExposure);
                    DevoidGUI.DrawFloatField("FilterRadius", ref RenderGraph.BloomRenderer.filterRadius);
                    ImGui.TreePop();

                    //Renderer3D.SetBloomSettings(bloomSettings);
                }
                ImGui.TreePop();
            }

            if (ImGui.CollapsingHeader("Tonemapper Settings"))
            {
                ImGui.TreePush();

                UI.BeginPropertyGrid("TONEMAP_SETTINGS");

                UI.BeginProperty("Tonemapper Mode");

                FieldInfo field = typeof(RenderGraph).GetField("TonemapMode", BindingFlags.Public | BindingFlags.Static);

                UI.DrawEnumField(field, null);

                UI.EndProperty();

                UI.BeginProperty("Gamma Correction");

                UI.PropertyBool(ref RenderGraph.GammeCorrect);

                UI.EndProperty();

                UI.EndPropertyGrid();

                ImGui.TreePop();
            }

            if (ImGui.CollapsingHeader("VoxelTracer Settings"))
            {
                UI.BeginPropertyGrid("VXGI_SETTINGS");

                UI.BeginProperty("VXGI Toggle");

                UI.PropertyBool(ref VoxelTracer.Enabled);

                UI.EndProperty();

                UI.BeginProperty("VXGI Debug View");

                UI.PropertyBool(ref VoxelTracer.Debug);

                UI.EndProperty();

                UI.BeginProperty("VXGI Debug View Opacity");

                UI.PropertyFloat(ref VoxelTracer.DebugViewOpacity, 0, 1, 0.1f);

                UI.EndProperty();

                UI.BeginProperty("SkyColor");

                UI.PropertyColor4(ref VoxelTracer.SkyColor);

                UI.EndProperty();

                UI.BeginProperty("GridMin");

                UI.PropertyVector3(ref VoxelTracer.GridMin, 0.2f, float.MinValue, -0.1f);

                UI.EndProperty();

                UI.BeginProperty("GridMax");

                UI.PropertyVector3(ref VoxelTracer.GridMax, 0.2f, 0.1f);

                UI.EndProperty();

                UI.BeginProperty("NearPlane");

                UI.PropertyFloat(ref VoxelTracer.NearPlane);

                UI.EndProperty();

                UI.BeginProperty("FarPlane");

                UI.PropertyFloat(ref VoxelTracer.FarPlane);

                UI.EndProperty();

                UI.BeginProperty("Cone Angle");

                UI.PropertyFloat(ref VoxelTracer.ConeAngle);

                UI.EndProperty();

                UI.BeginProperty("Step Multiplier");

                UI.PropertyFloat(ref VoxelTracer.StepMultiplier, 0.1f);

                UI.EndProperty();

                UI.EndPropertyGrid();
            }

            if (ImGui.CollapsingHeader("PathTracer Settings"))
            {
                UI.BeginPropertyGrid("PT_SETTINGS");

                UI.BeginProperty("PathTracer Toggle");

                UI.PropertyBool(ref RenderGraph.PathTrace);

                UI.EndProperty();


                UI.BeginProperty("Bake Meshes");

                if (UI.DrawButton("Bake"))
                {
                    PathTracedRenderer.BakeMeshes();
                }

                UI.EndProperty();

                UI.EndPropertyGrid();
            }

            ImGui.End();


            //NodeManager.BeginNodeEditor("Node Editor");

            //NodeManager.BeginNode("ash" + 1, "GameObject ", new Vector2(50, 50), new Vector2(300, 300));

            //NodeManager.PropertyText("Position      ", "X: 0 Y: 0 Z: 0", new Vector2(0, 0));
            //NodeManager.PropertyText("Rotation      ", "X: 0 Y: 0 Z: 0", new Vector2(0, 0));
            //NodeManager.PropertyText("Components    ", "List<Component>", new Vector2(0, 0));
            //NodeManager.PropertyText("Current Scene ", "DevoidScene1", new Vector2(0, 0));
            //NodeManager.PropertyText("Object Index  ", "0", new Vector2(0, 0));
            //NodeManager.PropertyText("Object Status ", "Hidden", new Vector2(0, 0));
            //NodeManager.PropertyFloat("SCALE: ", ref val, new Vector2(0, 0));

            //NodeManager.EndNode();

            //NodeManager.BeginNode("ash" + 2, "Value Inspecter ", new Vector2(55, 50), new Vector2(300, 300));
            //NodeManager.PropertyFloat("Value: ", ref val, new Vector2(0, 0));

            //NodeManager.EndNode();

            //NodeManager.EndNodeEditor();



        }
        Vector4 rectSize = new Vector4(40, 40, 300, 350);
        Vector4 clickPoint = Vector4.Zero;
        bool isDragging = false;
        float val = 0f;

        string GetSceneActionIcon()
        {
            return Editor.EditorScene.GetSceneState() == Scene.SceneState.EditorPlay ? FontAwesome.ForkAwesome.Play : FontAwesome.ForkAwesome.Pause;
        }

        public void DrawViewportTools()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            float size = 25;

            ImGui.SetCursorPosX(size);
            ImGui.SetCursorPosY(size + 22);

            if (DevoidGUI.Button(MaterialIconFont.MaterialDesign.Camera, new Vector2(size, size)))
            {
                editorCamera.projectionType = editorCamera.projectionType == 0 ? 1 : 0;
                editorCamera.UpdateProjection();
            }

            ImGui.SetCursorPosX(size * 3);
            ImGui.SetCursorPosY(size + 22);

            if (DevoidGUI.Button(FontAwesome.ForkAwesome.LightbulbO, new Vector2(size, size)))
            {
                RenderGraph.EnableLighting = !RenderGraph.EnableLighting;
            }

            ImGui.SetCursorPosX(size * 5);
            ImGui.SetCursorPosY(size + 22);

            if (DevoidGUI.Button(FontAwesome.ForkAwesome.LightbulbO, new Vector2(size, size)))
            {
                EditorOutlinePass.CurrentOutlinedObjectUUID = 0;
            }

            ImGui.SetCursorPosX((ImGui.GetWindowContentRegionMax().X * 0.5f) - (size * 0.5f));
            ImGui.SetCursorPosY(size + 22);
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(.21f, .21f, .21f, 0.7f));
            if (DevoidGUI.Button(GetSceneActionIcon(), new Vector2(size + 10, size)))//ImGui.ImageButton(GetSceneActionIcon(), new System.Numerics.Vector2(size, size)))
            {
                if (Editor.EditorScene.GetSceneState() == Scene.SceneState.Play)
                {
                    Editor.EditorScene.SetSceneState(Scene.SceneState.EditorPlay);
                    Renderer.SetCamera(editorCamera.Camera);
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
                        Guizmo3D.DrawGuizmo(LightComponentTex, gameObjects[i].transform.position, 4f);
                        if (((GameObjectPanel)Editor.EditorPanels[2]).CurrentSelectedGameObject == components[x].gameObject) Guizmo3D.DrawWireSphere(components[x].gameObject.transform.position, ((LightComponent)(components[x])).Attenuation * 4);

                    }
                    if (components[x].GetType() == typeof(CameraComponent))
                    {
                        Guizmo3D.DrawGuizmo(CameraComponentTex, gameObjects[i].transform.position, 4f);
                    }
                }
            }

            //Guizmo3D.DrawSphere(Vector3.Zero, 2);

            Guizmo3D.End();
        }

        void HandleObjectSelect()
        {
            if (ImGui.IsItemClicked())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    EditorRendererPass.frameBuffer.Bind();

                    float rel_x = ((ImGui.GetContentRegionMax().X - (ImGui.GetMousePos().X - ImGui.GetWindowPos().X)) / ImGui.GetContentRegionMax().X);
                    float rel_y = ((ImGui.GetContentRegionMax().Y - (ImGui.GetMousePos().Y - (ImGui.GetWindowPos().Y))) / ImGui.GetContentRegionMax().Y);

                    float mouseX = (int)(rel_x * Editor.Application.GetWindowSize().X);
                    float mouseY = (int)(rel_y * Editor.Application.GetWindowSize().Y);

                    int count = 0;

                    int[] pixels = new int[1];
                    OpenTK.Graphics.OpenGL.GL.ReadPixels((int)mouseX, (int)mouseY,  1, 1, OpenTK.Graphics.OpenGL.PixelFormat.RedInteger, OpenTK.Graphics.OpenGL.PixelType.Int, pixels);

                    GameObject[] gameObjects = Editor.EditorScene.GetSceneRegistry().GetAllGameObjects();

                    for (int i = 0; i < gameObjects.Length; i++) 
                    { 
                        if (gameObjects[i].ID == pixels[0])
                        {
                            ((GameObjectPanel)Editor.EditorPanels[2]).CurrentSelectedGameObject = gameObjects[i];
                            EditorOutlinePass.CurrentOutlinedObjectUUID = gameObjects[i].ID;
                        }
                    }

                    EditorRendererPass.frameBuffer.UnBind();
                }
            }
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
                case ".obj":
                    HandleDropMeshFile(item.path);
                    break;
                case ".gltf":
                    HandleDropMeshFile(item.path);
                    break;
                case ".dmesh":
                    HandleDropMeshFile(item.path);
                    break;
                case ".devoidscene":
                    HandleDropSceneFile(item.path);
                    break;
                case ".scene":
                    HandleDropSceneFile(item.path);
                    break;
            }
        }

        void HandleDropMeshFile(string path)
        {
            Mesh[] meshes = ModelImporter.AddMaterialsToScene(Editor.EditorScene, ModelImporter.LoadModel(path));
            if (meshes == null || meshes.Length == 0) { return; }
            GameObject DropObject = Editor.EditorScene.NewGameObject(meshes[0].name);
            MeshHolder MeshHolder = DropObject.AddComponent<MeshHolder>();
            MeshHolder.AddMeshes(meshes);
            DropObject.AddComponent<MeshRenderer>();
        }

        void HandleDropSceneFile(string path)
        {
            string sceneData;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                sceneData = reader.ReadToEnd();
            }
            Scene scene = Deserializer.Deserialize(sceneData);
            Editor.ChangeScenes(scene);
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
            //DrawGuizmos();
            //viewportGrid.Render(editorCamera.Camera);
        }

        public EditorCamera GetEditorCamera()
        {
            return editorCamera;
        }
    }
}
