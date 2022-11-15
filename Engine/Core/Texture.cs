using DevoidEngine.Engine.Utilities;

using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Core
{
    class Texture
    {

        private int TextureHandle;

        public Texture()
        {

        }

        public Texture(int handle)
        {
            TextureHandle = handle;
        }

        public Texture(byte[] data)
        {
            LoadPixels(data);
        }

        public Texture(string path)
        {
            LoadFile(path);
        }

        public void LoadPixels(byte[] data)
        {
            Image ImageFile = new Image();
            ImageFile.LoadImage(data);

            TextureHandle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, ImageFile.Width, ImageFile.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ImageFile.Pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void LoadFile(string path)
        {
            Image ImageFile = new Image();
            ImageFile.LoadImageAlpha(path);

            TextureHandle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, ImageFile.Width, ImageFile.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ImageFile.Pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ChangeFilterType(MagMinFilterTypes filterType)
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)filterType);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)filterType);
        }

        public void ChangeWrapMode(WrapModeType wrapMode, WrapSide side)
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            if (wrapMode == WrapModeType.Repeat)
            {
                if (side == WrapSide.S)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)wrapMode);
                } else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)wrapMode);
                }
            }
        }

        public void BindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
        }

        public void SetActiveUnit(TextureActiveUnit unit, TextureTarget textureTarget = TextureTarget.Texture2D)
        {

            TextureUnit textureUnit = TextureUnit.Texture0;

            switch (unit)
            {
                case TextureActiveUnit.UNIT1:
                    textureUnit = TextureUnit.Texture1;
                    break;
                case TextureActiveUnit.UNIT2:
                    textureUnit = TextureUnit.Texture2;
                    break;
                case TextureActiveUnit.UNIT3:
                    textureUnit = TextureUnit.Texture3;
                    break;
                case TextureActiveUnit.UNIT4:
                    textureUnit = TextureUnit.Texture4;
                    break;
                default:
                    textureUnit = TextureUnit.Texture0;
                    break;
            }

            GL.ActiveTexture(textureUnit);
            GL.BindTexture(textureTarget, TextureHandle);
        }

        public int GetTexture()
        {
            return TextureHandle;
        }

        ~Texture()
        {
            //if (TextureHandle != 0)
            //{
            //    GL.DeleteTexture(TextureHandle);
            //}
        }
    }

    public enum TextureActiveUnit
    {
        UNIT0,
        UNIT1,
        UNIT2,
        UNIT3,
        UNIT4,
        UNIT5,
        UNIT6,
        UNIT7,
        UNIT8,
        UNIT9,
        UNIT10,
        UNIT11,
        UNIT12,
        UNIT13,
        UNIT14,
        UNIT15,
        UNIT16,
        UNIT17,
        UNIT18,
        UNIT19
    }

    public enum MagMinFilterTypes
    {
        Linear = 9729,
        Nearest = 9728
    }

    public enum WrapSide
    {
        T,
        S
    }

    public enum WrapModeType
    {
        Clamp = 10496,
        Repeat = 10497,
        ClampToBorder = 33069,
        ClampToBorderArb = 33069,
        ClampToBorderNv = 33069,
        ClampToBorderSgis = 33069,
        ClampToEdge = 33071,
        ClampToEdgeSgis = 33071,
        MirroredRepeat = 33648
    }
}
