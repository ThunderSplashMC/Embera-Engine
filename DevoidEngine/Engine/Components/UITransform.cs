using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    class UITransform : Component
    {
        public override string Type { get; } = nameof(UITransform);

        public Vector2 Position;
        public Vector2 Rotation;

        public float Top;
        public float Bottom;
        public float Left;
        public float Right;

        public override void OnStart()
        {

        }

        public override void OnUpdate(float deltaTime)
        {

        }

    }
}
