using System;
using System.Collections.Generic;

using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Components
{
    public class BasicMovement : Component
    {
        public override string Type { get; } = nameof(BasicMovement);

        Vector2 _lastPos;
        bool _firstMove = true;
        CameraComponent cameraComponent;

        public BasicMovement()
        {

        }

        public override void OnStart()
        {

            cameraComponent = gameObject.GetComponent<CameraComponent>();
            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {
            //if (Input.GetKeyDown(KeyCode.W))
            //    gameObject.transform.position += cameraComponent._front * deltaTime;
            //if (Input.GetKeyDown(KeyCode.A))
            //    //SampleObj2.GetComponent<CameraComponent>().Yaw -= 0.5f;
            //    gameObject.transform.position -= cameraComponent._right * deltaTime;
            //if (Input.GetKeyDown(KeyCode.S))
            //    gameObject.transform.position -= cameraComponent._front * deltaTime;
            //if (Input.GetKeyDown(KeyCode.D))
            //    //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
            //    gameObject.transform.position += cameraComponent._right * deltaTime;
            //if (Input.GetKeyDown(KeyCode.Space))
            //    //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
            //    gameObject.transform.position += Vector3.UnitY * deltaTime;

            if (Input.GetKeyDown(KeyCode.LeftShift))
                //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
                gameObject.transform.position -= Vector3.UnitY * deltaTime;

            if (_firstMove)
            {
                _lastPos = new Vector2(Input.MouseState.X, Input.MouseState.Y);
                _firstMove = false;
            }
            else
            {


                var deltaX = Input.MouseState.X - _lastPos.X;
                var deltaY = Input.MouseState.Y - _lastPos.Y;
                _lastPos = new Vector2(Input.MouseState.X, Input.MouseState.Y);

                gameObject.transform.rotation.X += deltaX * 0.2f;
                gameObject.transform.rotation.Y -= deltaY * 0.2f;
            }
        }
    }
}
