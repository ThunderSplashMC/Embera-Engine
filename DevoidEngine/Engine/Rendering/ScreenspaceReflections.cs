using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    public class ScreenspaceReflections : RenderPass
    {

        public FrameBuffer framebuffer;
        public ComputeShader SSR_Shader = new ComputeShader("Engine/EngineContent/shaders/SSR/SSR_Custom.glsl");

        public override void Initialize(int width, int height)
        {

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
        }

        public override void Resize(int width, int height)
        {
            framebuffer.Resize(width, height);
        }


        public override void DoRenderPass()
        {
            SSR_Shader.Use();

            GL.ClearTexImage(framebuffer.GetColorAttachment(0), 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindImageTexture(0, framebuffer.GetColorAttachment(0), 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);

            GL.BindTextureUnit(0, RenderGraph.CompositeBuffer.GetColorAttachment(0));
            GL.BindTextureUnit(1, RenderGraph.GeometryBuffer.GetColorAttachment(0));
            GL.BindTextureUnit(2, RenderGraph.GeometryBuffer.GetColorAttachment(1));

            SSR_Shader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            SSR_Shader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            SSR_Shader.SetVector3("C_VIEWPOS", RenderGraph.Camera.position);


            SSR_Shader.Dispatch(framebuffer.GetSize().X / 8, framebuffer.GetSize().Y / 8, 1);

            SSR_Shader.Wait();
        }

    }
}
