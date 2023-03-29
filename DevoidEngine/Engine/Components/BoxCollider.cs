using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    public class ColliderShape3D : Component
    {
        public override string Type { get; } = nameof(ColliderShape3D);

        public enum Shapes
        {
            Box,
            Sphere,
            Cone,
        }

        public Shapes ShapeType = Shapes.Box;
        public Vector3 Size;
        public float SphereRadius = 1f;

        private BulletSharp.CollisionShape Shape;

        public override void OnStart()
        {
            if (ShapeType == Shapes.Box)
            {
                Shape = new BulletSharp.BoxShape(new BulletSharp.Math.Vector3(gameObject.transform.scale.X, gameObject.transform.scale.Y, gameObject.transform.scale.Z));
            } else if (ShapeType == Shapes.Sphere)
            {
                Shape = new BulletSharp.SphereShape(SphereRadius);
            }
            base.OnStart();
        }

        public BulletSharp.CollisionShape GetCollisionShape()
        {
            return Shape;
        }

        private Vector3 prevSize;

        public override void OnUpdate(float deltaTime)
        {

        }
    }
}
