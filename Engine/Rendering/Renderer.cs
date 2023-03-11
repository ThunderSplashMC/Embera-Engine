using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    class Renderer
    {
        public static bool isInitialized = false;

        public static void Init(int width, int height)
        {
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
