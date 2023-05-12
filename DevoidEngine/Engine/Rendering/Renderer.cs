using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    public class Renderer
    {
        public static bool isInitialized = false;
        public static DeviceInformation DeviceInformation;

        public static void Init(int width, int height)
        {
            SystemInfo.Validate();
            if (!SystemInfo.GetDeviceInfo().isComputeShaderSupported)
                throw new Exception("Compute shaders are not supported on this device");

            RenderGraph.ViewportHeight = height;
            RenderGraph.ViewportWidth = width;
            RendererUtils.Init();
            Renderer2D.Init(width, height);
            Renderer3D.Init(width, height);
            isInitialized = true;
        }

        public static void Render()
        {
            Renderer3D.Render();
            Renderer2D.Render();

        }

        public static void Resize(int width, int height)
        {
            RenderGraph.ViewportHeight = height;
            RenderGraph.ViewportWidth = width;
            Renderer3D.Resize(width, height);
            Renderer2D.Resize(width, height);
        }

        public static void SetSkybox()
        {

        }

        public static void BlitToScreen()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.BlitNamedFramebuffer(RenderGraph.CompositeBuffer.GetRendererID(), 0, 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, 0, 0, RenderGraph.ViewportWidth, RenderGraph.ViewportHeight, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        }

        public static void ErrorLogging(bool value)
        {
            RendererUtils.ToggleErrorLogging();
        }

        public static void SetCamera(Camera camera)
        {
            RenderGraph.Camera = camera;
        }
    }
}
