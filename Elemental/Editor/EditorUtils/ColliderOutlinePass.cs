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
        DynamicCollider[] dynamicColliders;
        StaticCollider[]  staticColliders;

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
            //if (Editor.EditorScene.GetSceneState() == Scene.SceneState.Play) return;

            dynamicColliders = Editor.EditorScene.GetSceneRegistry().GetComponentsOfType<DynamicCollider>();
            staticColliders = Editor.EditorScene.GetSceneRegistry().GetComponentsOfType<StaticCollider>();

            RenderGraph.CompositeBuffer.Bind();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            DummyShader.Use();

            for (int i = 0; i < dynamicColliders.Length; i++)
            {
                Matrix4 MODELMATRIX = Matrix4.CreateScale(dynamicColliders[i].gameObject.transform.scale);

                MODELMATRIX *= Matrix4.CreateRotationX(dynamicColliders[i].gameObject.transform.rotation.X) * Matrix4.CreateRotationY(dynamicColliders[i].gameObject.transform.rotation.Y) * Matrix4.CreateRotationZ(dynamicColliders[i].gameObject.transform.rotation.Z);
                MODELMATRIX *= Matrix4.CreateTranslation(dynamicColliders[i].gameObject.transform.position);

                if (dynamicColliders[i].ColliderType == ColliderType.Box)
                {
                    DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                    DummyShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                    DummyShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                    RendererUtils.CubeVAO.Render();
                }
                else
                {
                    Guizmo3D.DrawSphere(dynamicColliders[i].gameObject.transform.position, 1);
                }
            }

            for (int i = 0; i < staticColliders.Length; i++)
            {
                Matrix4 MODELMATRIX = Matrix4.CreateScale(staticColliders[i].BoxSize);

                MODELMATRIX *= Matrix4.CreateRotationX(staticColliders[i].gameObject.transform.rotation.X) * Matrix4.CreateRotationY(staticColliders[i].gameObject.transform.rotation.Y) * Matrix4.CreateRotationZ(staticColliders[i].gameObject.transform.rotation.Z);
                MODELMATRIX *= Matrix4.CreateTranslation(staticColliders[i].gameObject.transform.position);

                if (staticColliders[i].ColliderType == ColliderType.Box)
                {
                    DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                    DummyShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
                    DummyShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
                    RendererUtils.CubeVAO.Render();
                }
                else
                {
                    Guizmo3D.DrawWireSphere(staticColliders[i].gameObject.transform.position, staticColliders[i].SphereSize);
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            RenderGraph.CompositeBuffer.UnBind();

        }

    }
}
