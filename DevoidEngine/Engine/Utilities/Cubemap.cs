using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using System;

namespace DevoidEngine.Engine.Utilities
{
    public class Cubemap
    {

        public int Handle;

        public int GetRendererID()
        {
            return Handle;
        }

        public void BindUnit(int unit)
        {
            GL.BindTextureUnit(unit, Handle);
        }

        public void Create(int width, int height, FilterTypes minFilter, FilterTypes magFilter)
        {
            Handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);

            for (int i = 0; i < 6; i++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (float)minFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (float)magFilter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);

            GL.GenerateTextureMipmap(Handle);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public void Create(string[] faces)
        {
            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, Handle);

            for (int i = 0; i < 6; i++)
            {
                Image data = new Image(faces[i]);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.Pixels);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }
    }
}
