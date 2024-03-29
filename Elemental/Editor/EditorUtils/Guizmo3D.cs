﻿using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using ImGuizmoNET;

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
        static Shader GridShaderEx;
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
            GridShaderEx = new Shader("Editor/Assets/Shaders/Experimental/grid");
            DummyShader = new Shader("Editor/Assets/Shaders/dummy/dummy");

            Vertex[] sphereVertices = VERTEX_DEFAULTS.GetSphereVertices();
            VertexBuffer sphere = new VertexBuffer(Vertex.VertexInfo, sphereVertices.Length);
            sphere.SetData(sphereVertices, sphereVertices.Length);
            SphereGuizmo = new VertexArray(sphere);

            SphereMesh = ModelImporter.LoadModel("Engine/EngineContent/model/sphere-lower.fbx")[0];

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
            GL.DepthFunc(DepthFunction.Always);
        }

        public static void DrawGrid2()
        {
            GridShader.Use();
            GuizmoShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90)) * Matrix4.CreateScale(1000));
            GridShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            GridShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());
            GridShader.SetBool("depthBehind", true);

            GuizmoVAO.Render();
        }

        public static void DrawGrid()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GridShaderEx.Use();
            GridShaderEx.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            GridShaderEx.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());
            //GridShaderEx.SetBool("depthBehind", true);

            GuizmoVAO.Render();

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
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
            icon.BindUnit(0);

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

        public static void DrawCube(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            Matrix4 MODELMATRIX = Matrix4.CreateRotationX(rot.X) * Matrix4.CreateRotationY(rot.Y) * Matrix4.CreateRotationZ(rot.Z);
            MODELMATRIX *= Matrix4.CreateScale(scale);
            MODELMATRIX *= Matrix4.CreateTranslation(pos);

            DummyShader.Use();
            DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", RenderGraph.Camera.GetProjectionMatrix());
            DummyShader.SetMatrix4("W_VIEW_MATRIX", RenderGraph.Camera.GetViewMatrix());
            RendererUtils.CubeVAO.Render();
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

        public static void DrawManipulatePosition(ref Vector3 position, Vector4 viewportSize)
        {
            ImGuizmo.SetRect(viewportSize.X, viewportSize.Y, viewportSize.Z, viewportSize.W);

            Matrix4 view = RenderGraph.Camera.GetViewMatrix();
            view.Column0 *= -1;

            Matrix4 proj = RenderGraph.Camera.GetProjectionMatrix();

            Matrix4 model = Matrix4.CreateTranslation(position);

            //ImGuizmo.Manipulate(ref *View_Ptr, ref *Proj_Ptr, OPERATION.TRANSLATE_X | OPERATION.TRANSLATE_Y | OPERATION.TRANSLATE_Z, MODE.LOCAL, ref model.Row0.X);
            ImGuizmo.Manipulate(ref view.Row0.X, ref proj.Row0.X, OPERATION.TRANSLATE_X | OPERATION.TRANSLATE_Y | OPERATION.TRANSLATE_Z, MODE.LOCAL, ref model.Row0.X);

            //model.Transpose();
            position = model.ExtractTranslation();
        }

        public static void DrawManipulateRotation(Vector3 position, ref Vector3 rotation, Vector4 viewportSize)
        {
            ImGuizmo.SetRect(viewportSize.X, viewportSize.Y, viewportSize.Z, viewportSize.W);

            Matrix4 view = RenderGraph.Camera.GetViewMatrix();
            view.Column0 *= -1;

            Matrix4 proj = RenderGraph.Camera.GetProjectionMatrix();

            Matrix4 model = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(rotation.X)) * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(rotation.Y)) * Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(rotation.Z)) * Matrix4.CreateTranslation(position);
            

            //ImGuizmo.Manipulate(ref *View_Ptr, ref *Proj_Ptr, OPERATION.TRANSLATE_X | OPERATION.TRANSLATE_Y | OPERATION.TRANSLATE_Z, MODE.LOCAL, ref model.Row0.X);
            ImGuizmo.Manipulate(ref view.Row0.X, ref proj.Row0.X, OPERATION.ROTATE, MODE.LOCAL, ref model.Row0.X);

            //model.Transpose();
            Vector3 rot = model.ExtractRotation().ToEulerAngles();
            rotation = new Vector3((float)MathHelper.RadiansToDegrees(rot.X), (float)MathHelper.RadiansToDegrees(rot.Y), (float)MathHelper.RadiansToDegrees(rot.Z));
        }

        public static void DrawViewManipulate(Vector4 viewportSize)
        {
            Matrix4 view = RenderGraph.Camera.GetViewMatrix();
            view.Column0 *= -1;

            ImGuizmo.ViewManipulate(ref view.Row0.X, 2, new System.Numerics.Vector2(viewportSize.X + viewportSize.Z - 100, viewportSize.Y + 50), new System.Numerics.Vector2(64, 64), 0x10101010);
        }

        public static int End()
        {
            GuizmoBuffer.UnBind();
            return GuizmoBuffer.GetColorAttachment(0);
        }
    }
}
