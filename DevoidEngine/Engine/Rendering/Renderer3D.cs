using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    public struct DrawItem
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Mesh mesh;
        public float distFromCam;
        public object associateObject;
    }

    public class Renderer3D
    {
        static List<DrawItem> DrawList = new List<DrawItem>();
        static List<LightComponent> PointLights = new List<LightComponent>();
        static List<LightComponent> SpotLights = new List<LightComponent>();
        static List<LightComponent> DirectionalLights = new List<LightComponent>();

        static List<Texture> ShadowDepthAttachments = new List<Texture>();

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
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D}
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            };

            RenderGraph.HDRFrameBuffer = new FrameBuffer(frameBufferSpecification);
            RenderGraph.CompositeBuffer = new FrameBuffer(frameBufferSpecification);
            RenderGraph.PreviousFrameBuffer = new FrameBuffer(frameBufferSpecification);

            FrameBufferSpecification miscBufferSpecification = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    },
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    }
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            };

            RenderGraph.LightBuffer = new FrameBuffer(miscBufferSpecification);

            FrameBufferSpecification geometryBufferSpecification = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
    {
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    },
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    },
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    },
                    new ColorAttachment() {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    }
    },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            };

            RenderGraph.GeometryBuffer = new FrameBuffer(geometryBufferSpecification);
        }

        public static void AddRenderPass(RenderPass renderpass)
        {
            RenderPasses.Add(renderpass);
            if (isInitialized)
            {
                renderpass.Initialize(RenderGraph.ViewportWidth, RenderGraph.ViewportHeight);
            }
        }


        public static void Submit(Vector3 position, Vector3 rotation, Vector3 scale, Mesh mesh, object associateObject = null)
        {
            if (mesh == null) { Console.WriteLine("MESH WAS NULL"); }
            DrawList.Add(new DrawItem()
            {
                position = position,
                rotation = rotation,
                scale = scale,
                mesh = mesh,
                distFromCam = Vector3.Distance(RenderGraph.Camera.position, position),
                associateObject = associateObject
            });
        }

        public static void BeginScene(List<LightComponent> Lights)//, ref List<LightComponent> directionalLights = null)
        {
            for (int i = 0; i < Lights.Count; i++)
            {
                if (Lights[i].Lighttype == LightComponent.LightType.PointLight)
                {
                    PointLights.Add(Lights[i]);
                } else
                {
                    SpotLights.Add(Lights[i]);
                }
            }
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
        }

        public static void Render()
        {
            if (RenderGraph.Camera == null) return;

            RenderStart();

            if (RenderGraph.PathTrace)
            {
                PathTracedRenderer.Render(DrawList);
                RenderEnd();
                return;
            }

            SortDrawListByDistance();

            ShadowPass();
            GeometryPass();
            LightingPass();
            BloomPass();
            CompositePass();
            StorePass();

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
            ShadowDepthAttachments.Clear();

            ResetViewport();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            DrawList.Clear();
        }

        static void SkyboxPass()
        {
            if (RenderGraph.SkyboxCubemap == null) return;

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            RendererUtils.SkyboxShader.Use();
            RendererUtils.SkyboxShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(RenderGraph.Camera.position));
            RendererUtils.SkyboxShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            RendererUtils.SkyboxShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            RendererUtils.SkyboxShader.SetFloat("Intensity", RenderGraph.SkyboxIntensity);
            RendererUtils.SkyboxShader.SetInt("skybox", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, RenderGraph.SkyboxCubemap.CubeMapTex);
            RendererUtils.CubeVAO.Render();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        static void LightingPass()
        {
            RenderGraph.LightBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < RenderPasses.Count; i++)
            {
                RenderPasses[i].LightPassEarly();
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, RenderGraph.RenderMode == RenderMode.Normal ? PolygonMode.Fill : PolygonMode.Line);

            SkyboxPass();

            for (int i = 0; i < DrawList.Count; i++)
            {
                DrawList[i].mesh.Material.GetShader().Use();
                ResetLighting(DrawList[i].mesh.Material.GetShader());
                UploadLightingData(DrawList[i].mesh.Material.GetShader());

                UploadCameraData(DrawList[i].mesh.Material.GetShader());
                UploadModelData(DrawList[i].mesh.Material.GetShader(), DrawList[i].position, DrawList[i].rotation, DrawList[i].scale);

                DrawList[i].mesh.Material.Apply();

                int texCount = DrawList[i].mesh.Material.GetAllTexAttributes().Length;

                for (int x = 0; x < RenderGraph.MAX_POINT_SHADOW_BUFFERS; x++)
                {
                    DrawList[i].mesh.Material.GetShader().SetInt("W_SHADOW_BUFFERS[" + x + "]", x + RenderGraph.MAX_PBR_TEXTURE_PROPS);

                    if (x < ShadowDepthAttachments.Count)
                    {
                        GL.BindTextureUnit(x + RenderGraph.MAX_PBR_TEXTURE_PROPS, ShadowDepthAttachments[x].GetTexture());
                    }
                }

                DrawList[i].mesh.Draw();

                RenderGraph.Renderer_3D_DrawCalls += 1;
                Texture.UnbindTexture();
            }

            RenderGraph.LightBuffer.UnBind();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        static void BloomPass()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RenderGraph.BloomRenderer.RenderBloomTexture(RenderGraph.LightBuffer.GetColorAttachment(1));
            ResetViewport();
        }

        static void CompositePass()
        {
            RenderGraph.CompositeBuffer.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RendererUtils.HDRShader.Use();
            RendererUtils.HDRShader.SetInt("S_RENDERED_TEXTURE", 0);
            RendererUtils.HDRShader.SetInt("S_BLOOM_TEXTURE", 1);
            //RendererUtils.HDRShader.SetInt("S_VIGNETTE_TEXTURE", 2);
            RendererUtils.HDRShader.SetInt("S_PREVIOUS_TEXTURE", 2);
            RendererUtils.HDRShader.SetFloat("U_BLOOM_STRENGTH", RenderGraph.BLOOM ? RenderGraph.BloomRenderer.bloomStr : 0);
            RendererUtils.HDRShader.SetFloat("U_VIGNETTE_STRENGTH", 0);
            RendererUtils.HDRShader.SetInt("TonemapMode", (int)RenderGraph.TonemapMode);
            RendererUtils.HDRShader.SetBool("GammaCorrect", RenderGraph.GammeCorrect);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RenderGraph.LightBuffer.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RenderGraph.BloomRenderer.GetBloomTexture());

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, RenderGraph.PreviousFrameBuffer.GetColorAttachment(0));

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            RendererUtils.QuadVAO.Render();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderGraph.CompositeBuffer.UnBind();
        }

        public static void ShadowPass()
        {
            GL.CullFace(CullFaceMode.Front);
            for (int i = 0; i < PointLights.Count; i++)
            {

                LightComponent light = PointLights[i];

                Vector2i Size = light.shadowBufferPointLight.GetSize();
                Vector3 lightPos = light.gameObject.transform.position;
                GL.Viewport(0, 0, Size.X, Size.Y);
                light.shadowBufferPointLight.Bind();
                GL.Clear(ClearBufferMask.DepthBufferBit);

                Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), (float)Size.X / (float)Size.Y, 0.1f, 300f);
                Matrix4[] shadowTransforms = new Matrix4[]
                {
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)) * shadowProj,
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f)) * shadowProj,
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj,
                    Matrix4.LookAt(lightPos, lightPos + new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, -1.0f, 0.0f)) * shadowProj
                };

                RendererUtils.ShadowShader.Use();
                for (int x = 0; x < 6; ++x)
                    RendererUtils.ShadowShader.SetMatrix4("shadowMatrices[" + x + "]", shadowTransforms[x]);

                RendererUtils.ShadowShader.SetFloat("far_plane", 300f);
                RendererUtils.ShadowShader.SetVector3("lightPos", light.gameObject.transform.position);

                for (int y = 0; y < DrawList.Count; y++)
                {
                    UploadModelData(RendererUtils.ShadowShader, DrawList[y].position, DrawList[y].rotation, DrawList[y].scale);
                    DrawList[y].mesh.Draw();

                    RenderGraph.Renderer_3D_DrawCalls += 1;
                }

                light.shadowBufferPointLight.UnBind();
                ResetViewport();
                ShadowDepthAttachments.Add(light.depthTexture);
            }
            GL.CullFace(CullFaceMode.Back);
        }

        public static void GeometryPass()
        {
            RenderGraph.GeometryBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RendererUtils.GeometryShader.Use();

            UploadCameraData(RendererUtils.GeometryShader);


            for (int i = 0; i < DrawList.Count; i++) 
            {
                UploadModelData(RendererUtils.GeometryShader, DrawList[i].position, DrawList[i].rotation, DrawList[i].scale);

                DrawList[i].mesh.Material.SetPropertyVector3(RendererUtils.GeometryShader, "material.albedo");
                DrawList[i].mesh.Material.SetPropertyFloat(RendererUtils.GeometryShader, "material.roughness");
                DrawList[i].mesh.Material.SetPropertyFloat(RendererUtils.GeometryShader, "material.metallic");
                DrawList[i].mesh.Material.SetPropertyInt(RendererUtils.GeometryShader, "USE_TEX_0");
                DrawList[i].mesh.Material.SetPropertyInt(RendererUtils.GeometryShader, "USE_TEX_1");
                DrawList[i].mesh.Material.SetPropertyInt(RendererUtils.GeometryShader, "USE_TEX_2");
                DrawList[i].mesh.Material.SetPropertyInt(RendererUtils.GeometryShader, "USE_TEX_3");
                DrawList[i].mesh.Material.SetPropertyInt(RendererUtils.GeometryShader, "USE_TEX_4");
                DrawList[i].mesh.Material.SetPropertyTexture(RendererUtils.GeometryShader, "material.ROUGHNESS_TEX", 0);
                DrawList[i].mesh.Material.SetPropertyTexture(RendererUtils.GeometryShader, "material.ALBEDO_TEX", 1);

                DrawList[i].mesh.Draw();

                RenderGraph.Renderer_3D_DrawCalls += 1;
            }

            RenderGraph.GeometryBuffer.UnBind();
        }

        public static void StorePass()
        {
            GL.BlitNamedFramebuffer(RenderGraph.CompositeBuffer.GetRendererID(), RenderGraph.PreviousFrameBuffer.GetRendererID(), 0, 0, RenderGraph.CompositeBuffer.GetSize().X, RenderGraph.CompositeBuffer.GetSize().Y, 0, 0, RenderGraph.PreviousFrameBuffer.GetSize().X, RenderGraph.PreviousFrameBuffer.GetSize().Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
        }

        public static void UploadCameraData(Shader shader)
        {
            shader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            shader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            shader.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);
        }

        public static void UploadModelData(Shader shader, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Matrix4 MODELMATRIX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            MODELMATRIX *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            MODELMATRIX *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            MODELMATRIX *= Matrix4.CreateScale(scale);
            MODELMATRIX *= Matrix4.CreateTranslation(position);

            shader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
        }

        public static void ResetLighting(Shader shader)
        {
            for (int i = 0; i < RenderGraph.MAX_POINT_LIGHTS; i++)
            {
                shader.SetFloat("L_POINTLIGHTS[" + i + "].intensity", 0f);
                shader.SetBool("L_POINTLIGHTS[" + i + "].shadows", false);
            }
            for (int i = 0; i < RenderGraph.MAX_SPOT_LIGHTS; i++)
            {
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].intensity", 0f);
            }
        }

        public static void UploadLightingData(Shader shader)
        {
            for (int i = 0; i < PointLights.Count; i++)
            {
                LightComponent light = PointLights[i];
                shader.SetFloat("L_POINTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                shader.SetFloat("L_POINTLIGHTS[" + i + "].constant", light.Attenuation);
                shader.SetVector3("L_POINTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                shader.SetVector3("L_POINTLIGHTS[" + i + "].position", light.gameObject.transform.position);
                shader.SetInt("L_POINTLIGHTS[" + i + "].shadows", light.CanEmitShadows ? 1 : 0);
            }

            for (int i = 0; i < SpotLights.Count; i++)
            {
                LightComponent light = SpotLights[i];
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].intensity", (float)light.Intensity);
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].diffuse", new Vector3(light.color.R, light.color.G, light.color.B));
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].position", light.gameObject.transform.position);
                shader.SetVector3("L_SPOTLIGHTS[" + i + "].direction", light.direction);
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].cutOff", light.cutOff);
                shader.SetFloat("L_SPOTLIGHTS[" + i + "].outerCutOff", light.OuterCutOff);
            }
        }

        static void SortDrawListByDistance()
        {
            DrawList.Sort((x, y) => x.distFromCam.CompareTo(y.distFromCam));
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

            for (int i = 0; i < RenderPasses.Count; i++)
            {
                RenderPasses[i].Resize(width, height);
            }
        }

        public static int GetPassCount()
        {
            return RenderPasses.Count;
        }

        public static List<LightComponent> GetPointLights()
        {
            return PointLights;
        }

        public static List<LightComponent> GetSpotLights()
        {
            return SpotLights;
        }

        public static List<DrawItem> GetRenderDrawList()
        {
            return DrawList;
        }
    }
}
