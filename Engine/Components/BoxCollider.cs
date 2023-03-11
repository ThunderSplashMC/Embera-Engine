using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    class BoxCollider : Component
    {
        public override string Type { get; } = nameof(BoxCollider);

        public BulletSharp.BoxShape Shape;
        public Vector3 Size;

        public override void OnStart()
        {
            Console.WriteLine("Shape set");
            Shape = new BulletSharp.BoxShape(new BulletSharp.Math.Vector3(gameObject.transform.scale.X, gameObject.transform.scale.Y, gameObject.transform.scale.Z));
            base.OnStart();
        }

        private Vector3 prevSize;

        public override void OnUpdate(float deltaTime)
        {

        }
    }
}
