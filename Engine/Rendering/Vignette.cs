using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    class Vignette
    {
        FrameBuffer VignetteFrameBuffer;
        Shader VignetteShader;
        VertexArray Quad;
        int width, height;

        public void Init(int width, int height)
        {
            this.width = width;
            this.height = height;
            FrameBufferSpecification FBS = new FrameBufferSpecification()
            {
                height = height,
                width = width,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F}
                }
            };

            VignetteFrameBuffer = new FrameBuffer(FBS);
            VignetteShader = new Shader("Engine/EngineContent/shaders/vignette");

            Vertex[] vertices = VERTEX_DEFAULTS.GetFrameBufferVertices();
            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            Quad = new VertexArray(vertexBuffer);
        }

        public void DoPass(int srcTex)
        {
            VignetteFrameBuffer.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            VignetteShader.Use();
            VignetteShader.SetInt("S_RENDERED_TEXTURE", 0);
            VignetteShader.SetVector2("S_RESOLUTION", new OpenTK.Mathematics.Vector2(width, height));


            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, srcTex);

            Quad.Render();

            VignetteFrameBuffer.UnBind();
        }

        public int GetVignetteTexture()
        {
            return VignetteFrameBuffer.GetColorAttachment(0);
        }

        public void Resize(int width, int height)
        {
            VignetteFrameBuffer.Resize(width, height);
            this.width = width;
            this.height = height;
        }
    }
}
