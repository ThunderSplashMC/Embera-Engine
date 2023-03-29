using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Utilities
{
    public class Cubemap
    {

        public int CubeMapTex;
        public Core.Texture CubeMapTexture;

        public void LoadCubeMap(string[] faces)
        {
            CubeMapTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, CubeMapTex);

            for (int i = 0; i < faces.Length; i++)
            {
                Image data = new Image(faces[i]);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgb, data.Width, data.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, data.Pixels);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (float)TextureWrapMode.ClampToEdge);

            CubeMapTexture = new Core.Texture(CubeMapTex);
        }
    }
}
