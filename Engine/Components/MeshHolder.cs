using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Components
{
    class MeshHolder : Component
    {
        internal List<Mesh> Meshes { get; set; } = new List<Mesh>();

        public Type HelloWorld;

        public void AddMesh(Mesh mesh)
        {
            Meshes.Add(mesh);
        }

        public void AddMeshes(Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].Material.materialIndex = i;
                Meshes.Add(meshes[i]);
            }
        }
    }
}
