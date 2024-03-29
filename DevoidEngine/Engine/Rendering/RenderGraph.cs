﻿using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderGraph
    {
        public static int MAX_MESH_COUNT = 100000;

        public static int MAX_POINT_LIGHTS = 8;
        public static int MAX_SPOT_LIGHTS = 8;
        public static int MAX_POINT_SHADOW_BUFFERS = 8;
        public static int MAX_PBR_TEXTURE_PROPS = 4;

        public static bool BLOOM = true;
        public static bool ShadowPass = false;
        public static bool PathTrace = false;

        // 

        public static TonemapperModes TonemapMode;
        public static bool GammeCorrect = true;

        //

        public static float SkyboxIntensity = 1f;

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
        public static FrameBuffer _2DBuffer;
        public static FrameBuffer PreviousFrameBuffer;

        public static RenderPass SSRPass;

        public static Bloom BloomRenderer;

        public static MeshSystem MeshSystem;

        public static void Init()
        {

        }
    }
    
    class RenderSettings
    {
        public static bool ErrorCheck = false;
    }

    public enum TonemapperModes
    {
        ACES,
        Filmic,
        Reinhard,
        Reinhard_Ex,
        Reinhard_Jodie
    }

    public enum RenderMode
    {
        Normal,
        Wireframe
    }
}
