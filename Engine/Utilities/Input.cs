using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Engine.Utilities
{
    public static class Input
    {
        public static KeyboardState KeyboardState;
        public static MouseState MouseState;

        public static bool GetKeyDown(KeyCode key)
        {
            return KeyboardState.IsKeyDown((Keys)key);
        }

        public static Vector2 GetMousePos()
        {
            return MouseState.Position;
        }

        public static Vector2 GetMouseMoveDelta()
        {
            return MouseState.Delta;
        }

        public static bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return MouseState.IsButtonDown(mouseButton);
        }
    }
}
