using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Rendering
{
    class RenderGraph
    {
        public static int MAX_POINT_LIGHTS = 8;
        public static int MAX_SPOT_LIGHTS = 8;
        public static int MAX_POINT_SHADOW_BUFFERS = 4;

        public static bool BLOOM = true;

        //public static FrameBuffer CompositePass;
        public static Camera Camera;
        public static int ViewportWidth, ViewportHeight;
        public static float time;
        public static RenderMode RenderMode = RenderMode.Normal;
        public static bool EnableLighting;
        public static int Renderer_3D_DrawCalls;

        public static Cubemap SkyboxCubemap;

        public static FrameBuffer LightBuffer;
        public static FrameBuffer GeometryBuffer;
        public static FrameBuffer CompositeBuffer;
        public static FrameBuffer HDRFrameBuffer;

        public static Bloom BloomRenderer;

        public static void Init()
        {

        }
    }
    
    class RenderSettings
    {
        public static bool ErrorCheck = false;
    }

    enum RenderMode
    {
        Normal,
        Wireframe
    }
}
