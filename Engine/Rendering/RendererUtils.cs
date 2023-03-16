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
        public static Shader HDRShader;
        public static Shader FrameBufferCombineShader;
        public static Shader ShadowShader;

        public static void Init()
        {
            // Cube Mesh for rendering
            Vertex[] vertices1 = VERTEX_DEFAULTS.GetCubeVertex();
            VertexBuffer CubeVBO = new VertexBuffer(Vertex.VertexInfo, vertices1.Length, true);
            CubeVBO.SetData(vertices1, vertices1.Length);
            CubeVAO = new VertexArray(CubeVBO);

            // FrameBuffer Quad for rendering
            Vertex[] vertices2 = VERTEX_DEFAULTS.GetFrameBufferVertices();
            VertexBuffer QuadVBO = new VertexBuffer(Vertex.VertexInfo, vertices2.Length, true);
            QuadVBO.SetData(vertices2, vertices2.Length);
            QuadVAO = new VertexArray(QuadVBO);

            // Setup Skybox Shader for rendering
            SkyboxShader = new Shader("Engine/EngineContent/shaders/skybox");
            ShaderLibrary.AddShader("skybox_shader_internal", SkyboxShader);

            // Setup HDR Shader for rendering
            HDRShader = new Shader("Engine/EngineContent/shaders/hdr");
            ShaderLibrary.AddShader("hdr_shader_internal", HDRShader);

            FrameBufferCombineShader = new Shader("Engine/EngineContent/shaders/Framebuffer/combine");
            ShaderLibrary.AddShader("fb_shader_internal", FrameBufferCombineShader);

            ShadowShader = new Shader("Engine/EngineContent/shaders/shadow.vert", "Engine/EngineContent/shaders/shadow.frag", "Engine/EngineContent/shaders/shadow.geom");
            ShaderLibrary.AddShader("shadow_shader_internal", ShadowShader);
        }

        public static void BlitFBToScreen(FrameBuffer srcFB, FrameBuffer destFB)
        {

            //GL.BlitNamedFramebuffer(srcFB.GetRendererID(), destFB.GetRendererID(), 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            destFB.Bind();

            FrameBufferCombineShader.Use();

            FrameBufferCombineShader.SetInt("S_SOURCE_TEXTURE", 0);
            FrameBufferCombineShader.SetInt("S_SCREEN_TEXTURE", 1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, srcFB.GetColorAttachment(0));

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, destFB.GetColorAttachment(0));

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            QuadVAO.Render();


            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            destFB.UnBind();

        }

        public static void Cull(bool value)
        {
            if (value)
                GL.Enable(EnableCap.CullFace);
            else
                GL.Disable(EnableCap.CullFace);
        }

        public static void DepthTest(bool value)
        {
            if (value)
                GL.Enable(EnableCap.DepthTest);
            else
                GL.Disable(EnableCap.DepthTest);
        }

        public static void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
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
