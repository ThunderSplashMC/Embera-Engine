using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Elemental
{
    class ElementalApp
    {
        public ElementalApp()
        {
            Init();
        }

        public ElementalApp(string path)
        {
            Init(path);
        }

        public void Init(string Path = "C:\\")
        {
            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                AntiAliasingSamples = 4,
                WindowHeight = 1080,
                WindowWidth = 1920,
                FramesPerSecond = 60,
                Vsync = true,
                WindowTitle = "Elemental Editor",
                WindowFullscreen = false,
                workingDir = Path,
                enableImGui = true,
                iconPath = "Engine/EngineContent/icons/icon64-stroke.png",
            };

            Application Application = new Application();
            Application.Create(ref applicationSpecification);
            Application.AddLayer(new EditorLayer());
            Application.Run();
        }
    }
}
