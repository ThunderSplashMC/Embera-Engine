﻿using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Utilities;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Rendering
{
    public class RendererUtils
    {
        private static DebugProc DebugMessageDelegate = OnDebugMessage;
        
        public static VertexArray CubeVAO;
        public static VertexArray QuadVAO;
        public static VertexArray SphereVAO;

        public static Shader SkyboxShader;
        public static Shader HDRShader;
        public static Shader FrameBufferCombineShader;
        public static Shader ShadowShader;
        public static Shader GeometryShader;
        public static Shader DepthPrePassShader;
        public static Shader BasicShader;
        public static Shader PBRShader;

        public static string ErrorInfo = string.Empty;

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
            SkyboxShader = new Shader("Engine/EngineContent/shaders/Skybox/skybox");
            ShaderLibrary.AddShader("skybox_shader_internal", SkyboxShader);

            // Setup HDR Shader for rendering
            HDRShader = new Shader("Engine/EngineContent/shaders/HDR/hdr");
            ShaderLibrary.AddShader("hdr_shader_internal", HDRShader);

            FrameBufferCombineShader = new Shader("Engine/EngineContent/shaders/Framebuffer/combine");
            ShaderLibrary.AddShader("fb_shader_internal", FrameBufferCombineShader);

            ShadowShader = new Shader("Engine/EngineContent/shaders/Shadows/shadow.vert", "Engine/EngineContent/shaders/Shadows/shadow.frag", "Engine/EngineContent/shaders/Shadows/shadow.geom");
            ShaderLibrary.AddShader("shadow_shader_internal", ShadowShader);

            GeometryShader = new Shader("Engine/EngineContent/shaders/GBuffer/geometry");
            ShaderLibrary.AddShader("gbuffer_shader_internal", GeometryShader);

            DepthPrePassShader = new Shader("Engine/EngineContent/shaders/DepthPrePass/depthprepass");
            ShaderLibrary.AddShader("depth_prepass_shader_internal", DepthPrePassShader);

            BasicShader = new Shader("Engine/EngineContent/shaders/experimental/basic");
            ShaderLibrary.AddShader("basic_shader_internal", BasicShader);

            PBRShader = new Shader("Engine/EngineContent/shaders/PBR/pbr");
            ShaderLibrary.AddShader("pbr_shader_internal", PBRShader);
        }

        public static void BlitFBToScreen(FrameBuffer srcFB, FrameBuffer destFB, float opacity = 0.5f, bool additive = false, int unit = 0)
        {
            destFB.Bind();

            if (opacity == 1.0f)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }

            FrameBufferCombineShader.Use();

            FrameBufferCombineShader.SetInt("S_SOURCE_TEXTURE", 0);
            FrameBufferCombineShader.SetInt("S_SCREEN_TEXTURE", 1);
            FrameBufferCombineShader.SetFloat("OPACITY_MIX", opacity);
            FrameBufferCombineShader.SetBool("ADDITIVE_BLEND", additive);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, srcFB.GetColorAttachment(unit));

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

        public static void SkyboxIntensity(float intensity)
        {
            RenderGraph.SkyboxIntensity = intensity;
        }

        public static void OnDebugMessage(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.

            // Potentially, you may want to throw from the function for certain severity
            // messages.
            if (type == DebugType.DebugTypeError)
            {
                ErrorInfo = "ERR: " + message;

                //Console.WriteLine(" ");
                //Console.WriteLine("##################");
                //Console.WriteLine("ERR: " + message);
                //Console.WriteLine("SOURCE: " + source);
                //Console.WriteLine("TYPE: " + type);
                //Console.WriteLine("ID: " + id);
                //Console.WriteLine("##################");
                //Console.WriteLine(" ");
                //Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
            }
            else if (type == DebugType.DebugTypeDeprecatedBehavior)
            {
                ErrorInfo = "DEPRECATED: " + message;
            }
            else if (type == DebugType.DebugTypePerformance)
            {
                ErrorInfo = "PERFORMANCE ERR: " + message;
            }
            else if (type == DebugType.DebugTypeOther)
            {
                ErrorInfo = "OTHER ERR: " + message;
            }
            else if (type == DebugType.DontCare)
            {
                ErrorInfo = "DONT CARE: " + message;
            }
        }

    }
}
