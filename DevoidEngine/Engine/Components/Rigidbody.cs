using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

namespace DevoidEngine.Engine.Components
{
    public enum BodyType
    {
        Kinematic,
        Dynamic
    }

    [RunInEditMode]
    public class Rigidbody : Component
    {
        public override string Type { get; } = nameof(Rigidbody);

        public BodyType RigidbodyType = BodyType.Dynamic;
        private BodyType prevType = BodyType.Dynamic;
        public bool FreeMove = false;


        bool isInitialized = false;


        BodyReference bodyReference;
        Static staticReference;
        Collider3D collider;


        public override void OnStart()
        {

            if (isInitialized)
            {
                gameObject.scene.PhysicsSystem.RemoveBody(bodyReference);
            }

            collider = gameObject.GetComponent<Collider3D>();


            SetupPhysics();
            isInitialized = true;
        }

        void SetupPhysics()
        {
            ColliderShapes shape = collider.GetCollider();
            if (RigidbodyType == BodyType.Dynamic)
            {
                if (shape == ColliderShapes.Box)
                {
                    bodyReference = gameObject.scene.PhysicsSystem.AddDynamicBody(gameObject.transform.position, gameObject.transform.rotation, ((Box)collider.GetColliderShape()).ComputeInertia(1), collider.GetColliderID());
                }
                if (shape == ColliderShapes.Sphere)
                {

                    bodyReference = gameObject.scene.PhysicsSystem.AddDynamicBody(gameObject.transform.position, gameObject.transform.rotation, ((Sphere)collider.GetColliderShape()).ComputeInertia(1), collider.GetColliderID());
                }
            }
            else
            {
                if (shape == ColliderShapes.Box)
                {
                    staticReference = gameObject.scene.PhysicsSystem.AddStaticBody(gameObject.transform.position, gameObject.transform.rotation, collider.GetColliderID());
                }
                if (shape == ColliderShapes.Sphere)
                {

                    staticReference = gameObject.scene.PhysicsSystem.AddStaticBody(gameObject.transform.position, gameObject.transform.rotation, collider.GetColliderID());
                }
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (prevType != RigidbodyType)
            {
                SetupPhysics();
                prevType = RigidbodyType;
            }
            if (collider.HasShapeChanged())
            {
                SetupPhysics();
            }

            if (FreeMove)
            {
                if (RigidbodyType == BodyType.Kinematic)
                {
                    staticReference.Pose.Position = new System.Numerics.Vector3(gameObject.transform.position.X, gameObject.transform.position.Y, gameObject.transform.position.Z);
                    staticReference.Pose.Orientation = PhysicsHelper.ToQuaternion(new System.Numerics.Vector3(gameObject.transform.rotation.X, gameObject.transform.rotation.Y, gameObject.transform.rotation.Z));
                } else
                {
                    bodyReference.Pose.Position = new System.Numerics.Vector3(gameObject.transform.position.X, gameObject.transform.position.Y, gameObject.transform.position.Z);
                    bodyReference.Pose.Orientation = PhysicsHelper.ToQuaternion(new System.Numerics.Vector3(gameObject.transform.rotation.X, gameObject.transform.rotation.Y, gameObject.transform.rotation.Z));
                }
            } else
            {
                if (RigidbodyType == BodyType.Dynamic)
                {
                    gameObject.transform.position = new Vector3(bodyReference.Pose.Position.X, bodyReference.Pose.Position.Y, bodyReference.Pose.Position.Z);

                    System.Numerics.Vector3 rot = PhysicsHelper.ToEulerAngles(bodyReference.Pose.Orientation);

                    gameObject.transform.rotation = new Vector3(rot.X, rot.Y, rot.Z);
                }
            }
        }
    }
}
