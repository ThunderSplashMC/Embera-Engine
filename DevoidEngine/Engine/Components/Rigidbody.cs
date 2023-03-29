using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    public class Rigidbody : Component
    {
        public override string Type { get; } = nameof(Rigidbody);

        public int RigidbodyID;

        public float mass = 0f;
        public Vector3 gravity = new Vector3(0, 9.8f, 0);
        public Vector3 Inertia = new Vector3(0);

        public Physics PhysicsSystem;
        public BulletSharp.RigidBody body;

        public override void OnStart()
        {
            PhysicsSystem = gameObject.scene.PhysicsSystem;
            SetupRigidBody();
            base.OnStart();
        }

        private float prevMass;
        private Vector3 prevGrav;
        private Vector3 prevPosition;

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
            Inertia = new Vector3(body.LocalInertia.X, body.LocalInertia.Y, body.LocalInertia.Z);
            base.OnUpdate(deltaTime);
        }

        public BulletSharp.CollisionShape GetCollisionShape()
        {
            return gameObject.GetComponent<ColliderShape3D>().GetCollisionShape();
        }

        public void ChangeMass()
        {
            gameObject.scene.PhysicsSystem.PhysicsWorld.RemoveRigidBody(body);
            body.SetMassProps(mass, new BulletSharp.Math.Vector3(Inertia.X, Inertia.Y, Inertia.Z));
            gameObject.scene.PhysicsSystem.PhysicsWorld.AddRigidBody(body);
            body.Activate();
        }

        public void ChangeGravity()
        {
            body.Gravity = new BulletSharp.Math.Vector3(gravity.X, gravity.Y, gravity.Z);
        }

        public void SetupRigidBody()
        {
            body = PhysicsSystem.CreateRigidBody(mass, gameObject.transform.GetTransform(), GetCollisionShape());
            body.Gravity = new BulletSharp.Math.Vector3(gravity.X, gravity.Y, gravity.Z);
        }
    }
}
