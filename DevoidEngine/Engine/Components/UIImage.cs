using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    public class UIImage : Component
    {
        public override string Type { get; } = nameof(UIImage);

        public Texture texture;

        Canvas canvas;
        UITransform UITransform;

        public override void OnStart()
        {
            Canvas[] canvases = gameObject.scene.GetSceneRegistry().GetComponentsOfType<Canvas>();
            canvas = canvases.Length == 0 ? null : canvases[0];
            UITransform = gameObject.GetComponent<UITransform>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (UITransform == null || canvas == null)
            {
                return;
            }

            canvas.AddToQueue(texture, UITransform.Position, UITransform.Rotation, UITransform.Size);
        }
    }
}
