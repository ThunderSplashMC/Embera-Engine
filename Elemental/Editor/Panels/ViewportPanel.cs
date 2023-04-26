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
        private ColliderOutlinePass ColliderOutlinePass;
        private GizmoPass GizmoPass;

        private EditorCamera editorCamera;
        public int ViewportTexture;
        private float prev_h, prev_w;
        //private PostEffectSettings.BloomSettings bloomSettings;

        private Texture SampleTex;
        private Texture LightComponentTex;
        private Texture CameraComponentTex;


        public override void OnInit()
        {
            ViewportTexture = /*((ScreenspaceReflections)RenderGraph.SSRPass).framebuffer.GetColorAttachment(0);*/RenderGraph.CompositeBuffer.GetColorAttachment(0);
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
            ColliderOutlinePass = new ColliderOutlinePass();
            GizmoPass = new GizmoPass();

            ColliderOutlinePass.Editor = Editor;

            Renderer3D.AddRenderPass(EditorRendererPass);
            Renderer3D.AddRenderPass(EditorOutlinePass);
            Renderer3D.AddRenderPass(VoxelTracer);
            Renderer3D.AddRenderPass(ColliderOutlinePass);
            Renderer3D.AddRenderPass(GizmoPass);
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


            // Play Pause Menu

            //ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 0, 0, 1));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
            ImGui.Begin($"{MaterialIconFont.MaterialDesign.Landscape} Game View", ImGuiWindowFlags.NoBackground);
            DevoidGUI.Image((IntPtr)ViewportTexture, new Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 32));
            SetViewportInputLocalCoord();
            HandleObjectSelect();
            HandleDragDrop();


            ImGui.SetItemAllowOverlap();
            DrawViewportTools();


            if (prev_h != DevoidGUI.GetWindowHeight() || prev_w != DevoidGUI.GetWindowWidth())
            {
                Renderer2D.ResizeOrtho((int)ImGui.GetWindowWidth(), (int)ImGui.GetWindowHeight());
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

                using (FileStream fs = File.Create(Editor.CurrentDir + "/DevoidScene.scene"))
                {
                    string dataasstring = Serializer.Serialize(Editor.EditorScene).ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
                    byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                    fs.Write(info, 0, info.Length);
                }

                using (StreamReader reader = new StreamReader(Editor.CurrentDir + "/DevoidScene.scene", Encoding.UTF8))
                {
                    Editor.ChangeScenes(Deserializer.Deserialize(reader.ReadToEnd()));
                }
            }

            if (DevoidGUI.DrawButtonField("Load Assemblies", "Load"))
            {
                ProjectUtils.BuildVSProjectEditor(Editor.PROJECT_DIRECTORY);
                Editor.PROJECT_ASSEMBLY = ProjectUtils.LoadVSProjectEditor(Editor.PROJECT_DIRECTORY);
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

                UI.BeginProperty("Step Multiplier");

                UI.PropertyFloat(ref VoxelTracer.StepMultiplier, 0.1f);

                UI.EndProperty();

                UI.BeginProperty("Normal Ray Offet");

                UI.PropertyFloat(ref VoxelTracer.NormalRayOffset, 0.1f);

                UI.EndProperty();


                UI.BeginProperty("GI Boost");

                UI.PropertyFloat(ref VoxelTracer.GIBoost, 0.1f);

                UI.EndProperty();

                UI.BeginProperty("Min Cone Angle");

                UI.PropertyFloat(ref VoxelTracer.MinConeAngle, 0.1f);

                UI.EndProperty();

                UI.BeginProperty("Max Cone Angle");

                UI.PropertyFloat(ref VoxelTracer.MaxConeAngle, 0.001f, 360, 0.01f);

                UI.EndProperty();

                UI.BeginProperty("MaxSamples");

                UI.PropertyInt(ref VoxelTracer.MaxSamples);

                UI.EndProperty();

                UI.BeginProperty("SampleLOD");

                UI.PropertyFloat(ref VoxelTracer.SampleLOD);

                UI.EndProperty();


                UI.BeginProperty("Max Distance");

                UI.PropertyFloat(ref VoxelTracer.MaxDistance);

                UI.EndProperty();


                UI.BeginProperty("Cone Factor");

                UI.PropertyFloat(ref VoxelTracer.ConeFactor);

                UI.EndProperty();

                UI.BeginProperty("Recompile");

                if (UI.DrawButton("Compile Vis."))
                {
                    VoxelTracer.ConeTracer.ReCompile();
                }

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
        }

        string GetSceneActionIcon()
        {
            return Editor.EditorScene.GetSceneState() == Scene.SceneState.EditorPlay ? FontAwesome.ForkAwesome.Play : FontAwesome.ForkAwesome.Pause;
        }

        void SetViewportInputLocalCoord()
        {
            float rel_x = ((ImGui.GetContentRegionMax().X - (ImGui.GetMousePos().X - ImGui.GetWindowPos().X)) / ImGui.GetContentRegionMax().X);
            float rel_y = ((ImGui.GetContentRegionMax().Y - (ImGui.GetMousePos().Y - (ImGui.GetWindowPos().Y))) / (ImGui.GetContentRegionMax().Y - 32));

            float mouseX = (((int)(rel_x * ImGui.GetContentRegionMax().X)));// / Editor.Application.GetWindowSize().X) * RenderGraph.ViewportWidth;
            float mouseY = (((int)(rel_y * ImGui.GetContentRegionMax().Y)));// / Editor.Application.GetWindowSize().Y) * RenderGraph.ViewportHeight;

            InputSystem.SetMousePosition(new Vector2(mouseX, mouseY));
            if (ImGui.IsItemHovered())
            {
                InputSystem.SetMouseDown(InputSystem.MouseButton.Left, ImGui.IsMouseClicked(ImGuiMouseButton.Left));
                InputSystem.SetMouseDown(InputSystem.MouseButton.Right, ImGui.IsMouseClicked(ImGuiMouseButton.Right));
            } else
            {
                InputSystem.SetMouseDown(InputSystem.MouseButton.Left, false);
                InputSystem.SetMouseDown(InputSystem.MouseButton.Right, false);

            }
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
            if (DevoidGUI.Button(GetSceneActionIcon(), new Vector2(size + 10, size)))
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
                    HandleDropMeshFile(item.fileName);
                    break;
                case ".glb":
                    HandleDropMeshFile(item.fileName);
                    break;
                case ".obj":
                    HandleDropMeshFile(item.fileName);
                    break;
                case ".gltf":
                    HandleDropMeshFile(item.fileName);
                    break;
                case ".dmesh":
                    HandleDropMeshFile(item.fileName);
                    break;
                case ".devoidscene":
                    HandleDropSceneFile(item.path);
                    break;
                case ".scene":
                    HandleDropSceneFile(item.path);
                    break;
                case ".png":
                    HandleDropTextureFile(item.fileName);
                    break;
            }
        }

        void HandleDropMeshFile(string filename)
        {
            Mesh[] meshes = (Mesh[])Resources.Load(filename);
            if (meshes == null || meshes.Length == 0) { return; }
            GameObject DropObject = Editor.EditorScene.NewGameObject(meshes[0].name);
            MeshHolder MeshHolder = DropObject.AddComponent<MeshHolder>();
            MeshHolder.AddMeshes(meshes);
            DropObject.AddComponent<MeshRenderer>();
        }

        void HandleDropTextureFile(string filename)
        {
            Texture texture = (Texture)Resources.Load(filename);
            GameObject DropObject = Editor.EditorScene.NewGameObject(filename);
            SpriteRenderer SpriteRenderer = DropObject.AddComponent<SpriteRenderer>();
            SpriteRenderer.Texture = texture;
            
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
            DrawGuizmos();
            this.deltaTime = dt;
            //Console.WriteLine("FPS: " + 1 / dt);
            editorCamera.Update(dt);

        }

        Vector2 ROT = Vector2.Zero;

        public override void OnRender() 
        {

        }

        public EditorCamera GetEditorCamera()
        {
            return editorCamera;
        }
    }
}
