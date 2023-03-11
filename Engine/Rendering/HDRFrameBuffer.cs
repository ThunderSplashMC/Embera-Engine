using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    class HDRFrameBuffer
    {
        public FrameBuffer HDRFB;
        VertexArray FrameBufferPlane;
        Shader HDRFBShader;
        public void Init(int width, int height)
        {
            HDRFB = new FrameBuffer(new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F}
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            });

            HDRFBShader = new Shader("Engine/EngineContent/shaders/hdr");

            Vertex[] vertices = VERTEX_DEFAULTS.GetFrameBufferVertices();
            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            FrameBufferPlane = new VertexArray(vertexBuffer);
        }

        public void StartRender()
        {
            HDRFB.Bind();
        }

        public void EndRender()
        {
            HDRFB.UnBind();
        }

        public void Resize(int width, int height)
        {
            HDRFB.Resize(width, height);
        }
    }
}
