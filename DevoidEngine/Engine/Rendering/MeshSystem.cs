using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Utilities;
using ImGuiNET;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using static Assimp.Metadata;
using DevoidEngine.Engine.Core;
using System.Linq;

namespace DevoidEngine.Engine.Rendering
{
    public struct DrawItem
    {
        public Mesh mesh;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        // Distance from camera
        public float DistFromView;
        public object associateObject;
    }

    public class MeshSystem
    {
        List<Material> Materials = new List<Material>();
        Dictionary<int, DrawItem> DrawCommands = new Dictionary<int, DrawItem>();


        public MeshSystem()
        {

        }

        public List<Material> GetAllMaterials()
        {
            return Materials;
        }

        public Material GetMaterial(int index)
        {
            return Materials[index];
        }

        /// <summary>
        /// Adds a mesh to the mesh system.
        /// </summary>
        /// <param name="material"></param>
        /// <returns>An index/id to identify the mesh</returns>
        public int Submit(Material material)
        {
            Materials.Add(material);
            return Materials.Count - 1;
        }

        /// <summary>
        /// Adds a mesh to the mesh system.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>An index/id to identify the mesh</returns>
        public int Submit(Mesh mesh)
        {
            DrawItem item = new DrawItem()
            {
                mesh = mesh,
                position = Vector3.Zero,
                rotation = Vector3.Zero,
                scale = Vector3.One
            };

            int ID = (int)(UtilRandom.GetInt(0, RenderGraph.MAX_MESH_COUNT));
            DrawCommands[ID] = item;
            return ID;
        }

        /// <summary>
        /// Adds a mesh to the mesh system.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns>An index/id to identify the mesh</returns>
        public int Submit(Mesh mesh, Vector3 position, Vector3 rotation, Vector3 scale, object associateObject = null)
        {
            DrawItem item = new DrawItem()
            {
                mesh = mesh,
                position = position,
                rotation = rotation,
                scale = scale,
                DistFromView = Vector3.Distance(RenderGraph.Camera.position, position),
                associateObject = associateObject
            };

            int ID = (int)(UtilRandom.GetInt(0, RenderGraph.MAX_MESH_COUNT));
            DrawCommands[ID] = item;
            return ID;
        }

        public void RemoveMesh(int meshID)
        {
            DrawCommands.Remove(meshID);
        }


        public List<DrawItem> GetRenderDrawList()
        {
            return DrawCommands.Values.ToList<DrawItem>();
        }

        /// <summary>
        /// Sets the transform of a mesh given its ID.
        /// </summary>
        /// <param name="meshID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public void SetTransform(int meshID, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            DrawItem item = DrawCommands[meshID];
            item.position = position;
            item.rotation = rotation;
            item.scale = scale;
            item.DistFromView = Vector3.Distance(RenderGraph.Camera.position, position);
            DrawCommands[meshID] = item;
        }

        /// <summary>
        /// Renders all the meshes in the mesh system.
        /// </summary>
        public void Render()
        {
            List<DrawItem> DrawList = new List<DrawItem>();

            foreach(KeyValuePair<int, DrawItem> Entry in DrawCommands)
            {
                DrawList.Add(Entry.Value);
            }

            DrawList.Sort((x, y) => x.DistFromView.CompareTo(y.DistFromView));

            for (int i = 0; i < DrawList.Count; i++)
            {
                DrawItem item = DrawList[i];

                Shader shader = Materials[item.mesh.MaterialIndex].GetShader();

                Renderer3D.UploadModelData(shader, item.position, item.rotation, item.scale);
                Renderer3D.UploadCameraData(shader);
                Renderer3D.UploadLightingData(shader);

                item.mesh.Draw();
            }
        }

    }
}
