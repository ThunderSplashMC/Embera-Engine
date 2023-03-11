﻿using System;
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
        static Shader GridShader;

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

            GuizmoShader = new Shader("Elemental/Assets/Shaders/guizmo");
            GridShader = new Shader("Elemental/Assets/Shaders/grid");
            
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

        public static void DrawGrid()
        {
            GridShader.Use();
            GuizmoShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90)) * Matrix4.CreateScale(1000));
            GridShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            GridShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            GuizmoVAO.Render();
        }

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

        public static int End()
        {
            GuizmoBuffer.UnBind();
            return GuizmoBuffer.GetColorAttachment(0);
        }
    }
}
