﻿using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using ImGuiNET;
using DevoidEngine.Engine.Imgui;

namespace DevoidEngine.Engine.Rendering
{
    class VoxelTracer : RenderPass
    {
        Shader voxelize_shader = new Shader("Engine/EngineContent/shaders/Voxelize/voxelize.vert", "Engine/EngineContent/shaders/Voxelize/voxelize.frag", "Engine/EngineContent/shaders/Voxelize/voxelize.geom");
        Shader vizualize_voxel = new Shader("Engine/EngineContent/shaders/Visualize/visualize");
        
        ComputeShader vizualize_voxel_cs = new ComputeShader("Engine/EngineContent/shaders/Visualize/visualize.glsl", new Vector3i(64,64,64));

        Shader world_pos = new Shader("Engine/EngineContent/shaders/ToWorldPos");

        ComputeShader compute = new ComputeShader("Engine/EngineContent/shaders/voxelize.glsl", new Vector3i(64, 64, 64));

        FrameBuffer FrontFace, BackFace;

        int voxel_texture, visualize_texture;

        public override void Initialize(int width, int height)
        {

            voxel_texture = GL.GenTexture();
            visualize_texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture3D, voxel_texture);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMaxLevel, 1);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);

            GL.TexStorage3D(TextureTarget3d.Texture3D, 1, SizedInternalFormat.Rgba8, 64, 64, 64);

            GL.BindTexture(TextureTarget.Texture3D, 0);

            int w = 64, h = 64, d = 64;
            Vector4[] voxels = new Vector4[w * h * d];
            Random rng = new Random();
            for (int i = 0; i < voxels.Length; i++)
            {
                if (rng.Next(0, 1000) < 1) // 0.1% chance of setting a voxel
                {
                    Vector4 rnd = new Vector4(rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1.0f);
                    voxels[i] = rnd;
                }
            }
            GL.TextureSubImage3D(voxel_texture, 0, 0, 0, 0, w, h, d, PixelFormat.Rgba, PixelType.Float, voxels);

            GL.TextureSubImage3D(voxel_texture, 0, 3, 3, 3, 1, 1, 1, PixelFormat.Rgba, PixelType.Float, new Vector4[] { new Vector4(0.5f, 0.7f, 0.2f, 1f) });

            Console.WriteLine("Handle " + voxel_texture);

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

            FrontFace = new FrameBuffer(specs);
            BackFace = new FrameBuffer(specs);
        }

        public override void DoRenderPass()
        {
            Voxelize();
            //Visualize();
            base.DoRenderPass();
        }

        public override void Resize(int width, int height)
        {
            FrontFace.Resize(width, height);
            BackFace.Resize(width, height);

            base.Resize(width, height);
        }

        int count = 0;

        public Vector3 GridMin, GridMax = new Vector3(2);

        public void Voxelize()
        {
            count += 1;

            //GL.ClearTexImage(voxel_texture, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            ClearTextures();

            return;

            FrontFace.UnBind();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, 64, 64);
            GL.ColorMask(false, false, false, false);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);


            voxelize_shader.Use();

            GL.BindImageTexture(0, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);

            Renderer3D.UploadLightingData(voxelize_shader);

            List<DrawItem> drawList = Renderer3D.GetRenderDrawList();

            Renderer3D.UploadCameraData(voxelize_shader);

            for (int i = 0; i < drawList.Count; i++)
            {
                Renderer3D.UploadModelData(voxelize_shader, drawList[i].position, drawList[i].rotation, drawList[i].scale);

                Material material = drawList[i].mesh.Material;

                voxelize_shader.SetVector3("material.albedo", material.GetVec3("material.albedo"));
                //voxelize_shader.SetInt("USE_TEX_0", material.GetInt("USE_TEX_0"));
                //voxelize_shader.SetInt("USE_TEX_0", 0);
                //voxelize_shader.SetInt("material.ALBEDO_TEX", 0);
                //material.Apply();
                
                drawList[i].mesh.Draw();
            }

            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.TextureFetchBarrierBit);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);

            Renderer3D.ResetViewport();

            GL.ColorMask(true, true, true, true);

        }

        public void ClearTextures()
        {
            vizualize_voxel_cs.Use();

            GL.BindImageTexture(0, RenderGraph.CompositeBuffer.GetColorAttachment(0), 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            GL.BindTextureUnit(0, voxel_texture);

            vizualize_voxel_cs.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            vizualize_voxel_cs.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            vizualize_voxel_cs.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            vizualize_voxel_cs.SetMatrix4("W_ORTHOGRAPHIC_MATRIX", Renderer2D.OrthoProjection);

            vizualize_voxel_cs.SetVector3("GridMin", GridMin);
            vizualize_voxel_cs.SetVector3("GridMax", GridMax);


            vizualize_voxel_cs.Dispatch((int)(RenderGraph.CompositeBuffer.GetSize().X / 8), (int)(RenderGraph.CompositeBuffer.GetSize().Y / 8), 1);

            vizualize_voxel_cs.Wait();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.TextureFetchBarrierBit);

            //compute.Use();

            //GL.BindImageTexture(0, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);

            //compute.Dispatch(16, 16, 16);
            //compute.Wait();

            //GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.TextureFetchBarrierBit);

        }

        public void Visualize()
        {

            world_pos.Use();
            world_pos.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(10));
            world_pos.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            world_pos.SetMatrix4("W_ORTHOGRAPHIC_MATRIX", Renderer2D.OrthoProjection);
            world_pos.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            world_pos.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            GL.ClearColor(0, 0, 0, 1);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            // FrontFace
            FrontFace.Bind();
            GL.ClearColor(RenderGraph.Camera.GetClearColor().X, RenderGraph.Camera.GetClearColor().Y, RenderGraph.Camera.GetClearColor().Z, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.CullFace(CullFaceMode.Front);
            GL.Viewport(0, 0, FrontFace.GetSize().X, FrontFace.GetSize().Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RendererUtils.CubeVAO.Render();

            // Backface

            GL.CullFace(CullFaceMode.Back);
            BackFace.Bind();
            GL.Viewport(0, 0, BackFace.GetSize().X, BackFace.GetSize().Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RendererUtils.CubeVAO.Render();
            BackFace.UnBind();

            //

            GL.CullFace(CullFaceMode.Back);

            GL.Disable(EnableCap.CullFace);

            RenderGraph.CompositeBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            vizualize_voxel.Use();
            vizualize_voxel.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(1920,1080,1));
            vizualize_voxel.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            Renderer3D.UploadCameraData(vizualize_voxel);

            vizualize_voxel.SetMatrix4("W_PROJECTION_MATRIX", Renderer2D.OrthoProjection);

            // Settings.
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            // Activate textures.
            vizualize_voxel.SetInt("gBackfaceTexture", 0);
            vizualize_voxel.SetInt("gFrontfaceTexture", 1);
            vizualize_voxel.SetInt("gTexture3D", 2);

            vizualize_voxel.SetVector3("GridMin", GridMin);
            vizualize_voxel.SetVector3("GridMax", GridMax);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, BackFace.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, FrontFace.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture3D, voxel_texture);
            //GL.BindImageTexture(2, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);


            // Render.
            GL.Viewport(0, 0, 1920, 1080);
            RendererUtils.QuadVAO.Render();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderGraph.CompositeBuffer.UnBind();

        }

    }
}
