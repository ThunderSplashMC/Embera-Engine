using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Serializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DevoidPlayer.Player
{
    internal class Main
    {


        Application Application;

        public Main()
        {
            SetupApplication();



        }

        public void SetupApplication()
        {
            Application = new Application();

            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                AntiAliasingSamples = 16,
                WindowHeight = 1920,
                WindowWidth = 1080,
                FramesPerSecond = 60,
                Vsync = true,
                WindowTitle = "Devoid Export",
                WindowFullscreen = false,
                workingDir = AppContext.BaseDirectory,
                enableImGui = false
            };

            Application.Create(ref applicationSpecification);
            Application.AddLayer(new GameLayer());
            Application.Run();
        }

    }
}
