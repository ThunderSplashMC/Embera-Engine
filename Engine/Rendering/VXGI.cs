using System;
using System.Collections.Generic;
using System.Diagnostics;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace DevoidEngine.Engine.Rendering
{
    class VXGI
    {

        Shader voxelize_shader = new Shader("Engine/EngineContent/shaders/voxelize.vert", "Engine/EngineContent/shaders/voxelize.frag", "Engine/EngineContent/shaders/voxelize.geom");
        Shader vizualize_voxel = new Shader("Engine/EngineContent/shaders/voxel_visualization");
        //ComputeShader vizualize_voxel_cs = new ComputeShader("Engine/EngineContent/shaders/experimental/voxel_visualize.glsl");
        Shader world_pos = new Shader("Engine/EngineContent/shaders/ToWorldPos");
        int voxel_texture;
        int visualize_texture;
        public static int debugTexture;
        ComputeShader compute = new ComputeShader("Engine/EngineContent/shaders/voxelize.glsl", new Vector3i(64, 64, 64));
        FrameBuffer FBO1, FBO2, FBO3;
        Mesh cubemesh, quadmesh;
        Texture outtexture;

        public void Init(int width, int height)
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

            //

            GL.BindTexture(TextureTarget.Texture2D, visualize_texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 1);

            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, 1920, 1080);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            // Visualization


            FrameBufferSpecification FBS = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() { textureFormat = FrameBufferTextureFormat.RGBA16F }
                },
                DepthAttachment = new DepthAttachment() { width = width, height = height }

            };

            FBO1 = new FrameBuffer(FBS);
            FBO2 = new FrameBuffer(FBS);
            FBO3 = new FrameBuffer(FBS);

            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO1.GetRendererID());
            //RBO1 = GL.GenRenderbuffer();
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO1);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height); // Use a single rbo for both depth and stencil buffer.
            //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO1);
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO2.GetRendererID());
            //RBO2 = GL.GenRenderbuffer();
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO2);
            //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height); // Use a single rbo for both depth and stencil buffer.
            //GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, RBO2);
            //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);



            cubemesh = ModelImporter.LoadModel("Engine/EngineContent/model/cube.fbx")[0];
            quadmesh = new Mesh();
            quadmesh.SetVertices(VERTEX_DEFAULTS.GetFrameBufferVertices());

        }

        public void Voxelize()
        {
            ClearTextures();
            voxelize_shader.Use();
            voxelize_shader.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            Renderer3D.UploadLightingData(voxelize_shader);

            GL.BindImageTexture(0, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);
            Renderer3D.Draw();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.TextureFetchBarrierBit);
        }

        public void Visualize()
        {
            //vizualize_voxel_cs.Use();

            //GL.BindImageTexture(0, visualize_texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
            //GL.BindImageTexture(1, voxel_texture, 0, true, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba8);

            //vizualize_voxel_cs.Dispatch((1920 + 8 - 1) / 8, (1080 + 8 - 1) / 8, 1);

            //RenderGraph.CompositePass.Bind();

            //GL.Disable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.CullFace);

            //world_pos.Use();
            //world_pos.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(0, 0, 0));
            //world_pos.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            //world_pos.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            //world_pos.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            //cubemesh.Draw();

            //RenderGraph.CompositePass.UnBind();

            world_pos.Use();
            world_pos.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(5));
            world_pos.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            world_pos.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            world_pos.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            GL.ClearColor(RenderGraph.Camera.GetClearColor().X, RenderGraph.Camera.GetClearColor().Y, RenderGraph.Camera.GetClearColor().Z, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.CullFace(CullFaceMode.Front);
            FBO1.Bind();
            GL.Viewport(0, 0, FBO1.GetSize().X, FBO1.GetSize().Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cubemesh.Draw();

            // Front.
            GL.CullFace(CullFaceMode.Back);
            FBO2.Bind();
            GL.Viewport(0, 0, FBO2.GetSize().X, FBO2.GetSize().Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            cubemesh.Draw();

            FBO1.UnBind();

            GL.CullFace(CullFaceMode.Back);

            RenderGraph.CompositeBuffer.Bind();

            vizualize_voxel.Use();
            vizualize_voxel.SetMatrix4("W_PROJECTION_MATRIX", Renderer2D.OrthoProjection);
            vizualize_voxel.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(0, 0, 0) * Matrix4.CreateScale(1920, 1080, 1));
            vizualize_voxel.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);

            // Settings.
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            // Activate textures.
            vizualize_voxel.SetInt("textureBack", 0);
            vizualize_voxel.SetInt("textureFront", 1);
            vizualize_voxel.SetInt("texture3D", 2);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FBO1.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, FBO1.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture3D, voxel_texture);

            // Render.
            GL.Viewport(0, 0, 1920, 1080);
            RendererUtils.QuadVAO.Render();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderGraph.CompositeBuffer.UnBind();

        }

        public void ClearTextures() 
        {
            compute.Use();

            GL.BindImageTexture(0, voxel_texture, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba8);

            compute.Dispatch(16,16,16);
            compute.Wait();

        }

        public void DummyTestFunc(DevoidEngine.Engine.Core.Material model)
        {
            voxelize_shader.SetMatrix4("W_MODEL_MATRIX", model.GetMatrix4("W_MODEL_MATRIX") * Matrix4.CreateScale(0.2f));
            voxelize_shader.SetVector3("color", model.GetVec3("material.albedo"));
        }

    }
}
