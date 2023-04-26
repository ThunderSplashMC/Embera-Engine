using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using ImGuiNET;

namespace DevoidEngine.Engine.Rendering
{
    public class VoxelTracer : RenderPass
    {


        Shader voxelize_shader = new Shader("Engine/EngineContent/shaders/Voxelize/voxelize.vert", "Engine/EngineContent/shaders/Voxelize/voxelize.frag", "Engine/EngineContent/shaders/Voxelize/voxelize.geom");
        public ComputeShader vizualize_voxel_cs = new ComputeShader("Engine/EngineContent/shaders/Visualize/Debug/visualize.glsl", new Vector3i(64,64,64));

        public ComputeShader ConeTracer = new ComputeShader("Engine/EngineContent/shaders/Visualize/visualize.glsl");
        public ComputeShader MergeLightingShader = new ComputeShader("Engine/EngineContent/shaders/Visualize/mergeLighting.glsl");

        FrameBuffer framebuffer;
        FrameBuffer Finalframebuffer;

        public bool Enabled;
        public bool Debug = false;
        public float DebugViewOpacity = 1.0f;

        public Color4 SkyColor;

        int voxel_texture;

        int textureRes = 256;

        public float NearPlane = 0.1f;
        public float FarPlane = 1000f;

        public float SampleLOD = 0;

        public float MinConeAngle = 0.005f;
        public float MaxConeAngle = 0.32f;

        public float NormalRayOffset = 1;
        public int MaxSamples = 4;
        public float GIBoost = 2.0f;
        public float GISkyBoxBoost = 0.5f;
        public float StepMultiplier = 0.16f;
        public bool IsTemporalAccumulation = false;
        public float MaxDistance = 1f;
        public float ConeFactor = 1f;

        Matrix4 orthographicMatrix;

        public override void Initialize(int width, int height)
        {

            voxel_texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture3D, voxel_texture);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMaxLevel, Texture.GetMaxMipmapLevel(textureRes, textureRes, textureRes));
            GL.TexStorage3D(TextureTarget3d.Texture3D, Texture.GetMaxMipmapLevel(textureRes, textureRes, textureRes), SizedInternalFormat.Rgba8, textureRes, textureRes, textureRes);
            GL.BindTexture(TextureTarget.Texture3D, 0);

            Console.WriteLine(Texture.GetMaxMipmapLevel(textureRes, textureRes, textureRes));

