using System;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Mathematics;
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
                StartVisible = false,
                APIVersion = new Version(4,3),
                Flags = ContextFlags.Debug
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
                Icon = new WindowIcon(new Image(iconImg.Width, iconImg.Height, iconImg.Pixels));
            }

            this.IsVisible = true;
            base.OnLoad();
        }

        protected override void OnUnload()
        {
            base.Close();
            Dispose(true);
            
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }

        public override void Close()
        {
            base.Close();
        }

        ~Window()
        {
            GC.SuppressFinalize(this);
        }
    }
}
