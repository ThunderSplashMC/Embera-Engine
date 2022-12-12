using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Input;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Windowing;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Imgui;

namespace DevoidEngine.Engine.Core
{

    public struct ApplicationSpecification
    {
        public string WindowTitle;
        public int WindowWidth, WindowHeight;
        public bool WindowFullscreen;
        public bool Vsync;
        public int AntiAliasingSamples;
        public int FramesPerSecond;
        public string workingDir;
        public bool enableImGui;
        public string iconPath;
    }

    class Application
    {
        private ApplicationSpecification ApplicationSpecification;
        private Window Window;

        private LayerManager LayerManager = new LayerManager();
        private ImguiLayer? ImguiLayer;

        public void Create(ref ApplicationSpecification specification)
        {

            this.ApplicationSpecification = specification;
            WindowSpecification windowSpecification = new WindowSpecification();
            windowSpecification.title = specification.WindowTitle;
            windowSpecification.height = specification.WindowHeight;
            windowSpecification.width = specification.WindowWidth;
            windowSpecification.samples = specification.AntiAliasingSamples;
            windowSpecification.Vsync = specification.Vsync;
            windowSpecification.FramesPerSecond = specification.FramesPerSecond;
            windowSpecification.fullscreen = specification.WindowFullscreen;
            windowSpecification.isCentered = true;
            windowSpecification.iconPath = ApplicationSpecification.iconPath;
            this.Window = new Window(ref windowSpecification);

            FileSystem.SetBasePath(ApplicationSpecification.workingDir);

            Renderer.Init(ApplicationSpecification.WindowWidth, ApplicationSpecification.WindowHeight);
            if (ApplicationSpecification.enableImGui)
            {
                ImguiLayer = new ImguiLayer();
                ImguiLayer.InitIMGUI(Window);
            }
            
            // ==== BINDING EVENTS ====

            Input.KeyboardState = Window.KeyboardState;
            Input.MouseState = Window.MouseState;

            
            Window.Load += OnLoad;
            Window.Unload += OnUnload;
            Window.Resize += OnResize;
            Window.UpdateFrame += OnUpdateFrame;
            Window.RenderFrame += OnRenderFrame;
            Window.KeyDown += OnKeyDown;

            // ==== BINDING EVENTS END ====
        }

        public void AddLayer(Layer layer)
        {
            layer.Application = this;
            LayerManager.AddLayer(layer);
        }

        public T GetLayer<T>() where T : Layer, new()
        {
            return LayerManager.GetLayer<T>();
        }

        public void RemoveLayer(Layer layer)
        {
            LayerManager.RemoveLayer(layer);
        }

        public void Run()
        {
            //AddLayer(ImguiLayer);

            Window.Run();
        }

        public Vector2 GetWindowSize()
        {
            return Window.Size;
        }

        public void Close()
        {
            Window.Close();
            Window.Dispose();
        }

        public void OnLoad()
        {

        }

        public void OnUnload()
        {

        }

        public void OnUpdateFrame(FrameEventArgs args)
        {
            float dt = (float)args.Time;
            LayerManager.UpdateLayers(dt);
            if (ApplicationSpecification.enableImGui)
            {
                ImguiLayer.Begin(dt);

                for (int i = 0; i < LayerManager.Layers.Count; i++)
                {
                    Layer layer = LayerManager.Layers[i];
                    layer.GUIRender();
                }

                ImguiLayer.End();
            }
        }

        public void OnRenderFrame(FrameEventArgs args)
        {
            RenderGraph.time += (float)args.Time;
            LayerManager.RenderLayers();
            Renderer.Render();
            LayerManager.LateRenderLayers();
        }

        public void OnResize(ResizeEventArgs args)
        {
            Renderer.Resize(args.Width, args.Height);
            LayerManager.ResizeLayers(args.Width, args.Height);
        }

        public void OnKeyDown(KeyboardKeyEventArgs args)
        {
            LayerManager.KeyDownLayers(args);
        }
    }
}
