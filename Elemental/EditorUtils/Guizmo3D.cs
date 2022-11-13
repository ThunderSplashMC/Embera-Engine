using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Rendering;
using OpenTK.Graphics.OpenGL;


namespace DevoidEngine.Elemental.EditorUtils
{
    struct GuizmoIcon
    {
        public string name;
        public Texture icon;
    }

    static class Guizmo3D
    {
        static VertexArray GuizmoVAO;
        static Shader GuizmoShader;

        static FrameBuffer GuizmoBuffer;

        static Camera GuizmoView;

        static List<GuizmoIcon> GuizmoIcons = new List<GuizmoIcon>();

        static Matrix4 OProjection;

        public static void Init(int width, int height, FrameBuffer CompositeBuffer)
        {
            GuizmoBuffer = CompositeBuffer;

            Vertex[] vertices = VERTEX_DEFAULTS.GetFrameBufferVertices();

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            GuizmoVAO = new VertexArray(vertexBuffer);

            GuizmoShader = new Shader("Elemental/Assets/Shaders/guizmo");
            
        }

        public static void AddGuizmoIcon(string name, Texture texture)
        {
            GuizmoIcons.Add(new GuizmoIcon()
            {
                name = name,
                icon = texture
            });
        }

        public static void Begin(EditorCamera camera)
        {
            GuizmoView = camera.Camera;
            GuizmoBuffer.Bind();
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
        }

        public static void DrawGuizmo(Texture icon, Vector3 position)
        {

            Matrix4 MODELMATRIX = Matrix4.LookAt(position, GuizmoView.position, Vector3.UnitY);

            MODELMATRIX.Invert();

            GuizmoShader.Use();
            GuizmoShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            GuizmoShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            GuizmoShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            GuizmoShader.SetInt("u_Texture", 0);
            icon.SetActiveUnit(TextureActiveUnit.UNIT0);
            icon.BindTexture();

            GuizmoVAO.Render();
        }

        public static int End()
        {
            GuizmoBuffer.UnBind();
            return GuizmoBuffer.GetColorAttachment(0);
        }
    }
}
