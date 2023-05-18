using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public class PhysicsDemo : Component
    {

        public override string Type => nameof(PhysicsDemo);

        Mesh mesh;

        public override void OnStart()
        {
            mesh = ((Mesh[])Resources.Load("sphere.fbx"))[0];

            if (gameObject.GetComponent<CameraComponent>() == null )
            {
                gameObject.AddComponent<CameraComponent>();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (InputSystem.GetKeyDown(Utilities.KeyCode.T))
            {
                Spawn();
            }
        }

        void Spawn()
        {
            GameObject physicsObject = gameObject.scene.NewGameObject("PhysicsBullet");

            physicsObject.transform.position = gameObject.transform.position;

            DynamicCollider collider = physicsObject.AddComponent<DynamicCollider>();
        
            CameraComponent camera = gameObject.GetComponent<CameraComponent>();

            collider.ApplyForce(camera.Front * 20);
        }

    }
}
