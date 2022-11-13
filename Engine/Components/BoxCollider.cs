using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    class BoxCollider : Component
    {
        public BulletSharp.BoxShape Shape;

        public override void OnStart()
        {
            Shape = new BulletSharp.BoxShape(new BulletSharp.Math.Vector3(gameObject.transform.scale.X / 2, gameObject.transform.scale.Y / 2, gameObject.transform.scale.Z / 2));
            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {


            base.OnUpdate(deltaTime);
        }

    }
}
