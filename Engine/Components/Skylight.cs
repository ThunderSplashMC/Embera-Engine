using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class Skylight : Component
    {
        public override string Type { get; } = nameof(Skylight);

        public float Intensity = 1f;

        private Cubemap Cubemap;
        private bool loaded;

        public override void OnStart()
        {
            if (loaded) { return; }
            Cubemap = new Cubemap();
            Cubemap.LoadCubeMap(new string[] {
                "Engine/EngineContent/cubemaps/right.jpg",
                "Engine/EngineContent/cubemaps/left.jpg",
                "Engine/EngineContent/cubemaps/top.jpg",
                "Engine/EngineContent/cubemaps/bottom.jpg",
                "Engine/EngineContent/cubemaps/front.jpg",
                "Engine/EngineContent/cubemaps/back.jpg"
            });
            //Renderer3D.GetSkybox().SetSkyboxCubemap(Cubemap);
            loaded = true;

            RendererUtils.SetSkybox(Cubemap);

            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {
            RendererUtils.SkyboxIntensity(Intensity);

            base.OnUpdate(deltaTime);
        }

    }
}
