using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Engine.Core
{
    public static class InputSystem
    {
        public enum MouseButton
        {
            Left,
            Right,
            Middle
        }

        static int KeyDownCode = -1;
        static int KeyUpCode = -1;
        static Vector2 MousePos = new Vector2();
        public static MouseState MouseState;

        static bool MouseLeftDown = false;
        static bool MouseRightDown = false;
        static bool MouseMiddleDown = false;

        public static void SetKeyDown(int keyCode)
        {
            KeyDownCode = keyCode;
        }
        public static void SetKeyUp(int keyCode)
        {
            KeyUpCode = keyCode;
            
        }

        public static void SetMousePosition(Vector2 pos)
        {
            MousePos = pos;
        }

        public static void SetMouseDown(MouseButton mouseButton, bool value)
        {
            if (mouseButton == MouseButton.Left)
            {
                MouseLeftDown = value;
            } else if (mouseButton == MouseButton.Right)
            {
                MouseRightDown = value;
            }
        }

        public static bool GetMouseDown(MouseButton mouseButton)
        {
            return mouseButton == MouseButton.Left ? MouseLeftDown : MouseRightDown;
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            if ((int)keyCode == KeyDownCode)
            {
                return true;
            } return false;
        }

        public static bool GetKeyUp(KeyCode keyCode)
        {
            if ((int)keyCode == KeyUpCode)
            {
                return true;
            }
            return false;
        }

        public static Vector2 GetMousePos()
        {
            return MousePos;
        }

        public static void Update()
        {
            if (MouseState.GetSnapshot().IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
            {
                SetMouseDown(MouseButton.Left, true);
            } else
            {
                SetMouseDown(MouseButton.Left, false);
            }

            if (MouseState.GetSnapshot().IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right))
            {
                SetMouseDown(MouseButton.Right, true);
            } else
            {
                SetMouseDown(MouseButton.Right, false);
            }
        }

        public static void Clear()
        {
            KeyDownCode = -1;
            KeyUpCode = -1;
        }
    }
}
