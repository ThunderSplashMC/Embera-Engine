using System;
using System.Collections.Generic;
using System.Linq;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using OpenTK.Graphics.OpenGL;

namespace Elemental.Editor.EditorUtils
{
    class EditorRendererPass : RenderPass
    {
        ComputeShader SelectShader = new ComputeShader("Editor/Assets/Shaders/ObjectSelect/objectselect.glsl"); // Shader that outputs the selected UUID based on cursor position
        public int texture; // The Select texture. Value = SelectedObject UUID
        public FrameBuffer ReadBuffer;

        public override void Initialize(int width, int height)
        {
            texture = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32i, 1, 1, 0, PixelFormat.RedInteger, PixelType.UnsignedInt, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            ReadBuffer = new FrameBuffer(new FrameBufferSpecification()
            {
                width = 1,
                height = 1,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment()
                    {
                        textureFormat = FrameBufferTextureFormat.R32I,
                        textureType = FrameBufferTextureType.Texture2D
                    }
                }
            });

            //GL.BindTexture(TextureTarget.Texture2D, texture);
            //ReadBuffer.AttachColorTexture(texture, PixelInternalFormat.R32i, PixelFormat.RedInteger, 1, 1, 0, TextureTarget.Texture2D);
        }

        public override void DoRenderPass()
        {

        }

        public int GetClickUUID(Vector2 ClickPosition)
        {
            // ALTERNATE METHOD (SLOW BUT NEGLIGIBLE);

            int[] readpixels = new int[1];

            GL.GetTextureSubImage(RenderGraph.GeometryBuffer.GetColorAttachment(4), 0, (int)ClickPosition.X, (int)ClickPosition.Y, 0, 1, 1, 1, PixelFormat.RedInteger, PixelType.Int, sizeof(int), readpixels);

            return readpixels[0];

            // FAST APPROACH (I THINK)

            SelectShader.Use();

            SelectShader.SetVector2("ClickPosition", ClickPosition);

            GL.BindImageTexture(0, texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.R32i);
            GL.BindTextureUnit(0, RenderGraph.GeometryBuffer.GetColorAttachment(4));

            SelectShader.Dispatch(1, 1, 1);
            SelectShader.Wait();

            int[] pixels = new int[1];

            GL.GetTextureSubImage(texture, 0, 0, 0, 0, 1, 1, 1, PixelFormat.RedInteger, PixelType.Int, sizeof(int), pixels);

            return pixels[0];
        }

        public override void Resize(int width, int height)
        {

        }

    }
}
