using System;
using DevoidEngine.Engine;
using DevoidEngine.Engine.Core;
using OpenTK.Windowing.Common;

namespace DevoidEngine
{

    // start here
    class Game : Layer
    {
        public override void OnAttach()
        {

        }

        public override void OnDetach()
        {

        }

        public override void OnUpdate(float deltaTime)
        {

        }

        public override void OnRender()
        {
#if STANDALONE_MODE

            Engine.Rendering.Renderer.BlitToScreen();

#endif
        }

        public override void OnResize(int width, int height)
        {

        }

        public override void KeyDown(KeyboardKeyEventArgs keyboardevent)
        {

        }
    }
}
