using System;
using System.IO;
using System.Collections.Generic;

using Assimp;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Utilities
{
    class ModelImporter
    {
        

        public static Mesh[] LoadModel(string path)
        {
            AssimpContext ImporterObject = new AssimpContext();
            Assimp.Scene scene;
            try
            {
                scene = ImporterObject.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipWindingOrder | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateUVCoords);
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
                Console.WriteLine("Model was not found at path: " + path);
                return null;
            }
            List<Mesh> ModelTotalMeshes = new List<Mesh>();

            string[] splitPath = Path.GetFullPath(path).Split("\\");
            string completepath = string.Join("/", splitPath[0..(splitPath.Length - 1)]);

            void ProcessNode(Assimp.Node node, Assimp.Scene scene)
            {
                for (int i = 0; i < node.MeshIndices.Count; i++)
                {
                    Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                    ModelTotalMeshes.Add(ProcessMesh(mesh, scene, node.Transform, completepath));
                }
                for (int i = 0; i < node.ChildCount; i++)
                {
                    ProcessNode(node.Children[i], scene);
                }
            }
            
            ProcessNode(scene.RootNode, scene);
            ImporterObject.Dispose();

            for (int i = 0; i < ModelTotalMeshes.Count; i++)
            {
                ModelTotalMeshes[i].SetPath(path);
            }

            return ModelTotalMeshes.ToArray();
        }

        public static void ProcessLights()
        {

        }

        public static Mesh ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene, Matrix4x4 transform, string path = "")
        {
            List<Vertex> vertices = new List<Vertex>();
            int[] indices = mesh.GetIndices();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex;
                if (mesh.TextureCoordinateChannels[0].Count != 0)
                {
                    // (Matrix4.Mult( * new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)).xyz;
                    Vector3 modifiedVertex = (Vector4.TransformColumn(ToOpenTKMatrix(transform), new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)) * Matrix4.CreateScale(0.02f)).Xyz;
                    Vector3 modifiedNormals = (Vector4.TransformColumn(ToOpenTKMatrix(transform), new Vector4((mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, 1))) * Matrix4.CreateScale(0.02f)).Xyz;
                    modifiedNormals.Normalize();
                    if (mesh.Tangents.Count > 0 && mesh.BiTangents.Count > 0)
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y), new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z), new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z));
                    } else
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                    }
                }
                else
                {
                    vertex = new Vertex(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z), new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z), Vector2.Zero);//, new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                }
                // process vertex positions, normals and texture coordinates
                vertices.Add(vertex);
            }
            Mesh mesh1 = new Mesh();
            mesh1.name = mesh.Name;
            mesh1.SetVertices(vertices.ToArray());
            if (indices.Length != 0)
            {
                mesh1.SetIndices(indices);
            }
            Assimp.Material meshMat = scene.Materials[mesh.MaterialIndex];

            Vector3 Albedo = new Vector3(meshMat.ColorDiffuse.R, meshMat.ColorDiffuse.G, meshMat.ColorDiffuse.B);
            
            Engine.Core.Material material = new Engine.Core.Material(new Core.Shader("Engine/EngineContent/shaders/pbr"));
            material.Set("material.albedo", Albedo);
            material.Set("material.metallic", meshMat.Shininess * 0.01f);
            material.Set("material.roughness", meshMat.Reflectivity);
            material.Set("material.emission", new Vector3(meshMat.ColorEmissive.R, meshMat.ColorEmissive.G, meshMat.ColorEmissive.B));
            material.Set("material.ao", 0.1f);


            TextureSlot RoughnessMap;
            TextureSlot EmissiveMap;
            TextureSlot tex;

            if (meshMat.HasTextureDiffuse && File.Exists(Path.GetFullPath(Path.Join(path, meshMat.TextureDiffuse.FilePath))))
            {
                Core.Texture AlbedoTex = new Core.Texture(Path.GetFullPath(Path.Join(path, meshMat.TextureDiffuse.FilePath)));

                SetWrapping(meshMat.TextureDiffuse.WrapModeU, meshMat.TextureDiffuse.WrapModeV, AlbedoTex);

                material.SetTexture("material.ALBEDO_TEX", AlbedoTex);
                
                material.Set("USE_TEX_0", 1);
            }

            if (meshMat.GetMaterialTexture(TextureType.Shininess, 0, out RoughnessMap) && File.Exists(Path.GetFullPath(Path.Join(path, RoughnessMap.FilePath))))
            {
                Core.Texture RoughnessTex = new Core.Texture(Path.GetFullPath(Path.Join(path, RoughnessMap.FilePath)));

                SetWrapping(RoughnessMap.WrapModeU, RoughnessMap.WrapModeV, RoughnessTex);

                material.SetTexture("material.ROUGHNESS_TEX", RoughnessTex, 1);
                material.Set("USE_TEX_1", 1);
            }

            if (meshMat.GetMaterialTexture(TextureType.Emissive, 0, out EmissiveMap) && File.Exists(Path.GetFullPath(Path.Join(path, EmissiveMap.FilePath))))
            {
                material.SetTexture("material.EMISSION_TEX", new Core.Texture(Path.GetFullPath(Path.Join(path, EmissiveMap.FilePath))), 2);
                material.Set("USE_TEX_2", 1);
            }

            if (meshMat.HasTextureNormal && File.Exists(Path.GetFullPath(Path.Join(path, meshMat.TextureNormal.FilePath))))
            {
                Core.Texture normalTexture = new Core.Texture(Path.GetFullPath(Path.Join(path, meshMat.TextureNormal.FilePath)));

                SetWrapping(meshMat.TextureNormal.WrapModeU, meshMat.TextureNormal.WrapModeV, normalTexture);

                material.SetTexture("material.NORMAL_TEX", normalTexture, 3);
                material.Set("USE_TEX_3", 1);
            }

            mesh1.SetMaterial(material);
            return mesh1;
        }

        static void SetWrapping(TextureWrapMode modeU, TextureWrapMode modeV, Core.Texture texture)
        {
            if (modeU == TextureWrapMode.Wrap)
            {
                texture.ChangeWrapMode(Core.WrapModeType.Repeat, Core.WrapSide.S);
            }
            if (modeV == TextureWrapMode.Wrap)
            {
                texture.ChangeWrapMode(Core.WrapModeType.Repeat, Core.WrapSide.T);
            }
        }

        public static Matrix4 ToOpenTKMatrix(Matrix4x4 matrix)
        {
            return new Matrix4(matrix.A1, matrix.A2, matrix.A3, matrix.A4, matrix.B1, matrix.B2, matrix.B3, matrix.B4, matrix.C1, matrix.C2, matrix.C3, matrix.C4, matrix.D1, matrix.D2, matrix.D3, matrix.D4);
        }

    }
}
