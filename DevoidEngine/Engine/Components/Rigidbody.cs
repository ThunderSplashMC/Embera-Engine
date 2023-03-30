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

        public bool IsKinematic = false;
        public bool FreeMove = false;


        bool isInitialized = false;
        bool isKinematic = false;


        BodyReference bodyReference;
        Collider3D collider;


        public override void OnStart()
        {

            if (isInitialized)
            {
                gameObject.scene.PhysicsSystem.RemoveBody(bodyReference);
            }

            collider = gameObject.GetComponent<Collider3D>();

            bodyReference = gameObject.scene.PhysicsSystem.AddBody(gameObject.transform.position, gameObject.transform.rotation);
            if (collider != null)
            {
                bodyReference.SetShape(collider.GetColliderID());
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (collider == null) return;

            if (collider.HasShapeChanged())
            {
                bodyReference.SetShape(collider.GetColliderID());
            }
            if (isKinematic != IsKinematic)
            {
                isKinematic = IsKinematic;
                bodyReference.BecomeKinematic();
            }

            if (isKinematic)
            {
                bodyReference.Velocity.Linear = new System.Numerics.Vector3(0,0,0);
            }

            if (FreeMove)
            {
                bodyReference.Pose.Position = new System.Numerics.Vector3(gameObject.transform.position.X, gameObject.transform.position.Y, gameObject.transform.position.Z);
            } else
            {
                gameObject.transform.position = new Vector3(bodyReference.Pose.Position.X, bodyReference.Pose.Position.Y, bodyReference.Pose.Position.Z);
            }

            bodyReference.GetDescription(out BodyDescription bodyDescription);
            bodyDescription.Collidable.Shape = collider.GetColliderID();
        }
    }
}
