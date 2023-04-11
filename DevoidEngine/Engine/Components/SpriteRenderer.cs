using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    public class SpriteRenderer : Component
    {
        public override string Type { get; } = nameof(SpriteRenderer);

        public Texture Texture;

        private Material nullMaterial = new Material(new Shader("Engine/EngineContent/Shaders/NullShader2d/null2d"));

        private UITransform UITransform;

        public override void OnStart()
        {
            if (gameObject.GetComponent<UITransform>() == null)
            {
                gameObject.AddComponent<UITransform>();
            }

            UITransform = gameObject.GetComponent<UITransform>();
        }

        float time = 0;

        public override void OnUpdate(float deltaTime)
        {
            time += deltaTime;
            if (Texture != null)
            {
                Renderer2D.Submit(UITransform.Position, gameObject.transform.rotation.Xy, gameObject.transform.scale.Xy, Renderer2D.Quad, Texture);
            } else
            {
                Renderer2D.Submit(UITransform.Position, gameObject.transform.rotation.Xy, gameObject.transform.scale.Xy, Renderer2D.Quad, null, nullMaterial);
            }
        }
    }
}
