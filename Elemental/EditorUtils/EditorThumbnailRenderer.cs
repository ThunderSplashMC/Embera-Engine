using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Components;

namespace DevoidEngine.Elemental.EditorUtils
{
    class EditorThumbnailRenderer : RenderPass
    {
        public struct MaterialViewData
        {
            public Material material;
            public FrameBuffer fb;
        }

        public List<MaterialViewData> materials = new List<MaterialViewData>();

        Camera Camera;

        FrameBuffer fb;

        Mesh mesh = ModelImporter.LoadModel("Engine/EngineContent/model/sphere.fbx")[0].mesh;

        Bloom bloomRenderer = new Bloom();

        bool reRender = true;

        public int AddMaterialToQueue(Material material)
        {
            FrameBuffer fb = new FrameBuffer(new FrameBufferSpecification()
            {

                width = 128,
                height = 128,
                ColorAttachments = new ColorAttachment[]
    {
                    new ColorAttachment() { textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D }
    }
            });

            materials.Add(new MaterialViewData()
            {
                material = material,
                fb = fb
            });

            return materials.Count - 1;
        }

        public void ReRender()
        {
            reRender = true;
        }

        public int GetThumbnail(int index)
        {
            return materials[index].fb.GetColorAttachment(0);
        }

        public void RemoveMaterialFromQueue(Material material)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].material == material)
                {
                    materials.Remove(materials[i]);
                }
            }
        }

        public void RemoveAllMaterialsFromQueue()
        {
            materials.Clear();
        }

        public override void Initialize(int width, int height)
        {
            fb = new FrameBuffer(new FrameBufferSpecification()
            {
                width = 128,
                height = 128,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment()
                    {
                        textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D
                    }
                }
            });

            Camera = new Camera();

            Camera.position = new Vector3(0, 0, -100);

            Camera.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1f, 0.001f, 1000f));

            Camera.SetViewMatrix(Matrix4.LookAt(Camera.position, Camera.position + new Vector3(0, 0, 1), new Vector3(0, 1, 0))); ;

            bloomRenderer.Init(128, 128);

            ReRender();
        }

        public Vector3 pos = new Vector3(0, 0, -60.8f);
        public int intensity = 10000;

        public override void DoRenderPass()
        {
            if (!reRender) return;
            for (int i = 0; i < materials.Count; i++)
            {

                FrameBuffer materialFB = materials[i].fb;
                Material material = materials[i].material;

                fb.Bind();

                GL.Viewport(0, 0, 128, 128);
                GL.ClearColor(0,0,0,1);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);

                RendererUtils.Clear();

                //dummy.Use();


                material.GetShader().Use();

                material.GetShader().SetMatrix4("W_PROJECTION_MATRIX", Camera.GetProjectionMatrix());
                material.GetShader().SetMatrix4("W_VIEW_MATRIX", Camera.GetViewMatrix());
                material.GetShader().SetVector3("C_VIEWPOS", Camera.position);

                Matrix4 MODELMATRIX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0f));
                MODELMATRIX *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(0f));
                MODELMATRIX *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(0f));
                MODELMATRIX *= Matrix4.CreateScale(new Vector3(15));
                MODELMATRIX *= Matrix4.CreateTranslation(new Vector3(1));

                material.GetShader().SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);

                material.GetShader().SetFloat("material.ao", 2f);

                Renderer3D.ResetLighting(material.GetShader());

                material.GetShader().SetFloat("L_POINTLIGHTS[0].intensity", intensity);
                material.GetShader().SetFloat("L_POINTLIGHTS[0].constant", 1f);
                material.GetShader().SetVector3("L_POINTLIGHTS[0].diffuse", new Vector3(1));
                material.GetShader().SetVector3("L_POINTLIGHTS[0].position", pos);
                material.GetShader().SetBool("L_POINTLIGHTS[0].shadows", false);

                material.Apply();

                //RendererUtils.QuadVAO.Render();

                //dummy.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                //dummy.SetMatrix4("W_VIEW_MATRIX", Camera.GetViewMatrix());
                //dummy.SetMatrix4("W_PROJECTION_MATRIX", Camera.GetProjectionMatrix());

                mesh.Draw();

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);


                fb.UnBind();

                bloomRenderer.RenderBloomTexture(fb.GetColorAttachment(0));


                materials[i].fb.Bind();

                RendererUtils.Clear();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                RendererUtils.HDRShader.Use();
                RendererUtils.HDRShader.SetInt("S_RENDERED_TEXTURE", 0);
                RendererUtils.HDRShader.SetInt("S_BLOOM_TEXTURE", 1);
                RendererUtils.HDRShader.SetInt("S_VIGNETTE_TEXTURE", 2);
                RendererUtils.HDRShader.SetFloat("U_BLOOM_STRENGTH", RenderGraph.BLOOM ? RenderGraph.BloomRenderer.bloomStr : 0);
                RendererUtils.HDRShader.SetFloat("U_VIGNETTE_STRENGTH", 0);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, fb.GetColorAttachment(0));

                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, bloomRenderer.GetBloomTexture());

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                RendererUtils.QuadVAO.Render();
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);



                materials[i].fb.UnBind();

            }

            reRender = false;
            
        }

        //public override void Resize(int width, int height)
        //{
        //    SetViewportSize(width, height);
        //}

        //public void SetViewportSize(int width, int height)
        //{
        //    SetPerspective(width, height);
        //}

        //private void SetPerspective(int width, int height)
        //{
        //    Camera.SetProjectionMatrix(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)128/128, 0.01f, 1000f));
        //}

    }
}
