using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Components
{
    public class MeshHolder : Component
    {
        public override string Type { get; } = nameof(MeshHolder);
        public List<Mesh> Meshes = new List<Mesh>();

        public Type HelloWorld;

        public override void OnStart()
        {
            base.OnStart();
        }

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
