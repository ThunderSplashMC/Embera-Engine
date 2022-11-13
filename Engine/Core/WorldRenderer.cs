using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Core
{
    class WorldRenderer
    {
        public Skybox skybox;

        public WorldRenderer()
        {
            skybox = new Skybox();
        }

        public void Init()
        {
            Cubemap Cubemap = new Cubemap();
            Cubemap.LoadCubeMap(new string[] {
                "Engine/EngineContent/cubemaps/right.jpg",
                "Engine/EngineContent/cubemaps/left.jpg",
                "Engine/EngineContent/cubemaps/top.jpg",
                "Engine/EngineContent/cubemaps/bottom.jpg",
                "Engine/EngineContent/cubemaps/front.jpg",
                "Engine/EngineContent/cubemaps/back.jpg"
            });
            skybox.SetSkyboxCubemap(Cubemap);
            skybox.Init();
        }
    }
}
