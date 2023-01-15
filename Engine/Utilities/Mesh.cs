using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Core;


namespace DevoidEngine.Engine.Utilities
{
    class Mesh
    {

        public static int TotalMeshCount = 0;

        public VertexBuffer VBO;
        public IndexBuffer IBO;
        public VertexArray VAO;

        public Material Material;
        public int MeshID;
        public int VertexCount;
        public string path;
        public string name;
        public bool Renderable = true;
        

        bool IsStatic = true;

        public Mesh()
        {
            TotalMeshCount += 1;
            MeshID = TotalMeshCount;
        }

        ~Mesh()
        {
            TotalMeshCount -= 1;
            VBO.Dispose();
            IBO.Dispose();
            VAO.Dispose();
        }

        public void Draw()
        {
            if (!Renderable) { return; }
            if (IBO != null)
            {
                VAO.RenderWithIndices(IBO);
            } else
            {
                VAO.Render();
            }
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public string GetPath()
        {
            return path;
        }

        public void SetMaterial(Material material)
        {
            Material = material;
        }

        public void SetStatic(bool value)
        {
            IsStatic = value;
        }

        public void SetVertices(Vertex[] vertices)
        {
            VBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, IsStatic);
            VBO.SetData(vertices, vertices.Length);
            VAO = new VertexArray(VBO);
            this.VertexCount = vertices.Length;
        }

        public void SetIndices(int[] indices)
        {
            IBO = new IndexBuffer(indices.Length, IsStatic);
            IBO.SetData(indices, indices.Length);
        }
    }
}
