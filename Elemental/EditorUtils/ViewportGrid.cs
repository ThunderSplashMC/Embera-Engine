using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Elemental.EditorUtils
{
    class ViewportGrid
    {

        VertexBuffer GRID_VB;
        VertexArray GRID_VA;

        Shader GRID_SHADER;

        public ViewportGrid()
        {
            GRID_SHADER = new Shader("Engine/EngineContent/shaders/LineShader");
        }

        public void Init(int size = 100)
        {

            Vertex[] vertices = VERTEX_DEFAULTS.GetPlaneVertices();
            GRID_VB = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            GRID_VB.SetData(vertices, vertices.Length);

            GRID_VA = new VertexArray(GRID_VB);
        }

        public void Render(Camera camera)
        {
            GRID_SHADER.Use();
            GRID_SHADER.SetMatrix4("W_PROJECTION_MATRIX",camera.GetProjectionMatrix());
            GRID_SHADER.SetMatrix4("W_VIEW_MATRIX", camera.GetViewMatrix());
            GRID_SHADER.SetVector3("pos", new Vector3(0,0,0));
            GRID_VA.Render();
            
        }
    }
}
