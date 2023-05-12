using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using ImGuiNET;

namespace DevoidEngine.Engine.Rendering
{

    public struct BloomMip
    {
        public Vector2 size;
        public Vector2i sizeInt;
        public int texture;
    }

    public class BloomFrameBuffer
    {
        public int FrameBufferHandle;
        public List<BloomMip> bloomMips = new List<BloomMip>();

        public void Init(int width, int height, int mipChainLength = 8)
        {
            FrameBufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);

            Vector2 mipSize = new Vector2(width, height);
            Vector2i mipSizeInt = new Vector2i(width, height);

            for (int i = mipChainLength - 1; i > 0; i--)
            {
                BloomMip bloomMip = new BloomMip();

                mipSize *= 0.5f;
                mipSizeInt /= 2;
                if (mipSizeInt == new Vector2i(1, 1))
                {
                    continue;
                }

                bloomMip.size = mipSize;
                bloomMip.sizeInt = mipSizeInt;


                GL.GenTextures(1, out bloomMip.texture);
                GL.BindTexture(TextureTarget.Texture2D, bloomMip.texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R11fG11fB10f, mipSizeInt.X, mipSizeInt.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                bloomMips.Add(bloomMip);
            }

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, bloomMips[0].texture, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("There was an error while creating bloom FBO");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
        }

        public List<BloomMip> GetBloomMips()
        {
            return bloomMips;
        }

        ~BloomFrameBuffer()
        {
            for (int i = 0; i < bloomMips.Count; i++)
            {
                //    GL.DeleteTexture(bloomMips[i].texture);
            }
            //GL.DeleteFramebuffer(FrameBufferHandle);
        }
    }

    public class BrightnessFilterFrameBuffer 
    {

        int FrameBufferHandle;
        int FilterTexture;

        public void Init(int width, int height)
        {
            FrameBufferHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);

            GL.GenTextures(1, out FilterTexture);
            GL.BindTexture(TextureTarget.Texture2D, FilterTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f,width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FilterTexture, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("There was an error while creating bloom FBO");
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
        }

        public int GetTexture()
        {
            return FilterTexture;
        }

    }


    public class Bloom
    {
        Vector2 viewportSize;
        Vector2i viewportSizeInt;
        BloomFrameBuffer BloomFrameBuffer;
        Shader DownsampleShader, UpsampleShader, BrightnessFilter;
        bool KarisOnDownsample = true;
        FrameBuffer BrightFrameBuffer;

        public void Init(int width, int height)
        {
            viewportSize = new Vector2(width, height);
            viewportSizeInt = new Vector2i(width, height);
            BloomFrameBuffer = new BloomFrameBuffer();
            BloomFrameBuffer.Init(viewportSizeInt.X, viewportSizeInt.Y, 8);
            BrightFrameBuffer = new FrameBuffer(new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F}
                },
                DepthAttachment = new DepthAttachment() { width = width, height = height }
            });

            DownsampleShader = new Shader("Engine/EngineContent/shaders/Bloom/downsample");
            UpsampleShader = new Shader("Engine/EngineContent/shaders/Bloom/upsample");
            BrightnessFilter = new Shader("Engine/EngineContent/shaders/Bloom/brightfilter");

            DownsampleShader.Use();
            DownsampleShader.SetInt("srcTexture", 0);

            UpsampleShader.Use();
            UpsampleShader.SetInt("srcTexture", 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void RenderDownSamples(int texture)
        {
            List<BloomMip> bloomMips = BloomFrameBuffer.GetBloomMips();

            DownsampleShader.Use();
            DownsampleShader.SetVector2("srcResolution", viewportSize);
            
            if (KarisOnDownsample)
            {
                DownsampleShader.SetInt("mipLevel", 0);
            }

            DownsampleShader.SetInt("srcTexture", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            for (int i = 0; i < bloomMips.Count; i++)
            {
                BloomMip bloomMip = bloomMips[i];
                GL.Viewport(0, 0, bloomMip.sizeInt.X, bloomMip.sizeInt.Y);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, bloomMip.texture, 0);
                RendererUtils.QuadVAO.Render();
                DownsampleShader.SetVector2("srcResolution", bloomMip.size);
                GL.BindTexture(TextureTarget.Texture2D, bloomMip.texture);
                if (i==0) { DownsampleShader.SetInt("mipLevel", 1); }
            }
            GL.UseProgram(0);
        }

        public void RenderUpSamples(float filterRadius)
        {
            List<BloomMip> bloomMips = BloomFrameBuffer.GetBloomMips();

            UpsampleShader.Use();
            UpsampleShader.SetFloat("filterRadius", filterRadius);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            for (int i = bloomMips.Count - 1; i > 0; i--)
            {
                BloomMip bloomMip = bloomMips[i];
                BloomMip nextMip = bloomMips[i - 1];

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, bloomMip.texture);
                GL.Viewport(0, 0, nextMip.sizeInt.X, nextMip.sizeInt.Y);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, nextMip.texture, 0);

                RendererUtils.QuadVAO.Render();
            }

            GL.Disable(EnableCap.Blend);
            GL.UseProgram(0);
        }

        public int GetBloomTexture()
        {
            return BloomFrameBuffer.GetBloomMips()[0].texture;
        }

        public void FilterTexture(int srctex)
        {
            GL.Viewport(0, 0, viewportSizeInt.X, viewportSizeInt.Y);
            BrightFrameBuffer.Bind();
            BrightnessFilter.Use();
            BrightnessFilter.SetInt("scene", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, srctex);
            RendererUtils.QuadVAO.Render();
            BrightFrameBuffer.UnBind();
        }

        public void RenderBloomTexture(int srcTexture)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            //FilterTexture(srcTexture);


            BloomFrameBuffer.Bind();

            RenderDownSamples(srcTexture);
            //RenderDownSamples(BrightFrameBuffer.GetColorAttachment(0));
            RenderUpSamples(filterRadius);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, viewportSizeInt.X, viewportSizeInt.Y);
        }

        public bool enabled = true;
        public float bloomStr = 0.19f;
        public float bloomExposure = 1f;
        public float filterRadius = 0f;

        public void Resize(int width, int height)
        {
            //viewportSize = new Vector2(width, height);
            //viewportSizeInt = new Vector2i(width, height);
            //BloomFrameBuffer = new BloomFrameBuffer();
        }
    }
}
