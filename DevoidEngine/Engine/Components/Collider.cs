using BepuPhysics;
using BepuPhysics.Collidables;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public enum ColliderType
    {
        Box,
        Sphere,
        Convex
    }

    public class DynamicCollider : Component
    {
        public override string Type => nameof(DynamicCollider);

        public ColliderType ColliderType;

        public float mass = 1; // KG

        private ColliderType prevColliderType; // This is used because the engine's editor currently does not support properties in the inspector. (Fields only!)
                                               // I do know that i shouldnt compromise on optimizing code due to the editor, since its an isolated project, but its just one of those lazy days yknow.
        bool isInitialized;
        BodyReference colliderRef;
        TypedIndex shapeIndex;

        public override void OnStart()
        {
            if (isInitialized)
            {
                Destroy();
                InitializeCollider();
            }
            else
            {
                InitializeCollider();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (ColliderType != prevColliderType)
            {
                prevColliderType = ColliderType;
                Destroy();
                InitializeCollider();
            }

            gameObject.transform.position = Physics.NumericsToVector3(colliderRef.Pose.Position);
            System.Numerics.Vector3 rot = PhysicsHelper.ToEulerAngles(colliderRef.Pose.Orientation);
            gameObject.transform.rotation = Physics.NumericsToVector3(rot);

            if (InputSystem.GetKeyDown(Utilities.KeyCode.Space))
            {
                ApplyForce(new Vector3(0, 10, 0));
            }

            if (InputSystem.GetKeyDown(Utilities.KeyCode.A))
            {
                ApplyForce(new Vector3(-10, 0, 0));
            }

            if (InputSystem.GetKeyDown(Utilities.KeyCode.D))
            {
                ApplyForce(new Vector3(10, 0, 0));
            }
        }

        void InitializeCollider()
        {
            if (ColliderType == ColliderType.Box)
            {
                Box shape = new Box(1,1,1);

                shapeIndex = gameObject.scene.PhysicsSystem.AddCollider(shape);
                colliderRef = gameObject.scene.PhysicsSystem.AddDynamicBody(gameObject.transform.position, gameObject.transform.rotation, shape.ComputeInertia(mass), shapeIndex);
            } else if (ColliderType == ColliderType.Sphere)
            {
                Sphere shape = new Sphere(1);

                shapeIndex = gameObject.scene.PhysicsSystem.AddCollider(shape);
                colliderRef = gameObject.scene.PhysicsSystem.AddDynamicBody(gameObject.transform.position, gameObject.transform.rotation, shape.ComputeInertia(mass), shapeIndex);
            }
            isInitialized = true;
        }

        void Destroy()
        {
            gameObject.scene.PhysicsSystem.RemoveBody(colliderRef);
            gameObject.scene.PhysicsSystem.RemoveCollider(shapeIndex);
        }

        public void ApplyForce(Vector3 value)
        {
            colliderRef.ApplyImpulse(Physics.Vector3ToNumerics(value), System.Numerics.Vector3.Zero);
        }


    }

    public class StaticCollider : Component
    {
        public override string Type => nameof(StaticCollider);

        public ColliderType ColliderType;
        public Vector3 BoxSize = new Vector3(1, 1, 1);
        public float SphereSize = 1f;

        private ColliderType prevColliderType; // This is used because the engine's editor currently does not support properties in the inspector. (Fields only!)
                                               // I do know that i shouldnt compromise on optimizing code due to the editor, since its an isolated project, but its just one of those lazy days yknow.
        bool isInitialized;
        StaticReference colliderRef;
        TypedIndex shapeIndex;

        public override void OnStart()
        {
            if (isInitialized)
            {
                Destroy();
                InitializeCollider();
            }
            else
            {
                InitializeCollider();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (ColliderType != prevColliderType)
            {
                prevColliderType = ColliderType;
                Destroy();
                InitializeCollider();
            }

            colliderRef.Pose.Position = Physics.Vector3ToNumerics(gameObject.transform.position);
            colliderRef.Pose.Orientation = PhysicsHelper.ToQuaternion(Physics.Vector3ToNumerics(gameObject.transform.rotation));

        }

        void InitializeCollider()
        {
            if (ColliderType == ColliderType.Box)
            {
                Box shape = new Box(BoxSize.X, BoxSize.Y, BoxSize.Z);

                shapeIndex = gameObject.scene.PhysicsSystem.AddCollider(shape);
                colliderRef = gameObject.scene.PhysicsSystem.AddStaticBody(gameObject.transform.position, gameObject.transform.rotation, shapeIndex);
            }
            else if (ColliderType == ColliderType.Sphere)
            {
                Sphere shape = new Sphere(SphereSize);

                shapeIndex = gameObject.scene.PhysicsSystem.AddCollider(shape);
                colliderRef = gameObject.scene.PhysicsSystem.AddStaticBody(gameObject.transform.position, gameObject.transform.rotation, shapeIndex);
            }
            isInitialized = true;
        }

        void Destroy()
        {
            gameObject.scene.PhysicsSystem.RemoveStatic(colliderRef);
            gameObject.scene.PhysicsSystem.RemoveCollider(shapeIndex);
        }

    }
}
