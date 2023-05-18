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

            InitializeRenderFramebuffers(width, height);
        }

        static void InitializeRenderFramebuffers(int width, int height)
        {
            FrameBufferSpecification frameBufferSpecification = new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA16F, textureType = FrameBufferTextureType.Texture2D}
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            };

            RenderGraph._2DBuffer = new FrameBuffer(frameBufferSpecification);
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
            OrthoProjection = Matrix4.CreateOrthographicOffCenter(
                    0,
                    (float)width,
                    0,
                    (float)height,
                    0,
                    1
            );
        }


        public static void Render()
        {
            RenderGraph._2DBuffer.Bind();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            DrawItems(DrawList);
            DrawList.Clear();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            RenderGraph._2DBuffer.UnBind();

            RendererUtils.BlitFBToScreen(RenderGraph._2DBuffer, RenderGraph.CompositeBuffer, 0.5f, true);
        }

        static void DrawItems(List<DrawItem> DrawList)
        {
            Texture.UnbindTexture();
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
                    if (drawItem.Texture != null) GL.BindTextureUnit(0, drawItem.Texture.GetRendererID());
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

        public static void ResizeOrtho(int width, int height)
        {
            SetOrthographic(width, height);
        }

        public static void Resize(int width, int height)
        {
            
            RenderGraph._2DBuffer.Resize(width, height);
        }
    }
}
