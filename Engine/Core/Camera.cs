using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Core
{
    class Camera
    {
        public bool Main;
        public Vector3 position;
        private Matrix4 projection;
        private Matrix4 ViewMatrix;

        public void SetProjectionMatrix(Matrix4 projectionMatrix)
        {
            projection = projectionMatrix;
        }

        public Matrix4 GetProjectionMatrix()
        {
            return projection;
        }

        public void SetViewMatrix(Matrix4 viewMatrix)
        {
            ViewMatrix = viewMatrix;
        }

        public Matrix4 GetViewMatrix()
        {
            return ViewMatrix;
        }
    }
}
