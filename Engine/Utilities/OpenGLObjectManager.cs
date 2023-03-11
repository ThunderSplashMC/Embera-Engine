using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Utilities
{
    static class OpenGLObjectManager
    {

        static List<int> d_VertexArrays = new List<int>();

        static List<int> d_VertexBuffers = new List<int>();

        static List<int> d_FrameBuffers = new List<int>();

        public static void AddVAOToDispose(int VAO)
        {
            d_VertexArrays.Add(VAO);
        }

        public static void AddVBOToDispose(int VBO)
        {
            d_VertexBuffers.Add(VBO);
        }

        public static void AddFBToDispose(int FBO)
        {
            d_FrameBuffers.Add(FBO);
        }

        public static void Dispose()
        {
            for (int i = 0; i < d_VertexArrays.Count; i++)
            {
                GL.BindVertexArray(0);
                GL.DeleteVertexArray(d_VertexArrays[i]);
            }
            for (int i = 0; i < d_VertexBuffers.Count; i++)
            {
                if (d_VertexBuffers[i] == 1) continue;
                GL.DeleteBuffer(d_VertexBuffers[i]);
            }
            for (int i = 0; i < d_FrameBuffers.Count; i++)
            {
                if (d_VertexBuffers[i] == 1) continue;
                GL.DeleteBuffer(d_VertexBuffers[i]);
            }
            d_VertexArrays.Clear();
            d_VertexBuffers.Clear();
            d_FrameBuffers.Clear();
        }
    }
}
