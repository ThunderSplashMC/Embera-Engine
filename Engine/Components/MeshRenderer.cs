using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class MeshRenderer : Component
    {
        [RunInEditMode]

        MeshHolder MeshHolder;

        public override void OnStart()
        {
            MeshHolder = gameObject.GetComponent<MeshHolder>();
        }

        public override void OnRender()
        {
            if (MeshHolder == null) {
                MeshHolder = gameObject.GetComponent<MeshHolder>();
                return;
            }
            for (int i = 0; i < MeshHolder.Meshes.Count; i++)
            {
                Renderer3D.Submit(MeshHolder.Meshes[i], gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale, gameObject.ID);
            }
        }
    }
}
