using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;
using Evergine.Bindings.Imguizmo;

namespace DevoidEngine.Engine.GUI
{
    
    class DebugRenderer
    {
        
        public static void DrawManipulate(Camera camera, Matrix4 matrix)
        {
            
            unsafe
            {
                Matrix4 Viewpointer = camera.GetViewMatrix();
                Matrix4 Projectionpointer = camera.GetProjectionMatrix();
                Matrix4 matrixpointer = matrix;
                float* Viewptr = (float*)&Viewpointer;
                float* Projectionptr = (float*)&Projectionpointer;
                float* matrixptr = (float*)&matrixpointer;
                ImguizmoNative.ImGuizmo_Manipulate(Viewptr, Projectionptr, OPERATION.TRANSLATE, MODE.WORLD, matrixptr, null, null, null, null);
            }
        }
    }
}
