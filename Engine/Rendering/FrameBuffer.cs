using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{

    public enum FrameBufferTextureFormat
    {
        RGB,
        RGBA8,
        RGBA16F,
        RGBA32F,
        R11G11B10,
        RG16F,
        R32I,
        R32F,
    }

    public enum FrameBufferTextureType
    {
        Texture2D,
        Cubemap,
        Texture3D
    }

    public struct ColorAttachment
    {
        public FrameBufferTextureFormat textureFormat;
        public FrameBufferTextureType textureType;
    }

    public struct DepthAttachment
    {
        public int width, height;
        public FrameBufferTextureType textureType;
    }

    struct FrameBufferSpecification
    {
        public int width, height;
        public ColorAttachment[] ColorAttachments;
        public DepthAttachment DepthAttachment;

    }

    class FrameBuffer
    {
        private int RendererID;
        private List<int> ColorAttachments = new List<int>();
        private int DepthAttachment = 0;

        FrameBufferSpecification frameBufferSpecification;

        public FrameBuffer(FrameBufferSpecification frameBufferSpecification)
        {
            this.frameBufferSpecification = frameBufferSpecification;

            Create();
        }

        public void AttachColorTexture(int texture, PixelInternalFormat internalFormat, PixelFormat pixelFormat, int width, int height, int index, TextureTarget textureTarget = TextureTarget.Texture2D, PixelType pixelType = PixelType.UnsignedByte)
        {
            GL.TexImage2D(textureTarget, 0, internalFormat, width, height, 0, pixelFormat, pixelType, IntPtr.Zero);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);

            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + index, texture, 0);
        }

        public void AttachDepthTexture(int texture, int width, int height, TextureTarget textureTarget = TextureTarget.Texture2D)
        {
            if (textureTarget == TextureTarget.TextureCubeMap)
            {
                for (int i = 0; i < 6; i++)
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);
            }

            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);

            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);


            switch (textureTarget)
            {
                case TextureTarget.Texture2D:
                    GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, textureTarget, texture, 0);
                    break;
                case TextureTarget.TextureCubeMap:
                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, texture, 0);
                    break;
                default:
                    break;
            }
        }

        public void Create()
        {
            if (RendererID != 0)
            {
                GL.DeleteTextures(ColorAttachments.Count, ColorAttachments.ToArray());
                GL.DeleteTexture(DepthAttachment);
                GL.DeleteFramebuffer(RendererID);
            }


            RendererID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RendererID);

            ColorAttachment[] colorAttachments = frameBufferSpecification.ColorAttachments;

            if (colorAttachments != null)
            {
                for (int i = 0; i < colorAttachments.Length; i++)
                {
                    int colorTexture = GL.GenTexture();
                    ColorAttachments.Add(colorTexture);

                    GL.BindTexture(TextureTarget.Texture2D, colorTexture);

                    TextureTarget texturetarget = colorAttachments[i].textureType == FrameBufferTextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;

                    switch (colorAttachments[i].textureFormat)
                    {
                        case (FrameBufferTextureFormat.RGB):
                            AttachColorTexture(colorTexture, PixelInternalFormat.Rgb, PixelFormat.Rgb, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.RGBA8):
                            AttachColorTexture(colorTexture, PixelInternalFormat.Rgba8, PixelFormat.Rgba, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.RGBA16F):
                            AttachColorTexture(colorTexture, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.R11G11B10):
                            AttachColorTexture(colorTexture, PixelInternalFormat.R11fG11fB10f, PixelFormat.Rgb, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.RGBA32F):
                            AttachColorTexture(colorTexture, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.RG16F):
                            AttachColorTexture(colorTexture, PixelInternalFormat.Rg16f, PixelFormat.Rg, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case (FrameBufferTextureFormat.R32I):
                            AttachColorTexture(colorTexture, PixelInternalFormat.R32i, PixelFormat.RedInteger, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget);
                            break;
                        case FrameBufferTextureFormat.R32F:
                            AttachColorTexture(colorTexture, PixelInternalFormat.R32f, PixelFormat.Red, frameBufferSpecification.width, frameBufferSpecification.height, i, texturetarget, PixelType.Float);
                            break;
                        default:
                            Console.WriteLine("FrameBuffer: Invalid Texture Format");
                            break;

                    }
                }
            }

            if (frameBufferSpecification.DepthAttachment.width != 0 || frameBufferSpecification.DepthAttachment.height != 0)
            {
                DepthAttachment = GL.GenTexture();

                TextureTarget texturetarget = frameBufferSpecification.DepthAttachment.textureType == FrameBufferTextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;

                GL.BindTexture(texturetarget, DepthAttachment);

                AttachDepthTexture(DepthAttachment, frameBufferSpecification.DepthAttachment.width, frameBufferSpecification.DepthAttachment.height, texturetarget);
            }

            if (ColorAttachments.Count > 1)
            {
                DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[5]
                {
                        DrawBuffersEnum.ColorAttachment0,
                        DrawBuffersEnum.ColorAttachment1,
                        DrawBuffersEnum.ColorAttachment2,
                        DrawBuffersEnum.ColorAttachment3,
                        DrawBuffersEnum.ColorAttachment4,
                };
                GL.DrawBuffers(drawBuffers.Length, drawBuffers);
            }

            if (ColorAttachments.Count == 0)
            {
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
            }

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Incomplete FBO: " + GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer));
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        }

        ~FrameBuffer()
        {
            if (ColorAttachments.Count > 0)
            {
                GL.DeleteTextures(ColorAttachments.Count, ColorAttachments.ToArray());
            }
            if ((frameBufferSpecification.DepthAttachment.width != 0 || frameBufferSpecification.DepthAttachment.height != 0) && GL.IsTexture(DepthAttachment))
            {
                GL.DeleteTexture(DepthAttachment);
            }
            GL.DeleteFramebuffer(RendererID);
        }

        public int GetRendererID()
        {
            return RendererID;
        }

        public void Resize(int width, int height)
        {
            frameBufferSpecification.width = width;
            frameBufferSpecification.height = height;
            Create();
        }

        public Vector2i GetSize()
        {
            return new Vector2i(frameBufferSpecification.width, frameBufferSpecification.height);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, RendererID);
        }

        public void UnBind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void BindColorAttachment(int index, int slot)
        {
            GL.BindTextureUnit(slot, ColorAttachments[index]);
        }

        public int GetColorAttachment(int index)
        {
            return ColorAttachments[index];
        }

        public int GetDepthAttachment()
        {
            return DepthAttachment;
        }

    }
}
