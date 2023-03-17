using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Core;
using DevoidEngine.Elemental;
using System.IO;

namespace DevoidEngine.Sandbox
{
    class CustomBlockSpawner : Component
    {
        public override string Type { get; } = nameof(CustomBlockSpawner);


        public override void OnStart()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
        }
        public void Generate()
        {
            Mesh[] meshes = ModelImporter.AddMaterialsToScene(gameObject.scene, ModelImporter.LoadModel("D:\\Programming\\Devoid\\ExampleAssets\\gress\\scene.gltf")); ;

            ModelImporter.ConvertMeshToFile(VERTEX_DEFAULTS.GetCubeVertex());

            for (int i = 0; i < 25; i++)
            {
                for (int y = 0; y < 25; y++)
                {
                    GameObject go = gameObject.scene.NewGameObject("H" + i);
                    go.transform.position = new OpenTK.Mathematics.Vector3(i * 0.4f, 0, y * 0.4f);
                    go.transform.scale = new OpenTK.Mathematics.Vector3(10, 10, 10);
                    go.transform.rotation.X = -90;
                    go.AddComponent<MeshHolder>().AddMeshes(meshes);
                    go.AddComponent<MeshRenderer>();
                }
                
            }
        }

    }
}
