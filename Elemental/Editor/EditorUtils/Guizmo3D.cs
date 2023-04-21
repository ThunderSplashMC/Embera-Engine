using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Rendering;
using OpenTK.Graphics.OpenGL;


namespace Elemental.Editor.EditorUtils
{
    struct GuizmoIcon
    {
        public string name;
        public Texture icon;
    }

    static class Guizmo3D
    {
        static VertexArray GuizmoVAO;
        static VertexArray SphereGuizmo;
        static Mesh SphereMesh;

        static Shader GuizmoShader;
        static Shader GridShader;
        static Shader DummyShader;

        static LineRenderer LineRenderer;

        static FrameBuffer GuizmoBuffer;
        
        static Camera GuizmoView;
        
        static List<GuizmoIcon> GuizmoIcons = new List<GuizmoIcon>();

        public static void Init(int width, int height, FrameBuffer CompositeBuffer)
        {
            GuizmoBuffer = CompositeBuffer;

            Vertex[] vertices = VERTEX_DEFAULTS.GetFrameBufferVertices();
            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            GuizmoVAO = new VertexArray(vertexBuffer);

            GuizmoShader = new Shader("Editor/Assets/Shaders/guizmo");
            GridShader = new Shader("Editor/Assets/Shaders/grid");
            DummyShader = new Shader("Editor/Assets/Shaders/dummy/dummy");

            Vertex[] sphereVertices = VERTEX_DEFAULTS.GetSphereVertices();
            VertexBuffer sphere = new VertexBuffer(Vertex.VertexInfo, sphereVertices.Length);
            sphere.SetData(sphereVertices, sphereVertices.Length);
            SphereGuizmo = new VertexArray(sphere);

            SphereMesh = ModelImporter.LoadModel("Engine/EngineContent/model/sphere-lower.fbx")[0].mesh;

            LineRenderer = new LineRenderer();

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
            GL.Enable(EnableCap.DepthTest);
        }

        public static void DrawGrid()
        {
            GridShader.Use();
            GuizmoShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90)) * Matrix4.CreateScale(1000));
            GridShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            GridShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());
            GridShader.SetBool("depthBehind", true);

            GuizmoVAO.Render();
        }

        // Draws a texture provided as a gizmo icon in the world with the specified position.
        // it always faces the camera.
        public static void DrawGuizmo(Texture icon, Vector3 position, float scale = 1)
        {

            Matrix4 MODELMATRIX = Matrix4.LookAt(position, GuizmoView.position, -Vector3.UnitY);
            MODELMATRIX *= Matrix4.CreateScale(scale);

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

        public static void DrawSphere(Vector3 position, float radius)
        {
            DummyShader.Use();

            Matrix4 MODELMATRIX = Matrix4.CreateScale(radius);
            MODELMATRIX *= Matrix4.CreateTranslation(position);

            DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            DummyShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            SphereMesh.Draw();

        }

        public static void DrawWireSphere(Vector3 position, float radius)
        {
            DummyShader.Use();

            Matrix4 MODELMATRIX = Matrix4.CreateScale(radius);
            MODELMATRIX *= Matrix4.CreateTranslation(position);

            DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            DummyShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            //GL.BindVertexArray(SphereMesh.VAO.VertexArrayObject);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, SphereMesh.VBO.VertexCount);
            SphereMesh.Draw();
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            
        }

        public static void DrawLine(Vector3 start, Vector3 end, float thickness = 2f)
        {
            LineRenderer.Render(GuizmoView, start, end, thickness);  
        }

        public static void DrawManipulatePosition(ref Vector3 position)
        {

        }

        public static int End()
        {
            GuizmoBuffer.UnBind();
            return GuizmoBuffer.GetColorAttachment(0);
        }
    }
}