            FrameBufferSpecification specs = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment()
                    {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    }
                }
            };

            framebuffer = new FrameBuffer(specs);
            Finalframebuffer = new FrameBuffer(specs);

            orthographicMatrix = Matrix4.CreateOrthographicOffCenter(
                GridMin.X, GridMax.X, GridMin.Y, GridMax.Y, GridMax.Z, GridMin.Z
            );
        }

        public override void DoRenderPass()
        {
            if (Enabled)
            {
                Voxelize();
                if (Debug)
                {
                    VisualizeDebug();
                } else
                {
                    Visualize();
                }
            }
        }

        public override void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
            Finalframebuffer.Resize(width, height);

            base.Resize(width, height);
        }

        public Vector3 GridMin = new Vector3(-28,-3,-17);

        public Vector3 GridMax = new Vector3(28,20,17);

        private Vector3 prevGridMin;
        private Vector3 prevGridMax;

        public void UpdateOrthographicProjection()
        {
            if (prevGridMax != GridMax || prevGridMin != GridMin)
            {
                orthographicMatrix = Matrix4.CreateOrthographicOffCenter(
                    GridMin.X, GridMax.X, GridMin.Y, GridMax.Y, GridMax.Z, GridMin.Z
                );
            }
        }

        public void Voxelize()
        {
            GL.ClearTexImage(voxel_texture, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            framebuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, textureRes, textureRes);
            GL.ColorMask(false, false, false, false);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);


            voxelize_shader.Use();

            GL.BindImageTexture(0, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);
            GL.BindTextureUnit(0, RenderGraph.LightBuffer.GetColorAttachment(1));

            Renderer3D.UploadLightingData(voxelize_shader);

            List<DrawItem> drawList = Renderer3D.GetRenderDrawList();

            Renderer3D.UploadCameraData(voxelize_shader);

            voxelize_shader.SetMatrix4("W_ORTHOGRAPHIC_MATRIX", orthographicMatrix);

            for (int i = 0; i < drawList.Count; i++)
            {
                Renderer3D.UploadModelData(voxelize_shader, drawList[i].position, drawList[i].rotation, drawList[i].scale);

                Material material = drawList[i].mesh.Material;

                material.SetPropertyVector3(voxelize_shader, "material.albedo");
                material.SetPropertyVector3(voxelize_shader, "material.emission");
                material.SetPropertyFloat(voxelize_shader, "material.emissionStr");

                if (material.GetInt("USE_TEX_0") == 1)
                {
                    voxelize_shader.SetInt("USE_TEX_0", 1);
                    voxelize_shader.SetInt("material.ALBEDO_TEX", 0);
                    material.GetTexture("material.ALBEDO_TEX").SetActiveUnit(TextureActiveUnit.UNIT0);
                }
                else
                {
                    voxelize_shader.SetInt("USE_TEX_0", 0);
                }

                if (material.GetInt("USE_TEX_2") == 1)
                {
                    voxelize_shader.SetInt("USE_TEX_2", 1);
                    voxelize_shader.SetInt("material.EMISSION_TEX", 1);
                    material.GetTexture("material.EMISSION_TEX").SetActiveUnit(TextureActiveUnit.UNIT1);
                }
                else
                {
                    voxelize_shader.SetInt("USE_TEX_2", 0);
                }

                drawList[i].mesh.Draw();
            }

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.TextureFetchBarrierBit);

            Renderer3D.ResetViewport();

            GL.ColorMask(true, true, true, true);

            framebuffer.UnBind();

            GL.GenerateTextureMipmap(voxel_texture);
        }

        public void Visualize()
        {

            ConeTracer.Use();

            GL.BindImageTexture(0, framebuffer.GetColorAttachment(0), 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            GL.BindTextureUnit(0, voxel_texture);
            GL.BindTextureUnit(1, RenderGraph.GeometryBuffer.GetColorAttachment(0));
            GL.BindTextureUnit(2, RenderGraph.GeometryBuffer.GetColorAttachment(1));
            GL.BindTextureUnit(3, RenderGraph.GeometryBuffer.GetDepthAttachment());
            GL.BindTextureUnit(4, RenderGraph.GeometryBuffer.GetColorAttachment(2));

            ConeTracer.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            ConeTracer.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            ConeTracer.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            ConeTracer.SetMatrix4("W_ORTHOGRAPHIC_MATRIX", orthographicMatrix);

            ConeTracer.SetVector3("GridMin", GridMin);
            ConeTracer.SetVector3("GridMax", GridMax);

            ConeTracer.SetFloat("NormalRayOffset", NormalRayOffset);
            ConeTracer.SetFloat("GIBoost", GIBoost);
            ConeTracer.SetFloat("StepMultiplier", StepMultiplier);
            ConeTracer.SetInt("MaxSamples", MaxSamples);
            ConeTracer.SetBool("IsTemporalAccumulation", IsTemporalAccumulation);
            ConeTracer.SetFloat("MaxDistance", MaxDistance);
            ConeTracer.SetFloat("maxConeAngle", MaxConeAngle);
            ConeTracer.SetFloat("minConeAngle", MinConeAngle);
            ConeTracer.SetFloat("coneFactor", ConeFactor);


            ConeTracer.Dispatch((int)(RenderGraph.CompositeBuffer.GetSize().X / 8), (int)(RenderGraph.CompositeBuffer.GetSize().Y / 8), 1);

            ConeTracer.Wait();

            Texture.UnbindTexture();


            MergeLightingShader.Use();

            GL.BindImageTexture(0, Finalframebuffer.GetColorAttachment(0), 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            GL.BindTextureUnit(0, RenderGraph.CompositeBuffer.GetColorAttachment(0));
            GL.BindTextureUnit(1, framebuffer.GetColorAttachment(0));
            GL.BindTextureUnit(2, RenderGraph.GeometryBuffer.GetColorAttachment(3));

            MergeLightingShader.Dispatch((int)(RenderGraph.CompositeBuffer.GetSize().X / 8), (int)(RenderGraph.CompositeBuffer.GetSize().Y / 8), 1);
            MergeLightingShader.Wait();

            Texture.UnbindTexture();

            RendererUtils.BlitFBToScreen(Finalframebuffer, RenderGraph.CompositeBuffer, 1.0f);
        }

        public void VisualizeDebug()
        {

            vizualize_voxel_cs.Use();

            GL.BindImageTexture(0, framebuffer.GetColorAttachment(0), 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            GL.BindTextureUnit(0, voxel_texture);

            vizualize_voxel_cs.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            vizualize_voxel_cs.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            vizualize_voxel_cs.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            vizualize_voxel_cs.SetMatrix4("W_ORTHOGRAPHIC_MATRIX", orthographicMatrix);

            GridMax = new Vector3((int)GridMax.X, (int)GridMax.Y, (int)GridMax.Z);
            GridMin = new Vector3((int)GridMin.X, (int)GridMin.Y, (int)GridMin.Z);

            vizualize_voxel_cs.SetVector3("GridMin", GridMin);
            vizualize_voxel_cs.SetVector3("GridMax", GridMax);
            vizualize_voxel_cs.SetFloat("NearPlane", NearPlane);
            vizualize_voxel_cs.SetFloat("FarPlane", FarPlane);
            vizualize_voxel_cs.SetFloat("ConeAngle", MinConeAngle);
            vizualize_voxel_cs.SetFloat("StepMultiplier", StepMultiplier);
            vizualize_voxel_cs.SetVector3("SkyColor", new Vector3(SkyColor.R, SkyColor.G, SkyColor.B));

            vizualize_voxel_cs.SetFloat("SampleLOD", SampleLOD);


            vizualize_voxel_cs.Dispatch((int)(RenderGraph.CompositeBuffer.GetSize().X / 8), (int)(RenderGraph.CompositeBuffer.GetSize().Y / 8), 1);

            vizualize_voxel_cs.Wait();

            Texture.UnbindTexture();

            RendererUtils.BlitFBToScreen(framebuffer, RenderGraph.CompositeBuffer, DebugViewOpacity);

        }

    }
}
