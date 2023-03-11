using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Rendering
{
    class RendererUtils
    {
        private static DebugProc DebugMessageDelegate = Devoid.OnDebugMessage;
        
        public static VertexArray CubeVAO;
        public static VertexArray QuadVAO;

        public static Shader SkyboxShader;

        public static void Init()
        {
            // Cube Mesh for rendering
            {
                Vertex[] vertices = VERTEX_DEFAULTS.GetCubeVertex();
                VertexBuffer CubeVBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, true);
                CubeVBO.SetData(vertices, vertices.Length);
                CubeVAO = new VertexArray(CubeVBO);
            }

            // FrameBuffer Quad for rendering
            {
                Vertex[] vertices = VERTEX_DEFAULTS.GetFrameBufferVertices();
                VertexBuffer CubeVBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, true);
                CubeVBO.SetData(vertices, vertices.Length);
                CubeVAO = new VertexArray(CubeVBO);
            }

            // Setup Skybox Shader for rendering
            {
                SkyboxShader = new Shader("Engine/EngineContent/shaders/skybox");
                ShaderLibrary.AddShader("skybox_shader_internal", SkyboxShader);
            }
            
        }

        public static void ToggleErrorLogging()
        {
            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            GL.Enable(EnableCap.DebugOutput);
        }

        public static void SetSkybox(Cubemap cubemap)
        {
            RenderGraph.SkyboxCubemap = cubemap;
        }

    }
}
