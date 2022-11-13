using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.EngineSandbox
{
    class SphereMover : DevoidScript
    {

        public int Speed = 1;
        public bool move = true;

        public override void OnStart()
        {
            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!move) { return; }
            if (Input.GetKeyDown(KeyCode.W))
                gameObject.transform.position += OpenTK.Mathematics.Vector3.UnitZ * deltaTime * Speed;
            if (Input.GetKeyDown(KeyCode.A))
                //SampleObj2.GetComponent<CameraComponent>().Yaw -= 0.5f;
                gameObject.transform.position -= OpenTK.Mathematics.Vector3.UnitX * deltaTime * Speed;
            if (Input.GetKeyDown(KeyCode.S))
                gameObject.transform.position -= OpenTK.Mathematics.Vector3.UnitZ * deltaTime * Speed;
            if (Input.GetKeyDown(KeyCode.D))
                //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
                gameObject.transform.position += OpenTK.Mathematics.Vector3.UnitX * deltaTime * Speed;
            if (Input.GetKeyDown(KeyCode.Space))
                //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
                gameObject.transform.position += OpenTK.Mathematics.Vector3.UnitY * deltaTime * Speed;

            if (Input.GetKeyDown(KeyCode.LeftShift))
                //SampleObj2.GetComponent<CameraComponent>().Yaw += 0.5f;
                gameObject.transform.position -= OpenTK.Mathematics.Vector3.UnitY * deltaTime * Speed;

            base.OnUpdate(deltaTime);
        }
    }
}
