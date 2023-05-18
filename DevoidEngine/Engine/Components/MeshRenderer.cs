using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    public class MeshRenderer : Component
    {
        public override string Type { get; } = nameof(MeshRenderer);

        public bool isInitialized = false;

        public List<Mesh> Meshes = new List<Mesh>();
        List<int> MeshIDs = new List<int>();

        public void AddMesh(Mesh mesh)
        {
            Meshes.Add(mesh);
            MeshIDs.Add(RenderGraph.MeshSystem.Submit(mesh));
        }

        public override void OnStart()
        {
            if (isInitialized)
            {
                for (int i = 0; i < MeshIDs.Count; i++)
                {
                    RenderGraph.MeshSystem.RemoveMesh(MeshIDs[i]);
                }
            }

            MeshIDs.Clear();

            for (int i = 0; i < Meshes.Count; i++)
            {
                MeshIDs.Add(RenderGraph.MeshSystem.Submit(Meshes[i]));
            }
        }

        public override void OnRender()
        {
            for (int i = 0; i < MeshIDs.Count; i++)
            {
                RenderGraph.MeshSystem.SetTransform(MeshIDs[i], gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale);
            }
        }
    }
}
