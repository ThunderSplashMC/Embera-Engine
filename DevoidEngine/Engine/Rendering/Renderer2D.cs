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


    public class Renderer2D
    {
        public struct DrawItem
        {
            public Texture Texture;
            public Vector2 position;
            public Vector2 rotation;
            public Vector2 scale;
            public Mesh mesh;
            public Material material;
            public float distanceFromCamera;
        }

        static Renderer2DData RendererData = new Renderer2DData();
        static List<DrawItem> DrawList = new List<DrawItem>();
        static Shader QuadShader;
        public static Matrix4 OrthoProjection;
        public static Mesh Quad;

        public static void Init(int width, int height)
        {
            RendererData.ViewportHeight = height;
            RendererData.ViewportWidth = width;

            SetOrthographic();

            Quad = new Mesh();
            Quad.SetVertexArrayObject(RendererUtils.QuadVAO);
            QuadShader = new Shader("Engine/EngineContent/shaders/2D/2d");
        }

        static void SetOrthographic()
        {
            float aspectRatio = (float)RenderGraph.ViewportWidth / RenderGraph.ViewportHeight;

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
            for (int i = 0; i < DrawList.Count; i++)
            {
                DrawItem drawItem = DrawList[i];

                Matrix4 MODELMATRIX = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(drawItem.rotation.X));

                MODELMATRIX *= Matrix4.CreateScale(drawItem.scale.X, drawItem.scale.Y, 1);
                MODELMATRIX *= Matrix4.CreateTranslation(drawItem.position.X, drawItem.position.Y, 0);

                if (drawItem.material == null)
                {
                    QuadShader.Use();
                    QuadShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
                    QuadShader.SetMatrix4("W_PROJECTION_MATRIX", OrthoProjection);

                    QuadShader.SetInt("u_Texture", 0);
                    QuadShader.SetInt("USE_TEX_0", 1);
                    drawItem.Texture.BindTexture();
                    drawItem.Texture.SetActiveUnit(TextureActiveUnit.UNIT0 + i);
                }
                else
                {
                    Material drawMat = drawItem.material;
                    drawMat.Set("W_MODEL_MATRIX", MODELMATRIX);
                    drawMat.Set("W_PROJECTION_MATRIX", OrthoProjection);

                    drawMat.GetShader().Use();

                    drawMat.Apply();

                    drawItem.mesh.Draw();
                }

                drawItem.mesh.Draw();
            }
        }

        public static void Submit(Vector2 pos, Vector2 rot, Vector2 scale, Mesh mesh, Texture texture, Material material = null)
        {
            DrawList.Add(new DrawItem()
            {
                mesh = mesh,
                Texture = texture,
                material = material,
                position = pos,
                rotation = rot,
                scale = scale
            });
        }

        public static void Resize(int width, int height)
        {
            SetOrthographic(width, height);
        }
    }
}
