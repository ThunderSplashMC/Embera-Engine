using DevoidEngine.Engine.Windowing;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using DevoidEngine.Engine.Imgui;
using DevoidEngine.Engine.Core;
using Elemental;

namespace DevoidLauncher.Launcher
{
    class Main
    {
        MainLayer mainLayer = new MainLayer();
        ImguiLayer imguiLayer;
        public Window Window;
        public bool EDITOR_STARTED = false;
        public string PROJECT_DIRECTORY = ".";
        public string PROJECT_FILE_PATH;

        public void Initialize()
        {
            WindowSpecification windowSpecification = new WindowSpecification()
            {
                height = 600,
                width = 900,
                fullscreen = false,
                Vsync = true,
                isCentered = true,
                title = "Devoid Launcher",
            };

            Window = new Window(ref windowSpecification);
            Window.Load += OnInit;
            Window.UpdateFrame += Loop;

            mainLayer.main = this;

            try
            {
                Window.Run();
            } catch (Exception ex) 
            {
                Console.WriteLine(ex);
            }
        }

        ElementalApp app;

        public void StartEditor()
        {
            Window.Close();

            app = new ElementalApp();
            app.Init(PROJECT_FILE_PATH);
        }

        public void OnInit()
        {
            imguiLayer = new ImguiLayer();
            imguiLayer.InitIMGUI(Window);
            mainLayer.OnAttach();
        }

        public void Loop(FrameEventArgs time)
        {
            imguiLayer.OnUpdate((float)time.Time);
            imguiLayer.Begin((float)time.Time);
            mainLayer.OnUpdate((float)time.Time);
            mainLayer.GUIRender();
            imguiLayer.End();

            if (EDITOR_STARTED)
            {
                StartEditor();
            }
        }

    }
}
