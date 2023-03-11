using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Engine.Core
{
    class EditorCamera
    {
        private Vector2 initialMousePos;
        private Vector2 initialMouseScroll;
        private float Distance;
        private Vector3 FocalPoint;
        private float pitch = MathHelper.DegreesToRadians(0.0f), yaw = MathHelper.DegreesToRadians(180.0f);
        private float Fovy;
        private int width, height;
        private float farClip, nearClip;
        public Camera Camera;
        public float speed = 2.0f;
        public bool IsSelected = false;
        public int projectionType = 0;
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(Fovy);
            set {

                Fovy = MathHelper.DegreesToRadians(value);
                UpdateProjection();
            }
        }


        public EditorCamera(float fov, int width, int height, float farClip, float nearClip)
        {

            this.width = width;
            this.height = height;
            this.farClip = farClip;
            this.nearClip = nearClip;
            this.Fovy = fov;
            this.Camera = new Camera();
            UpdateView();
        }

        public void SetPitch(float degrees)
        {
            pitch = MathHelper.DegreesToRadians(degrees);
        }

        public void SetYaw(float degrees)
        {
            yaw = MathHelper.DegreesToRadians(degrees);
        }

        public void SetFov(float degrees)
        {
            Fovy = MathHelper.DegreesToRadians(degrees);
        }

        public void Update(float deltaTime)
        {
            
            if (Input.GetKeyDown(KeyCode.LeftAlt) || IsSelected)
            {
                Vector2 mousePos = Input.GetMousePos();
                Vector2 delta = (mousePos - initialMousePos) * 0.03f;
                initialMousePos = mousePos;
                if (Input.IsMouseButtonPressed(MouseButton.Middle))
                {
                    MousePan(delta);
                }
                if (Input.MouseState.ScrollDelta != Vector2.Zero)
                {
                    MouseZoom(Input.MouseState.ScrollDelta.Y);
                }
                if (Input.IsMouseButtonPressed(MouseButton.Button2) && !Input.GetKeyDown(KeyCode.LeftShift))
                {
                    MouseRotate(delta);
                }
                if (Input.IsMouseButtonPressed(MouseButton.Button2) && Input.GetKeyDown(KeyCode.LeftShift))
                {
                    MouseZoom(delta.X);
                }

                //if (Input.GetKeyDown(KeyCode.W))
                //    Camera.position += GetFrontVector() * deltaTime * speed;
                //if (Input.GetKeyDown(KeyCode.A))
                //    //SampleObj2.GetComponent<CameraComponent>().Yaw -= 0.5f;
                //    Camera.position -= GetRightVector() * deltaTime * speed;
                //if (Input.GetKeyDown(KeyCode.S))
                //    Camera.position -= GetFrontVector() * deltaTime * speed;
                //if (Input.GetKeyDown(KeyCode.D))
                //    //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
                //    Camera.position += GetRightVector() * deltaTime * speed;

                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    speed = 10f;
                } else { speed = 1f; }
            } else
            {
                initialMousePos = Input.GetMousePos();
            }
            UpdateView();
        }

        public void SetViewportSize(int width, int height)
        {
            this.width = width;
            this.height = height;
            UpdateProjection();
        }

        public void UpdateProjection()
        {
            if (projectionType == 0)
            {
                Camera.SetProjectionMatrix(
               Matrix4.CreatePerspectiveFieldOfView(Fovy, (float)width / height, nearClip, farClip)
                );
            } else
            {
                float aspectRatio = (float)width / height;
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
        }

        public Vector3 GetFrontVector()
        {
            Vector3 _front;

            _front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            _front.Y = MathF.Sin(pitch);
            _front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            return _front;
        }

        public Vector3 GetPosition()
        {
            return FocalPoint - GetFrontVector() * Distance;
        }

        public Vector3 GetRightVector()
        {
            return Vector3.Normalize(Vector3.Cross(GetFrontVector(), Vector3.UnitY));
        }
        
        public Vector3 GetUpVector()
        {
            return Vector3.Normalize(Vector3.Cross(GetRightVector(), GetFrontVector()));
        }

        public void UpdateView()
        {
            Camera.SetViewMatrix(Matrix4.LookAt(Camera.position, Camera.position + GetFrontVector(), GetUpVector()));
        }

        public Quaternion GetOrientation()
        {
            
            return new Quaternion(new Vector3(-pitch, yaw, 0.0f));
        }

        public float[] PanSpeed()
        {
            float x = MathF.Min(width / 1000f, 2.4f);
            float xFactor = 0.0366f * (x * x) - 0.1778f * x + 0.3021f;

            float y = MathF.Min(width / 1000.0f, 2.4f); // max = 2.4f
            float yFactor = 0.0366f * (y * y) - 0.1778f * y + 0.3021f;

            return new float[] { xFactor, yFactor };
        }

        public void MousePan(Vector2 delta)
	    {
            Camera.position += GetRightVector() * delta.X * speed;
            Camera.position += GetUpVector() * delta.Y * speed;
	    }

        public void MouseRotate(Vector2 delta)
	    {
		    float yawSign = GetUpVector().Y < 0 ? -1.0f : 1.0f;
            yaw -= yawSign * delta.X * RotationSpeed();
            pitch -= delta.Y * RotationSpeed();
        }

        public void MouseZoom(float delta)
        {
            //Distance -= delta * ZoomSpeed();
            //if (Distance < 1.0f)
            //{
            //    FocalPoint += GetFrontVector();
            //    Distance = 1.0f;
            //}
            Camera.position += GetFrontVector() * speed * delta;
        }

        float RotationSpeed()
	    {
		    return 0.8f;
        }

        float ZoomSpeed()
	    {
		    float distance = Distance * 0.2f;
            distance = MathF.Min(distance, 0.0f);
		    float speed = distance * distance;
            speed = MathF.Min(speed, 100.0f); // max speed = 100
		    return speed;
	    }


    }
}
