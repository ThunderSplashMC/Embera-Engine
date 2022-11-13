using System;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;
using OpenTK.Mathematics;

using ImGuiNET;

using DevoidEngine.Engine.Utilities;
using System.ComponentModel;

namespace DevoidEngine.Engine.Windowing
{

    public struct WindowSpecification
    {
        public string title;
        public int width, height;
        public int samples;
        public bool fullscreen;
        public bool Vsync;
        public bool isCentered;
        public int FramesPerSecond;
        public string iconPath;
    }

    public class Window : GameWindow
    {
        private WindowSpecification WindowSpec;

        public Window(ref WindowSpecification windowSpec) : base(GameWindowSettings.Default,
            new NativeWindowSettings() {
                NumberOfSamples = windowSpec.samples,
                Size = new Vector2i(windowSpec.width, windowSpec.height),
                Title = windowSpec.title,
                StartVisible = false
            })
        {
            WindowSpec = windowSpec;
            this.VSync = windowSpec.Vsync ? VSyncMode.Adaptive : VSyncMode.Off;


        }

        protected override void OnLoad()
        {
            if (WindowSpec.isCentered)
                this.CenterWindow();

            if (WindowSpec.fullscreen)
                this.WindowState = WindowState.Fullscreen;
            
            if (WindowSpec.iconPath != null)
            {
                Utilities.Image iconImg = new Utilities.Image();
                iconImg.LoadImageAlpha(WindowSpec.iconPath);
                Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(iconImg.Width, iconImg.Height, iconImg.Pixels));
            }


            this.IsVisible = true;
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (this.IsExiting || closing) { return; }
            SwapBuffers();
            base.OnRenderFrame(args);
        }

        bool closing = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            closing = true;
            base.OnClosing(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        ~Window()
        {
            this.Dispose(true);
        }
    }
}
