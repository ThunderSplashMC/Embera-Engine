﻿using DevoidEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace DevoidEngine.Engine.Core
{
    public enum TextureFormat
    {
        RGB = 6487,
        RGBA8,
        RGBA16F,
        RGBA32F,
        R11G11B10,
        RG16F,
        R32I,
        R32F
    }

    public struct TextureAttribute
    {
        public string AttrName;
        public Texture Tex;
        public int TextureIndex;

        public TextureAttribute(string AttrName, Texture Tex, int TextureIndex)
        {
            this.AttrName = AttrName;
            this.Tex = Tex;
            this.TextureIndex = TextureIndex;
        }
    }
    public class Texture : IResource
    {
        public FilterTypes FilterType = FilterTypes.Linear;
        private int TextureHandle;
        private Image ImageRef;
        public string fileID;

        bool isDisposed;

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
            fileID = Path.GetFileName(path);
            LoadFile(path);
        }

        public void LoadPixels(byte[] data)
        {
            Image ImageFile = new Image();
            ImageRef = ImageFile;
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
            ImageRef = ImageFile;
            ImageFile.LoadImageAlpha(path);

            TextureHandle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, ImageFile.Width, ImageFile.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ImageFile.Pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxAnisotropy, 8f);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void ChangeFilterType(FilterTypes filterType)
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
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)wrapMode);
                }
            }
        }

        public static void UnbindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void GenerateMips()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void BindUnit(int unit)
        {
            GL.BindTextureUnit(unit, TextureHandle);
        }

        public static int GetMaxMipmapLevel(int width, int height, int depth)
        {
            return MathF.ILogB(Math.Max(width, Math.Max(height, depth))) + 1;
        }

        public int GetRendererID()
        {
            return TextureHandle;
        }

        public Vector2 GetSize()
        {
            return new Vector2(ImageRef.Width, ImageRef.Height);
        }

        ~Texture()
        {

        }
    }

    public enum FilterTypes
    {
        Linear = 9729,
        Nearest = 9728,
        LinearMipmapLinear = 9987,
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