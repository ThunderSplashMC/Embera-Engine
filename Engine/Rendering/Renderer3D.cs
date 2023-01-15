using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    public enum PolygonModes
    {
        Fill,
        Wireframe
    }

    struct RendererData
    {
        public int ViewportHeight, ViewportWidth;

        public List<LightComponent> PointLights;
        public List<LightComponent> SpotLights;

        public Skybox skybox;

        public int DrawCalls;

        public PolygonMode PolygonMode;
        public int OutputTexture; // 0 = Standalone Sandboxed Game / 1 = Editor Output

        public HDRFrameBuffer HDRFrameBuffer;
        public Bloom BloomRenderer;
        public Vignette Vignette;
        public FrameBuffer RenderPass;
        public GBuffer GBuffer;
        public FrameBuffer AmbientOcculsionFB;

    }

    struct PerformanceData
    {
        public float SUBMIT_START, SUBMIT_END;
        public float DRAW_BUFFER_START, DRAW_BUFFER_END;
        public float POSTPROCESS_START, POSTPROCESS_END;
    }

    public class PostEffectSettings
    {
        public struct BloomSettings {
            public bool enabled;
            public float FilterRadius;
            public float BloomIntensity;
            public float CameraIntensity;
        }

        public struct AmbientOcculsionSettings
        {
            public bool enabled;
        }

        public struct GBufferSettings
        {
            public bool enabled;
        }

        public BloomSettings bloomSettings = new();
        public GBufferSettings gBufferSettings = new();
        public AmbientOcculsionSettings ambientOcculsionSettings = new();

        public PostEffectSettings()
        {
            bloomSettings.BloomIntensity = 0.19f;
            bloomSettings.FilterRadius = 0f;
            bloomSettings.CameraIntensity = 1.0f;
        }
    }


    class Renderer3D
    {
        public struct DrawItem
        {
            public Mesh Mesh; // Currently the mesh stores the material, however this needs to be seperated soon
            public Vector3 position;
            public Vector3 rotation;
            public Vector3 scale;
            public float distanceFromCamera;

            // Editor

            public int EntityID;
        }

        static List<DrawItem> DrawList = new List<DrawItem>();
        static RendererData RendererData = new RendererData();
        public static PerformanceData PerformanceData = new PerformanceData();
        static PostEffectSettings postEffectSettings = new PostEffectSettings();

        static Shader hdr_Shader;
        static Shader ambientOcculsion_Shader;
        static Mesh Quad;

        static Texture ambientOcculsionNoise;
        static List<Vector3> ssaoKernel = new List<Vector3>();

        public static void Submit(Mesh mesh, Vector3 pos, Vector3 rot, Vector3 scale, int ID = 0)
        {
            DrawItem DrawItem = new DrawItem();

            DrawItem.Mesh = mesh;
            DrawItem.position = pos;
            DrawItem.rotation = rot;
            DrawItem.scale = scale;
            DrawItem.distanceFromCamera = Vector3.Distance(RenderGraph.Camera.position, pos);
            DrawItem.EntityID = ID;

            DrawList.Add(DrawItem);
        }

        public static void SetPolygonMode(PolygonModes mode)
        {
            if (mode == PolygonModes.Fill) { RendererData.PolygonMode = PolygonMode.Fill; }
            if (mode == PolygonModes.Wireframe) { RendererData.PolygonMode = PolygonMode.Line; }
        }

        public static void Init(int width, int height)
        {
            postEffectSettings.bloomSettings.enabled = false;
            postEffectSettings.gBufferSettings.enabled = true;
            postEffectSettings.ambientOcculsionSettings.enabled = true;

            RendererData.PolygonMode = PolygonMode.Fill;

            RendererData.skybox = new Skybox();
            RendererData.skybox.Init();

            RendererData.ViewportWidth = width;
            RendererData.ViewportHeight = height;

            RendererData.PointLights = new List<LightComponent>();
            RendererData.SpotLights = new List<LightComponent>();

            hdr_Shader = new Shader("Engine/EngineContent/shaders/hdr");
            ambientOcculsion_Shader = new Shader("Engine/EngineContent/shaders/ambient_occulsion");

            Random rand = new Random();

            
            for (int i = 0; i < 64; ++i)
            {
                Vector3 sample = new Vector3((float)rand.NextDouble()* 2 - 1, (float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble());
                sample.Normalize();
                sample *= (float)rand.NextDouble();
                float scale = (float)i/ 64.0f;

            // scale samples s.t. they're more aligned to center of kernel
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                sample *= scale;
                ssaoKernel.Add(sample);
            }

            ambientOcculsion_Shader.Use();
            for (int i = 0; i < 64; ++i)
                ambientOcculsion_Shader.SetVector3("samples[" + i + "]", ssaoKernel[i]);

            ambientOcculsionNoise = new Texture("Engine/EngineContent/textures/ambient_occulsion_noise.png");

            Quad = new Mesh();
            Quad.SetVertices(VERTEX_DEFAULTS.GetFrameBufferVertices());

            InitRenderFBs(width, height);
            Resize(width, height);
        }

        public static void BeginScene(ref List<LightComponent> lights)
        {
            RendererData.PointLights.Clear();
            RendererData.SpotLights.Clear();
            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i].Lighttype == LightComponent.LightType.PointLight)
                {
                    RendererData.PointLights.Add(lights[i]);
                }
                if (lights[i].Lighttype == LightComponent.LightType.SpotLight)
                {
                    RendererData.SpotLights.Add(lights[i]);
                }
            }
        }

        public static void SetCamera(Camera camera)
        {
            RenderGraph.Camera = camera;
        }

        static void Render_Start()
        {
            RendererData.DrawCalls = 0;

            GL.ClearColor(RenderGraph.Camera.GetClearColor().X, RenderGraph.Camera.GetClearColor().Y, RenderGraph.Camera.GetClearColor().Z, 1);

            GL.PolygonMode(MaterialFace.FrontAndBack, RenderGraph.RenderMode == RenderMode.Normal ? PolygonMode.Fill : PolygonMode.Line);
            GL.FrontFace(FrontFaceDirection.Cw);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Dither);
            GL.Enable(EnableCap.Blend);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        static void Render_End()
        {
            RendererData.SpotLights.Clear();
            RendererData.PointLights.Clear();

            Reset_Viewport();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        static void Reset_Viewport()
        {
            GL.Viewport(0, 0, RendererData.ViewportWidth, RendererData.ViewportHeight);
        }

        static void Reset_Blending()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void Resize(int width, int height)
        {
            RendererData.ViewportWidth = width;
            RendererData.ViewportHeight = height;
            RendererData.Vignette.Resize(width, height);
            RendererData.HDRFrameBuffer.Resize(width, height);
            RendererData.BloomRenderer.Resize(width, height);
            RendererData.RenderPass.Resize(width, height);
            GL.Viewport(0, 0, width, height);
        }

        public static void Render()
        {
            if (!CheckCameraSet()) { return; }

            PERF_START("DRAW_BUFFER_START");
            
            Render_Start();

            SortDrawListByDistance();

            LightingPass();
            DoPostProcess();
            CompositePass();

            // Finish Rendering
            Render_End();

            // Clearing the draw list for next frame
            ClearDrawList();

            PERF_END("DRAW_BUFFER_END");

            // Check for errors
            CheckError();
        }

        public static ref Skybox GetSkybox()
        {
            return ref RendererData.skybox;
        }

        public static void BlitToScreen()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.BlitNamedFramebuffer(RenderGraph.CompositePass.GetRendererID(), 0, 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }

        static void RenderSkybox()
        {
            RendererData.skybox.Render(RenderGraph.Camera);
        }

        static void DoPostProcess()
        {
            if (postEffectSettings.bloomSettings.enabled)
            {
                BloomPass();
            }
            //VignettePass();

            // Render Meshes into GBuffer

            if (postEffectSettings.gBufferSettings.enabled)
                DrawItemsGBuffer(DrawList);

            if (postEffectSettings.ambientOcculsionSettings.enabled)
                AmbientOcculsionPass();

        }

        static void AmbientOcculsionPass()
        {
            RendererData.AmbientOcculsionFB.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            ambientOcculsion_Shader.Use();
            ambientOcculsion_Shader.SetInt("gPosition", 0);
            ambientOcculsion_Shader.SetInt("gNormal", 1);
            ambientOcculsion_Shader.SetInt("texNoise", 2);

            for (int i = 0; i < 64; ++i)
                ambientOcculsion_Shader.SetVector3("samples[" + i + "]", ssaoKernel[i]);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RendererData.GBuffer.GetColorAttachment(0));
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RendererData.GBuffer.GetColorAttachment(1));
            ambientOcculsionNoise.SetActiveUnit(TextureActiveUnit.UNIT2);

            Quad.Draw();

            RendererData.AmbientOcculsionFB.UnBind();
        }

        static void LightingPass()
        {
            RendererData.RenderPass.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RenderSkybox();
            SortDrawListByDistance();
            DrawItems(DrawList);
            RendererData.RenderPass.UnBind();
        }

        static void BloomPass()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RendererData.BloomRenderer.RenderBloomTexture(RendererData.RenderPass.GetColorAttachment(0), postEffectSettings.bloomSettings.FilterRadius);
            Reset_Viewport();
        }

        static void VignettePass()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RendererData.Vignette.DoPass(RendererData.RenderPass.GetColorAttachment(0));
        }

        static void CompositePass()
        {
            RenderGraph.CompositePass.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            hdr_Shader.Use();
            hdr_Shader.SetInt("S_RENDERED_TEXTURE", 0);
            hdr_Shader.SetInt("S_BLOOM_TEXTURE", 1);
            hdr_Shader.SetInt("S_VIGNETTE_TEXTURE", 2);
            hdr_Shader.SetFloat("U_BLOOM_STRENGTH", postEffectSettings.bloomSettings.enabled ? postEffectSettings.bloomSettings.BloomIntensity : 0);
            hdr_Shader.SetFloat("U_VIGNETTE_STRENGTH", 1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RendererData.RenderPass.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RendererData.BloomRenderer.GetBloomTexture());

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, RendererData.Vignette.GetVignetteTexture());

            Quad.Draw();

            RenderGraph.CompositePass.UnBind();

            Reset_Blending();
        }

        static bool CheckCameraSet()
        {
            if (RenderGraph.Camera == null)
            {
                return false;
            }
            return true;
        }

        
        
        static void SortDrawListByDistance()
        {
            DrawList.Sort((x, y) => x.distanceFromCamera.CompareTo(y.distanceFromCamera));
        }

        public static void DrawItems(List<DrawItem> drawList)
        {
            for (int i = 0; i < drawList.Count; i++)
            {
                Mesh mesh = drawList[i].Mesh;

                Matrix4 MODELMATRIX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(drawList[i].rotation.X));
                MODELMATRIX *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(drawList[i].rotation.Y));
                MODELMATRIX *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(drawList[i].rotation.Z));

                MODELMATRIX *= Matrix4.CreateScale(drawList[i].scale);
                MODELMATRIX *= Matrix4.CreateTranslation(drawList[i].position);
                mesh.Material.Set("W_MODEL_MATRIX", MODELMATRIX);
                mesh.Material.Set("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                mesh.Material.Set("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                mesh.Material.Set("C_VIEWPOS", RenderGraph.Camera.position);

                if (RendererData.skybox.Cubemap != null)
                {
                    mesh.Material.SetTexture("W_SKYBOX", RendererData.skybox.Cubemap.CubeMapTexture);
                }

                // Set Lighting Info (Forward Rendering)
                ResetLighting(mesh);
                SetLighting(mesh);

                if (mesh.Material.GetShader().ShaderProperties.requiresTime)
                {
                    mesh.Material.GetShader().SetFloat("E_TIME", RenderGraph.time);
                }
                mesh.Material.UpdateUniforms();

                mesh.Draw();
                RendererData.DrawCalls += 1;
            }

            
        }

        static void DrawItemsGBuffer(List<DrawItem> drawList)
        {
            RendererData.GBuffer.Bind();
            //GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // keep it black so it doesn't leak into g-buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            for (int i = 0; i < drawList.Count; i++)
            {
                Mesh mesh = drawList[i].Mesh;
                
                Matrix4 MODELMATRIX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(drawList[i].rotation.X));
                MODELMATRIX *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(drawList[i].rotation.Y));
                MODELMATRIX *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(drawList[i].rotation.Z));

                MODELMATRIX *= Matrix4.CreateScale(drawList[i].scale);
                MODELMATRIX *= Matrix4.CreateTranslation(drawList[i].position);
                RendererData.GBuffer.SetupGBufferShader(RenderGraph.Camera.GetProjectionMatrix(), RenderGraph.Camera.GetViewMatrix(), MODELMATRIX, drawList[i].EntityID);
                mesh.Draw();
            }
            UnBindFramebuffer();
        }

        static void ResetLighting(Mesh mesh)
        {
            for (int i = 0; i < 8; i++)
            {
                mesh.Material.Set("L_POINTLIGHTS[" + i + "].intensity", 0f);
            }
            for (int i = 0; i < 8; i++)
            {
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].intensity", 0f);
            }
        }

        static void SetLighting(Mesh mesh)
        {
            for (int i = 0; i < RendererData.PointLights.Count; i++)
            {
                LightComponent light = RendererData.PointLights[i];
                
                mesh.Material.Set("L_POINTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                mesh.Material.Set("L_POINTLIGHTS[" + i + "].constant", 1f);
                mesh.Material.Set("L_POINTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                mesh.Material.Set("L_POINTLIGHTS[" + i + "].position", light.gameObject.transform.position);

            }

            for (int i = 0; i < RendererData.SpotLights.Count; i++)
            {
                LightComponent light = RendererData.SpotLights[i];
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].position", light.gameObject.transform.position);
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].direction", light.direction);
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].cutOff", light.cutOff);
                mesh.Material.Set("L_SPOTLIGHTS[" + i + "].outerCutOff", light.OuterCutOff);
            }
        }

        static void PERF_START(string start)
        {
            if (start == "SUBMIT_START")
            {
                PerformanceData.SUBMIT_START = DateTime.Now.Millisecond;
            }
            else if (start == "DRAW_BUFFER_START")
            {
                PerformanceData.DRAW_BUFFER_START = DateTime.Now.Millisecond;
            }
            else if (start == "POSTPROCESS_START")
            {
                PerformanceData.POSTPROCESS_START = DateTime.Now.Millisecond;
            }
        }

        static void PERF_END(string end)
        {
            if (end == "SUBMIT_END")
            {
                PerformanceData.SUBMIT_END = DateTime.Now.Millisecond;
            }
            else if (end == "DRAW_BUFFER_END")
            {
                PerformanceData.DRAW_BUFFER_END = DateTime.Now.Millisecond;
            }
            else if (end == "POSTPROCESS_END")
            {
                PerformanceData.POSTPROCESS_END = DateTime.Now.Millisecond;
            }
        }

        static void ResetShaderLightingData(Mesh mesh)
        {

        }
        
        static void InitRenderFBs(int width, int height)
        {
            FrameBufferSpecification FBS = new FrameBufferSpecification()
            {
                width = RendererData.ViewportWidth,
                height = RendererData.ViewportHeight,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() { textureFormat = FrameBufferTextureFormat.RGBA16F }
                },
                addDepthBuffer = true
            };
            RendererData.RenderPass = new FrameBuffer(FBS);
            RenderGraph.CompositePass = new FrameBuffer(FBS);

            RendererData.Vignette = new Vignette();
            RendererData.HDRFrameBuffer = new HDRFrameBuffer();
            RendererData.BloomRenderer = new Bloom();
            RendererData.GBuffer = new GBuffer();
            RendererData.AmbientOcculsionFB = new FrameBuffer(new FrameBufferSpecification()
            {
                width = RendererData.ViewportWidth,
                height = RendererData.ViewportHeight,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() { textureFormat = FrameBufferTextureFormat.R32F }
                },
                addDepthBuffer = true
            });

            RendererData.Vignette.Init(width, height);
            RendererData.HDRFrameBuffer.Init(width, height);
            RendererData.BloomRenderer.Init(width, height);
            RendererData.GBuffer.Init(width, height);
        }

        public static RendererData GetRendererData()
        {
            return RendererData;
        }

        static void BindViewportFramebuffer()
        {
            RendererData.RenderPass.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        static void UnBindFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        static void RenderBloom()
        {
            int screenTexture = RendererData.HDRFrameBuffer.HDRFB.GetColorAttachment(0);
            RendererData.BloomRenderer.RenderBloomTexture(screenTexture, postEffectSettings.bloomSettings.FilterRadius);
            RendererData.BloomRenderer.RenderToScreen(screenTexture,postEffectSettings.bloomSettings.CameraIntensity, postEffectSettings.bloomSettings.BloomIntensity);
        }

        public static void GUIRender()
        {

        }

        static void ClearDrawList()
        {
            DrawList.Clear();
        }

        static void CheckError()
        {
            ErrorCode error = GL.GetError();
            if (error == ErrorCode.NoError)
                return;

            Console.WriteLine(error);
        }

        public static PostEffectSettings.BloomSettings GetBloomSettings()
        {
            return postEffectSettings.bloomSettings;
        }

        public static void SetBloomSettings(PostEffectSettings.BloomSettings bloomSettings)
        {
            postEffectSettings.bloomSettings = bloomSettings;
        }

        public static ref PostEffectSettings.GBufferSettings GetGBuffer()
        {
            return ref postEffectSettings.gBufferSettings;
        }

    }
}