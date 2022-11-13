using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.EngineSandbox
{
    class SandboxApp
    {
        public Application sandboxapp;

        public SandboxApp()
        {
            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                AntiAliasingSamples = 4,
                WindowHeight = 1080,
                WindowWidth = 1920,
                FramesPerSecond = 60,
                Vsync = true,
                WindowTitle = "Balls",
                WindowFullscreen = false,
                workingDir = "D:\\Programming\\DevoidEngine\\Elemental"
            };

            sandboxapp = new Application();
            sandboxapp.Create(ref applicationSpecification);
            sandboxapp.AddLayer(new SandboxLayer());
            sandboxapp.Run();
        }
    }
}
