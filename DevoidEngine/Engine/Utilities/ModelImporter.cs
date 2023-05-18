using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Encodings;
using Assimp;
using OpenTK.Mathematics;
using System.Linq;
using SharpFont;
using System.Text;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Rendering;
using BepuPhysics.Collidables;

namespace DevoidEngine.Engine.Utilities
{
    public class ModelImporter
    {
        public struct ModelData {
            public Mesh mesh;
            public Core.Material material;
        }


        static Dictionary<string, Core.Texture> Textures = new Dictionary<string, Core.Texture>();

        public static Mesh[] LoadModel(string path)
        {

            AssimpContext ImporterObject = new AssimpContext();
            Assimp.Scene scene;
            try
            {
                scene = ImporterObject.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals /* | PostProcessSteps.FlipWindingOrder*/ | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FlipUVs | PostProcessSteps.GenerateUVCoords | PostProcessSteps.OptimizeGraph | PostProcessSteps.OptimizeMeshes);
            }
            catch (Exception E)
            {
                Console.WriteLine("Model was not found at path: " + path + "\nOr Model file was invalid");
                Console.WriteLine("Exception: " + E);
                return null;
            }
            if (scene == null || scene.RootNode == null) return null;
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
                ModelTotalMeshes[i].fileID = Path.GetFileName(path);
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
                Vector3 modifiedVertex = (Vector4.TransformColumn(ToOpenTKMatrix(transform), new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)) * Matrix4.CreateScale(0.02f)).Xyz;
                Vector3 modifiedNormals = (Vector4.TransformColumn(ToOpenTKMatrix(transform), new Vector4((mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, 1))) * Matrix4.CreateScale(0.02f)).Xyz;
                if (mesh.TextureCoordinateChannels[0].Count != 0)
                {
                    // (Matrix4.Mult( * new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)).xyz;
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
                    vertex = new Vertex(modifiedVertex, modifiedNormals, Vector2.Zero);//, new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
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

            int MaterialID = SetupMaterial(mesh.MaterialIndex, scene, path);

            mesh1.MaterialIndex = MaterialID;

            return mesh1;
        }

        static int SetupMaterial(int MaterialIndex, Assimp.Scene scene, string path = "")
        {
            Assimp.Material meshMat = scene.Materials[MaterialIndex];

            Vector3 Albedo = new Vector3(meshMat.ColorDiffuse.R, meshMat.ColorDiffuse.G, meshMat.ColorDiffuse.B);

            Core.Material material = new Core.Material(ShaderLibrary.GetShader("pbr_shader_internal"));
            material.Set("material.albedo", Albedo);
            material.Set("material.metallic", meshMat.Shininess * 0.01f);
            material.Set("material.roughness", meshMat.Reflectivity);
            material.Set("material.emission", new Vector3(meshMat.ColorEmissive.R, meshMat.ColorEmissive.G, meshMat.ColorEmissive.B));
            material.Set("material.emissionStr", 1f);
            material.Set("material.ao", 0.1f);

            Resource? AlbedoResource;
            Resource? RoughnessResource;
            Resource? EmissiveResource;
            Resource? NormalResource;
            TextureSlot RoughnessMap;
            TextureSlot EmissiveMap;
            TextureSlot tex;

            if (meshMat.HasTextureDiffuse && Resources.TryLoad(Path.GetFileName(meshMat.TextureDiffuse.FilePath), out AlbedoResource))
            {
                Console.WriteLine(Path.GetFileName(meshMat.TextureDiffuse.FilePath));
                Core.Texture AlbedoTex = (Texture)AlbedoResource;//CheckTextureExists(CorrectFilePath(meshMat.TextureDiffuse.FilePath, path));
                if (AlbedoTex == null)
                {
                    AlbedoTex = new Core.Texture(CorrectFilePath(meshMat.TextureDiffuse.FilePath, path));
                    AddToTextureDict(CorrectFilePath(meshMat.TextureDiffuse.FilePath, path), AlbedoTex);

                }
                SetWrapping(meshMat.TextureDiffuse.WrapModeU, meshMat.TextureDiffuse.WrapModeV, AlbedoTex);
                material.SetTexture("material.ALBEDO_TEX", AlbedoTex);
                material.Set("USE_TEX_0", 1);
            }

            if (meshMat.GetMaterialTexture(TextureType.Shininess, 0, out RoughnessMap) && Resources.TryLoad(Path.GetFileName(RoughnessMap.FilePath), out RoughnessResource))
            {
                Core.Texture RoughnessTex = (Texture)RoughnessResource;
                if (RoughnessTex == null)
                {
                    RoughnessTex = new Core.Texture(CorrectFilePath(RoughnessMap.FilePath, path));
                    AddToTextureDict(CorrectFilePath(RoughnessMap.FilePath, path), RoughnessTex);
                }
                SetWrapping(RoughnessMap.WrapModeU, RoughnessMap.WrapModeV, RoughnessTex);
                RoughnessTex.ChangeFilterType(Core.FilterTypes.Linear);
                material.SetTexture("material.ROUGHNESS_TEX", RoughnessTex, 1);
                material.Set("USE_TEX_1", 1);
            }

            if (meshMat.GetMaterialTexture(TextureType.Emissive, 0, out EmissiveMap) && Resources.TryLoad(Path.GetFileName(EmissiveMap.FilePath), out EmissiveResource))
            {
                Core.Texture EmissionTex = (Texture)EmissiveResource;
                if (EmissionTex == null)
                {
                    EmissionTex = new Core.Texture(Path.GetFullPath(Path.Join(path, EmissiveMap.FilePath)));
                    AddToTextureDict(Path.GetFullPath(Path.Join(path, EmissiveMap.FilePath)), EmissionTex);
                }
                SetWrapping(EmissiveMap.WrapModeU, EmissiveMap.WrapModeV, EmissionTex);
                material.SetTexture("material.EMISSION_TEX", EmissionTex, 2);
                material.Set("USE_TEX_2", 1);
            }

            if (meshMat.HasTextureNormal && Resources.TryLoad(Path.GetFileName(meshMat.TextureNormal.FilePath), out NormalResource))
            {
                Core.Texture normalTexture = (Texture)NormalResource;
                if (normalTexture != null)
                {
                    normalTexture.ChangeFilterType(Core.FilterTypes.Linear);

                    SetWrapping(meshMat.TextureNormal.WrapModeU, meshMat.TextureNormal.WrapModeV, normalTexture);

                    material.SetTexture("material.NORMAL_TEX", normalTexture, 3);
                    material.Set("USE_TEX_3", 1);
                }
            }

            return RenderGraph.MeshSystem.Submit(material);
        }

