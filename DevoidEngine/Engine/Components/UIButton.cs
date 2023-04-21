using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class UIButton : Component
    {
        public override string Type { get; } = nameof(UIButton);

        UITransform transform;

        public override void OnStart()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
            if (transform == null) transform = gameObject.GetComponent<UITransform>();
            if (transform == null) return;

            Vector2 mousePos = InputSystem.GetMousePos();

            if ((mousePos.X < transform.Position.X + transform.Size.X && mousePos.Y < transform.Position.Y + transform.Size.Y) && (mousePos.X > transform.Position.X - transform.Size.X && mousePos.Y > transform.Position.Y - transform.Size.Y))
            {
                if (InputSystem.GetMouseDown(InputSystem.MouseButton.Left))
                {
                    Console.WriteLine("Button pressed");
                }
            }
        }

    }
}
