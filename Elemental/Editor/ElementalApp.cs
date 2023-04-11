using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using System.IO;
using Elemental.Editor.EditorUtils;

namespace Elemental
{
    public class ElementalApp
    {

        public ElementalApp()
        {

        }




        public void Init(string PROJECT_FILE_PATH)
        {
            ProjectUtils projectUtils = new ProjectUtils();

            projectUtils.LoadFile(PROJECT_FILE_PATH);

            string Path = projectUtils.GetProjectBasePath();


            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                AntiAliasingSamples = 16,
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
            layer.PROJECT_NAME = projectUtils.GetProjectName();
            layer.PROJECT_ASSET_DIR = Path + "\\Assets";
            
            Application.AddLayer(layer);
            Application.Run();
        }
    }
}
