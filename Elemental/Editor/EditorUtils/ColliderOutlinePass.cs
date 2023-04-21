using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Elemental.Editor.EditorUtils
{
    class ColliderOutlinePass : RenderPass
    {
        public EditorLayer Editor;
        Collider3D[] collider3D;

        public Shader DummyShader;

        public override void Initialize(int width, int height)
        {
            DummyShader = new Shader("Editor/Assets/Shaders/dummy/dummy");
        }

        public override void Resize(int width, int height)
        {
        
        }

        public override void DoRenderPass()
        {
            collider3D = Editor.EditorScene.GetSceneRegistry().GetComponentsOfType<Collider3D>();

            RenderGraph.CompositeBuffer.Bind();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            DummyShader.Use();

            for (int i = 0; i < collider3D.Length; i++)
            {
                Matrix4 MODELMATRIX = Matrix4.CreateScale(collider3D[i].gameObject.transform.scale);

                MODELMATRIX *= Matrix4.CreateRotationX(collider3D[i].gameObject.transform.rotation.X) * Matrix4.CreateRotationY(collider3D[i].gameObject.transform.rotation.Y) * Matrix4.CreateRotationZ(collider3D[i].gameObject.transform.rotation.Z);
                MODELMATRIX *= Matrix4.CreateTranslation(collider3D[i].gameObject.transform.position);

                if (collider3D[i].Shape == ColliderShapes.Box)
                {
                    DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                    DummyShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                    DummyShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                    RendererUtils.CubeVAO.Render();
                } else
                {
                    Guizmo3D.DrawSphere(collider3D[i].gameObject.transform.position, 1);
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            RenderGraph.CompositeBuffer.UnBind();

        }

    }
}
