using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    struct DrawItem
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Mesh mesh;
    }

    class Renderer3D
    {
        static List<DrawItem> DrawList = new List<DrawItem>();
        static List<LightComponent> PointLights = new List<LightComponent>();
        static List<LightComponent> SpotLights = new List<LightComponent>();
        static List<LightComponent> DirectionalLights = new List<LightComponent>();

        static List<RenderPass> RenderPasses = new List<RenderPass>();

        static bool isInitialized = false;

        public static void Init(int width, int height)
        {
            InitializeRenderFrameBuffers(width, height);

            RenderGraph.BloomRenderer = new Bloom();
            RenderGraph.BloomRenderer.Init(width, height);

            for (int i = 0; i < RenderPasses.Count; i++)
            {
                RenderPasses[i].Initialize(width, height);
            }
            isInitialized = true;
            Resize(width, height);
        }

        public static void InitializeRenderFrameBuffers(int width, int height)
        {
            FrameBufferSpecification frameBufferSpecification = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F}
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            };

            RenderGraph.HDRFrameBuffer = new FrameBuffer(frameBufferSpecification);
            RenderGraph.LightBuffer = new FrameBuffer(frameBufferSpecification);
            RenderGraph.CompositeBuffer = new FrameBuffer(frameBufferSpecification);
            RenderGraph.GeometryBuffer = new FrameBuffer(frameBufferSpecification);
        }

        public static void AddRenderPass(RenderPass renderpass)
        {
            RenderPasses.Add(renderpass);
        }


        public static void Submit(Vector3 position, Vector3 rotation, Vector3 scale, Mesh mesh)
        {
            DrawList.Add(new DrawItem()
            {
                position = position,
                rotation = rotation,
                scale = scale,
                mesh = mesh
            });
        }

        public static void BeginScene(ref List<LightComponent> pointLights, ref List<LightComponent> spotLights)//, ref List<LightComponent> directionalLights = null)
        {
            PointLights = pointLights;
            SpotLights = spotLights;
        }

        public static void RenderStart()
        {
            RenderGraph.Renderer_3D_DrawCalls = 0;

            GL.ClearColor(RenderGraph.Camera.GetClearColor().X, RenderGraph.Camera.GetClearColor().Y, RenderGraph.Camera.GetClearColor().Z, 1);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Dither);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, RenderGraph.RenderMode == RenderMode.Normal ? PolygonMode.Fill : PolygonMode.Line);
        }

        public static void Render()
        {
            if (RenderGraph.Camera == null) return;

            RenderStart();

            LightingPass();

            for (int i = 0; i < RenderPasses.Count; i++)
            {
                RenderPasses[i].DoRenderPass();
            }

            RenderEnd();
        }

        public static void RenderEnd()
        {
            PointLights.Clear();
            SpotLights.Clear();
            DirectionalLights.Clear();

            ResetViewport();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            DrawList.Clear();
        }

        static void LightingPass()
        {
            RenderGraph.LightBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (RenderGraph.SkyboxCubemap != null) {

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                RendererUtils.SkyboxShader.Use();
                RendererUtils.SkyboxShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(RenderGraph.Camera.position));
                RendererUtils.SkyboxShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                RendererUtils.SkyboxShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                RendererUtils.SkyboxShader.SetInt("skybox", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, RenderGraph.SkyboxCubemap.CubeMapTex);
                RendererUtils.CubeVAO.Render();
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);

            }

            Draw();

            RenderGraph.LightBuffer.UnBind();
        }

        public static void Draw()
        {
            DrawModels(DrawList);
        }

        public static void DrawModels(List<DrawItem> drawList)
        {
            for (int i = 0; i < drawList.Count; i++)
            {
                Mesh mesh = drawList[i].mesh;

                //

                Matrix4 MODELMATRIX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(drawList[i].rotation.X));
                MODELMATRIX *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(drawList[i].rotation.Y));
                MODELMATRIX *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(drawList[i].rotation.Z));
                MODELMATRIX *= Matrix4.CreateScale(drawList[i].scale);
                MODELMATRIX *= Matrix4.CreateTranslation(drawList[i].position);

                //

                mesh.Material.Set("W_MODEL_MATRIX", MODELMATRIX);
                mesh.Material.Set("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                mesh.Material.Set("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                mesh.Material.Set("C_VIEWPOS", RenderGraph.Camera.position);

                //

                mesh.Draw();
                
                //

                RenderGraph.Renderer_3D_DrawCalls += 1;
            }

        }

        public static void UploadLightingData(Shader shader)
        {
            for (int i = 0; i < RenderGraph.MAX_POINT_LIGHTS; i++)
            {
                if (RenderGraph.MAX_POINT_LIGHTS > PointLights.Count)
                {
                    shader.SetFloat("L_POINTLIGHTS[" + i + "].intensity", 0f);
                    continue;
                }
                LightComponent light = PointLights[i];

                shader.SetFloat("L_POINTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                shader.SetFloat("L_POINTLIGHTS[" + i + "].constant", 1f);
                shader.SetVector3("L_POINTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                shader.SetVector3("L_POINTLIGHTS[" + i + "].position", light.gameObject.transform.position);
                shader.SetBool("L_POINTLIGHTS[" + i + "].shadows", light.CanEmitShadows);

            }

            for (int i = 0; i < RenderGraph.MAX_SPOT_LIGHTS; i++)
            {
                if (RenderGraph.MAX_POINT_LIGHTS > PointLights.Count)
                {
                    shader.SetFloat("L_POINTLIGHTS[" + i + "].intensity", 0f);
                    continue;
                }
                LightComponent light = SpotLights[i];
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].position", light.gameObject.transform.position);
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].direction", light.direction);
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].cutOff", light.cutOff);
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].outerCutOff", light.OuterCutOff);
            }
        }

        public static void ResetViewport()
        {
            GL.Viewport(0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight);
        }

        public static void Resize(int width, int height)
        {
            if (isInitialized == false) { Console.WriteLine("Renderer3D has not been initialized yet!\nCall Renderer3D.Init()"); return; }
            RenderGraph.LightBuffer.Resize(width, height);
            RenderGraph.CompositeBuffer.Resize(width, height);
            RenderGraph.HDRFrameBuffer.Resize(width, height);
            RenderGraph.BloomRenderer.Resize(width, height);
            RenderGraph.GeometryBuffer.Resize(width, height);
        }

        public static List<LightComponent> GetPointLights()
        {
            return PointLights;
        }

        public static List<LightComponent> GetSpotLights()
        {
            return SpotLights;
        }

    }
}