        static Core.Texture CheckTextureExists(string path)
        {
            Core.Texture value;
            if (Textures.ContainsKey(path))
            {
                return Textures[path];
            }
            return null;
        }

        static void AddToTextureDict(string path, Core.Texture texture)
        {
            if (!Textures.ContainsKey(path))
            {
                Textures.Add(path, texture);
            }
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

        static string CorrectFilePath(string path, string basePath = null)
        {
            if (IsFullPath(path))
            {
                return path;
            }
            return GetAbsolutePath(basePath, path);
        }

        public static bool IsFullPath(string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        public static String GetAbsolutePath(String basePath, String path)
        {
            if (path == null)
                return null;
            if (basePath == null)
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            else
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            String finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
            {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                else
                    finalPath = Path.Combine(basePath, path);
            }
            else
                finalPath = path;
            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }

        public static Mesh[] LoadDmesh(string path)
        {
            List<string> lines = File.ReadLines(path).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                string data = lines[i];

                int firstOpenBrackInd = data.IndexOf("(");

                string positionNormalSubstring = data.Substring(firstOpenBrackInd + 1, data.IndexOf(")") - 1);

                string positionSubstring = positionNormalSubstring.Substring(firstOpenBrackInd + 1, positionNormalSubstring.Substring(positionNormalSubstring.IndexOf(",")).IndexOf(",") - 1);

                //float positionX = positionNormalSubstring.Substring(firstOpenBrackInd + 1, )

                Console.WriteLine(positionSubstring);


                Vector3 position = new Vector3();
            }
            return new Mesh[0];
        }

        public static void ConvertMeshToFile(Vertex[] vertices)
        {
            string output = "";

            for (int i = 0; i < vertices.Length; i++)
            {
                output += vertices[i].Position.X + "P" + vertices[i].Position.Y + "P" + vertices[i].Position.Z + "|";
                output += vertices[i].Normal.X + "N" + vertices[i].Normal.Y + "N" + vertices[i].Normal.Z + "|";
                output += vertices[i].TexCoord.X + "t" + vertices[i].TexCoord.Y + "|";
                output += vertices[i].Tangent.X + "T" + vertices[i].Tangent.Y + "T" + vertices[i].Tangent.Z + "|";
                output += vertices[i].BiTangent.X + "B" + vertices[i].BiTangent.Y + "B" + vertices[i].BiTangent.Z + "|\n";
            }

            FileStream fs = File.Create("D:\\BlenderProjects\\MeshFile.dmesh");
            fs.Write(ASCIIEncoding.ASCII.GetBytes(output));
        }

    }
}
