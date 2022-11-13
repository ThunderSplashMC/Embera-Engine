using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class SpriteRenderer : Component
    {
        public Texture Texture;

        public override void OnStart()
        {
            Console.WriteLine("I am here");
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Texture == null)
            {
                Renderer2D.Submit(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale);
            } else
            {
                Renderer2D.Submit(Texture, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale);
            }
        }
    }
}
