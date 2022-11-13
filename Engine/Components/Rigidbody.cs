using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class Rigidbody : Component
    {
        public int RigidbodyID;

        public float mass = 0f;
        public float gravity = 10f;

        public Physics PhysicsSystem;
        public BulletSharp.RigidBody body;

        public override void OnStart()
        {
            PhysicsSystem = gameObject.scene.PhysicsSystem;
            SetupRigidBody();
            base.OnStart();
        }

        private float prevMass, prevGrav;

        public override void OnUpdate(float deltaTime)
        {
            if (prevMass != mass)
            {
                ChangeMass();
                prevMass = mass;
            }
            if (prevGrav != gravity)
            {
                ChangeGravity();
                prevGrav = gravity;
            }
            base.OnUpdate(deltaTime);
        }

        public BulletSharp.CollisionShape GetCollisionShape()
        {
            return gameObject.GetComponent<BoxCollider>().Shape;
        }

        public void ChangeMass()
        {
            gameObject.scene.PhysicsSystem.PhysicsWorld.RemoveRigidBody(body);
            body.SetMassProps(mass, body.CollisionShape.CalculateLocalInertia(mass));
            gameObject.scene.PhysicsSystem.PhysicsWorld.AddRigidBody(body);
            body.Activate();
        }

        public void ChangeGravity()
        {
            //body.Gravity = new BulletSharp.Math.Vector3(0, gravity, 0);
        }

        public void SetupRigidBody()
        {
            body = PhysicsSystem.CreateRigidBody(mass, gameObject.transform.GetTransform(), GetCollisionShape());
            body.Gravity = new BulletSharp.Math.Vector3(0, gravity,0);
        }
    }
}
