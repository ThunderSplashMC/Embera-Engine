using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;

namespace Elemental.Editor.EditorUtils
{
    class LineRenderer
    {

        VertexArray VAO;
        Shader DummyShader = new Shader("Editor/Assets/Shaders/dummy/dummy");

        public LineRenderer() 
        {

            Vector3 startPoint = new Vector3(-0.5f, 0f, 0f);
            Vector3 endPoint = new Vector3(0.5f, 0f, 0f);
            float thickness = 0.3f;

            Vector3 direction = endPoint - startPoint;
            direction.Normalize();

            // Calculate the perpendicular direction of the line
            Vector3 perpendicular = new Vector3(direction.Y, -direction.X, 0f);
            perpendicular.Normalize();

            // Calculate the vertices of the line segment
            Vector3 vertex1 = startPoint + perpendicular * thickness / 2f;
            Vector3 vertex2 = startPoint - perpendicular * thickness / 2f;
            Vector3 vertex3 = endPoint + perpendicular * thickness / 2f;
            Vector3 vertex4 = endPoint - perpendicular * thickness / 2f;

            // Create a list of vertices
            List<Vertex> vertices = new List<Vertex>();
            vertices.Add(new Vertex(vertex1, Vector3.One, Vector2.One));
            vertices.Add(new Vertex(vertex2, Vector3.One, Vector2.One));
            vertices.Add(new Vertex(vertex3, Vector3.One, Vector2.One));
            vertices.Add(new Vertex(vertex4, Vector3.One, Vector2.One));
            vertices.Add(new Vertex(vertex1, Vector3.One, Vector2.One));
            vertices.Add(new Vertex(vertex3, Vector3.One, Vector2.One));

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Count, false);

            vertexBuffer.SetData(vertices.ToArray(), vertices.Count);

            VAO = new VertexArray(vertexBuffer);
        }

        public void Render(Camera GuizmoView, Vector3 startPoint, Vector3 endPoint, float thickness = 2f)
        {

            Vector3 direction = endPoint - startPoint;
            direction.Normalize();



            // Calculate the center point of the line
            Vector3 centerPoint = (startPoint + endPoint) / 2f;
            //rot.Invert();

            // Calculate the perpendicular direction of the line
            Vector3 perpendicular = (Vector4.UnitY * (Matrix4.LookAt(direction, GuizmoView.position, Vector3.UnitY))).Xyz;

            RenderWithoutRecursive(GuizmoView, centerPoint, perpendicular * 10, 0.2f);

            perpendicular.Normalize();

            //Matrix4 rot = Matrix4.LookAt(centerPoint, GuizmoView.position, Vector3.UnitY);// Matrix4.CreateRotationZ((float)MathHelper.Acos(Vector3.Dot(Vector3.Cross(GuizmoView.position, direction), GuizmoView.position)/(Vector3.Cross(GuizmoView.position, direction).Length * GuizmoView.position.Length)));

            // Calculate the vertices of the line segment
            Vector3 vertex1 = startPoint + perpendicular * thickness / 2f;
            Vector3 vertex2 = startPoint - perpendicular * thickness / 2f;
            Vector3 vertex3 = endPoint + perpendicular * thickness / 2f;
            Vector3 vertex4 = endPoint - perpendicular * thickness / 2f;

            // Create a list of vertices
            Vertex[] vertices = new Vertex[]
            {
                new Vertex(vertex1, Vector3.One, Vector2.One),
                new Vertex(vertex2, Vector3.One, Vector2.One),
                new Vertex(vertex3, Vector3.One, Vector2.One),
                new Vertex(vertex4, Vector3.One, Vector2.One),
                new Vertex(vertex1, Vector3.One, Vector2.One),
                new Vertex(vertex3, Vector3.One, Vector2.One)
            };

            VAO.VertexBuffer.SetData(vertices, vertices.Length);

            DummyShader.Use();

            Matrix4 MODELMATRIX = Matrix4.Identity;

            DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            DummyShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            VAO.Render();
        }

        public void RenderWithoutRecursive(Camera GuizmoView, Vector3 startPoint, Vector3 endPoint, float thickness = 2f)
        {

            Vector3 direction = endPoint - startPoint;
            direction.Normalize();



            // Calculate the center point of the line
            Vector3 centerPoint = (startPoint + endPoint) / 2f;
            //rot.Invert();

            // Calculate the perpendicular direction of the line
            Vector3 perpendicular = new Vector3(direction.Y, -direction.X, direction.Z);

            perpendicular.Normalize();

            //Matrix4 rot = Matrix4.LookAt(centerPoint, GuizmoView.position, Vector3.UnitY);// Matrix4.CreateRotationZ((float)MathHelper.Acos(Vector3.Dot(Vector3.Cross(GuizmoView.position, direction), GuizmoView.position)/(Vector3.Cross(GuizmoView.position, direction).Length * GuizmoView.position.Length)));

            // Calculate the vertices of the line segment
            Vector3 vertex1 = startPoint + perpendicular * thickness / 2f;
            Vector3 vertex2 = startPoint - perpendicular * thickness / 2f;
            Vector3 vertex3 = endPoint + perpendicular * thickness / 2f;
            Vector3 vertex4 = endPoint - perpendicular * thickness / 2f;

            // Create a list of vertices
            Vertex[] vertices = new Vertex[]
            {
                new Vertex(vertex1, Vector3.One, Vector2.One),
                new Vertex(vertex2, Vector3.One, Vector2.One),
                new Vertex(vertex3, Vector3.One, Vector2.One),
                new Vertex(vertex4, Vector3.One, Vector2.One),
                new Vertex(vertex1, Vector3.One, Vector2.One),
                new Vertex(vertex3, Vector3.One, Vector2.One)
            };

            VAO.VertexBuffer.SetData(vertices, vertices.Length);

            DummyShader.Use();

            Matrix4 MODELMATRIX = Matrix4.Identity;// * rot;

            DummyShader.SetMatrix4("W_MODEL_MATRIX", MODELMATRIX);
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", GuizmoView.GetProjectionMatrix());
            DummyShader.SetMatrix4("W_VIEW_MATRIX", GuizmoView.GetViewMatrix());

            VAO.Render();
        }

        private Vector3 RotateVertexAroundPoint(Vector3 vertex, Vector3 centerPoint, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            float x = vertex.X - centerPoint.X;
            float y = vertex.Y - centerPoint.Y;
            return new Vector3(x * cos - y * sin + centerPoint.X, x * sin + y * cos + centerPoint.Y, vertex.Z);
        }


    }
}
