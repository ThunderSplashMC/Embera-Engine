using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    struct Renderer2DData
    {
        public int ViewportHeight, ViewportWidth;
    }


    class Renderer2D
    {
        public struct DrawItem
        {
            public Texture Texture;
            public Vector3 position;
            public Vector2 rotation;
            public Mesh mesh;
            public Vector3 scale;
            public float distanceFromCamera;
        }

        static Renderer2DData RendererData = new Renderer2DData();
        static List<DrawItem> DrawList = new List<DrawItem>();
        static Shader QuadShader;
        public static Matrix4 OrthoProjection;

        public static void Init(int width, int height)
        {
            RendererData.ViewportHeight = height;
            RendererData.ViewportWidth = width;

            SetOrthographic();

            QuadShader = new Shader("Engine/EngineContent/shaders/2d");
        }

        static void SetOrthographic()
        {
            float aspectRatio = (float)RenderGraph.ViewportWidth / RenderGraph.ViewportHeight;

            //-5.0f * aspectRatio * 0.5f,
            //        5.0f * aspectRatio * 0.5f,
            //        -5.0f * 0.5f,
            //        5.0f * 0.5f,

            OrthoProjection = Matrix4.CreateOrthographicOffCenter(
                    0,
                    RenderGraph.ViewportWidth,
                    0,
                    RenderGraph.ViewportHeight,
                    0.1f,
                    1000f
            );
        }

        static void SetOrthographic(int width, int height)
        {
            float aspectRatio = (float)width / height;

            OrthoProjection = Matrix4.CreateOrthographicOffCenter(
                    0,
                    width,
                    0,
                    height,
                    0.001f,
                    1000f
            );
        }


        public static void Render()
        {
            RenderGraph.CompositeBuffer.Bind();
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            DrawItems(DrawList);
            DrawList.Clear();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderGraph.CompositeBuffer.UnBind();
        }

        static void DrawItems(List<DrawItem> DrawList)
        {
            //QuadShader.Use();
            //QuadShader.SetIntArray("u_Textures", new int[DrawList.Count]);
            for (int i = 0; i < DrawList.Count; i++)
            {
                DrawItem drawItem = DrawList[i];

                

                Matrix4 MODELMATRIX = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(drawItem.rotation.X));

                MODELMATRIX *= Matrix4.CreateScale(drawItem.scale);
                MODELMATRIX *= Matrix4.CreateTranslation(drawItem.position);

                QuadShader.Use();
                QuadShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                QuadShader.SetMatrix4("W_PROJECTION_MATRIX", OrthoProjection);

                if (drawItem.Texture != null)
                {
                    QuadShader.SetInt("u_Texture", 0);
                    QuadShader.SetInt("USE_TEX_0", 1);
                    drawItem.Texture.BindTexture();
                    drawItem.Texture.SetActiveUnit(TextureActiveUnit.UNIT0 + i);
                }
                else if (drawItem.mesh != null)
                {
                    Material drawMat = drawItem.mesh.Material;
                    //drawMat.Set("u_Texture", 0);
                    //drawMat.Set("USE_TEX_0", 1);
                    drawMat.Set("W_MODEL_MATRIX", MODELMATRIX);
                    drawMat.Set("W_PROJECTION_MATRIX", OrthoProjection);

                    drawMat.Apply();

                    drawItem.mesh.Draw();
                    continue;
                }

                RendererUtils.QuadVAO.Render();
            }
        }

        public static void Submit(Texture texture, Vector2 pos, Vector2 rot, Vector2 scale)
        {
            DrawList.Add(new DrawItem()
            {
                Texture = texture,
                position = new Vector3(pos.X, pos.Y, 0),
                rotation = rot,
                scale = new Vector3(scale.X, scale.Y, 1),
            });
        }

        public static void Submit(Vector2 pos, Vector2 rot, Vector2 scale)
        {
            DrawList.Add(new DrawItem()
            {
                Texture = null,
                position = new Vector3(pos.X, pos.Y, 0),
                rotation = rot,
                scale = new Vector3(scale.X, scale.Y, 1)
            });
        }

        public static void Submit(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            DrawList.Add(new DrawItem()
            {
                Texture = null,
                position = pos,
                rotation = rot.Xy,
                scale = scale
            });
        }

        public static void Submit(Texture texture, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            DrawList.Add(new DrawItem()
            {
                Texture = texture,
                position = pos,
                rotation = rot.Xy,
                scale = scale
            });
        }

        public static void Submit(Mesh mesh, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            DrawList.Add(new DrawItem()
            {
                mesh = mesh,
                position = pos,
                rotation = rot.Xy,
                scale = scale
            });
        }

        public static void Resize(int width, int height)
        {
            SetOrthographic(width, height);
        }
    }
}
