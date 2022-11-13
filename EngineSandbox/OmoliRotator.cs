using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;


namespace DevoidEngine.EngineSandbox
{
    [RunInEditMode]
    class OmoliRotator : Component
    {
        public float Speed;

        public override void OnStart()
        {

            base.OnStart();
        }

        public override void OnUpdate(float deltaTime)
        {
            gameObject.transform.rotation.X += deltaTime * Speed;
            base.OnUpdate(deltaTime);
        }
    }
}
