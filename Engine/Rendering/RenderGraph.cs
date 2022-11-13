using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    class RenderGraph
    {
        public static FrameBuffer CompositePass;
        public static Camera Camera;
        public static int ViewportWidth, ViewportHeight;
        public static float time;
        public static RenderMode RenderMode = RenderMode.Wireframe;

        public static void Init()
        {

        }
    }

    enum RenderMode
    {
        Normal,
        Wireframe
    }
}
