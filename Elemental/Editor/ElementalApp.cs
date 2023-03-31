using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using System.IO;

namespace Elemental
{
    public class ElementalApp
    {

        public ElementalApp(string path)
        {
            Init(path);
        }

        public void Init(string Path)
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
                iconPath = System.IO.Path.Join("Engine/EngineContent/icons/icon64-stroke.png"),
            };

            Application Application = new Application();
            Application.Create(ref applicationSpecification);
            EditorLayer layer = new EditorLayer();
            layer.PROJECT_DIRECTORY = Path;
            Application.AddLayer(layer);
            Application.Run();
        }
    }
}
