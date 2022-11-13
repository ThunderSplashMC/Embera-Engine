using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    class GBuffer
    {

        int GBufferFBO, RenderBufferHandle;

        int gPosition, gNormal, gSpecular;

        Shader GBufferShader;

        public void Init(int width, int height)
        {
            GBufferFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GBufferFBO);

            gPosition = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gPosition, 0);

            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gNormal, 0);

            gSpecular = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gSpecular);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gSpecular, 0);

            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0,DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2});

            RenderBufferHandle = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferHandle);

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RenderBufferHandle);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) { Console.WriteLine("There was an error creating the frame buffer"); Console.WriteLine(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer)); }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Shader Creation

            GBufferShader = new Shader("Engine/EngineContent/shaders/gbuffer");

        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, GBufferFBO);
        }

        public void UnBind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void SetupGBufferShader(Matrix4 projection, Matrix4 view, Matrix4 model)
        {

            GBufferShader.Use();

            GBufferShader.SetMatrix4("W_PROJECTION_MATRIX", projection);
            GBufferShader.SetMatrix4("W_VIEW_MATRIX", view);
            GBufferShader.SetMatrix4("W_MODEL_MATRIX", model);
        }

        public int GetColorAttachment(int index)
        {
            switch (index)
            {
                case 0: return gPosition;
                case 1: return gNormal;
                case 2: return gSpecular;
                default: return 0;
            }
        }

        public void ResolveToFramebuffer()
        {
            GL.BlitNamedFramebuffer(GBufferFBO, Renderer3D.GetRendererData().RenderPass.GetRendererID(), 0, 0, 1280, 720, 0, 0, 1280, 720, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }
    }
}
