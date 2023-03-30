using BepuPhysics.Collidables;
using BepuPhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public enum ColliderShapes
    {
        Box,
        Sphere,
        Cone
    }

    [RunInEditMode]
    class Collider3D : Component
    {
        public override string Type { get; } = nameof(Transform);

        public ColliderShapes Shape = ColliderShapes.Box;

        ColliderShapes prevShape = ColliderShapes.Box;
        TypedIndex ColliderID;

        public override void OnStart()
        {

        }

        public override void OnUpdate(float deltaTime)
        {

            if (HasShapeChanged())
            {
                if (ColliderID != null) { gameObject.scene.PhysicsSystem.RemoveCollider(ColliderID); }
                if (Shape == ColliderShapes.Box)
                {
                    SetupBoxCollider();
                }
                if (Shape == ColliderShapes.Sphere)
                {
                    SetupSphereCollider();
                }
            }

        }

        public bool HasShapeChanged()
        {
            if (prevShape != Shape)
            {
                prevShape = Shape;
                return true;
            }
            return false;
        }

        public ColliderShapes GetCollider()
        {
            return Shape;
        }

        public TypedIndex GetColliderID()
        {
            return ColliderID;
        }

        public void SetupBoxCollider()
        {
            Box shape = new Box(gameObject.transform.scale.X, gameObject.transform.scale.Y, gameObject.transform.scale.Z);

            Console.WriteLine("Box Collider Set");

            ColliderID = gameObject.scene.PhysicsSystem.AddCollider(shape);
        }

        public void SetupSphereCollider()
        {
            Sphere shape = new Sphere(1f);

            ColliderID = gameObject.scene.PhysicsSystem.AddCollider(shape);
        }

    }
}
