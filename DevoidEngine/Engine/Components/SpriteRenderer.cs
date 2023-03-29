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

        public Shader nulLShader = new Shader("Engine/EngineContent/Shaders/NullShader2d/null2d");

        public Mesh mesh;

        public override void OnStart()
        {
            mesh = new Mesh();
            mesh.SetVertexArrayObject(RendererUtils.QuadVAO);

            mesh.Material = new Material(nulLShader);
        }

        float time = 0;

        public override void OnUpdate(float deltaTime)
        {
            time += deltaTime;
            if (Texture != null)
            {
                Renderer2D.Submit(Texture, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale);
            }
        }
    }
}
