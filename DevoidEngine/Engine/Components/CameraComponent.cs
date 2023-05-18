using System;
using System.Collections;
using System.Collections.Generic;

using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    public class CameraComponent : Component
    {
        public override string Type { get; } = nameof(CameraComponent);

        public Camera Camera;
        public Color4 ClearColor;

        private CameraProjectionMode previousProjection;
        private float _fovy = MathHelper.DegreesToRadians(45.0f);

        private float nearClip = .1f, farClip = 1000f;

        private int width = 1280, height = 720;
        private float aspectRatio = (float)1280.0f/720;

        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        public CameraProjectionMode CameraProjection = CameraProjectionMode.Perspective;
        
        public enum CameraProjectionMode
        {
            Perspective,
            Orthographic
        }


        public float FOVY {
            get => MathHelper.DegreesToRadians(_fovy);
            set {
                _fovy = MathHelper.DegreesToRadians(value);
            }
        }

        public Vector3 Front
        {
            get => _front;
        }



        public CameraComponent()
        {
            Camera = new Camera();
        }

        public override void OnStart()
        {
            if (CameraProjection == CameraProjectionMode.Perspective)
            {
                SetPerspective();
            } else
            {
                CheckProjection();
            }

            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {
            Camera.position = gameObject.transform.position;
            Camera.SetClearColor(new Vector3(ClearColor.R, ClearColor.G, ClearColor.B));
            UpdateVectors();
            CheckProjection();
            base.OnUpdate(deltaTime);
        }

        private void CheckProjection()
        {
            if (CameraProjection == previousProjection) { return; }
            if (CameraProjection == CameraProjectionMode.Perspective)
            {
                SetPerspective();
            } else
            {
                SetOrthographic();
            }
            previousProjection = CameraProjection;
        }

        private void RecalculateProjection()
        {
            if (CameraProjection == CameraProjectionMode.Perspective)
            {
                SetPerspective();
            } else
            {
                SetOrthographic();
            }
        }

        public void SetViewportSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.aspectRatio = (float)width / height;
            RecalculateProjection();
        }

        private void SetPerspective()
        {
            Camera.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(_fovy, aspectRatio, nearClip, farClip));
        }
        private void SetOrthographic()
        {
            Camera.SetProjectionMatrix(
                Matrix4.CreateOrthographicOffCenter(
                    -5.0f * aspectRatio * 0.5f,
                    5.0f * aspectRatio * 0.5f,
                    -5.0f * 0.5f,
                    5.0f * 0.5f,
                    0.1f,
                    1000f
                )
            );
        }

        private void UpdateVectors()
        {

            float PITCH = MathHelper.DegreesToRadians(gameObject.transform.rotation.Y);
            float YAW = MathHelper.DegreesToRadians(gameObject.transform.rotation.X);

            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(PITCH) * MathF.Cos(YAW);
            _front.Y = MathF.Sin(PITCH);
            _front.Z = MathF.Cos(PITCH) * MathF.Sin(YAW);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));

            Camera.SetViewMatrix(Matrix4.LookAt(gameObject.transform.position, gameObject.transform.position + _front, _up));
        }


    }
}
