using System;
using System.Collections.Generic;
using ImGuiNET;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using Elemental.Editor.EditorUtils;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.GUI;
using System.IO;
using System.Runtime.CompilerServices;

namespace Elemental.Editor.Panels
{
    class MaterialPanel : Panel
    {

        EditorThumbnailRenderer thumbnailRenderer = new EditorThumbnailRenderer();

        int matCount = 0;

        List<int> materialThumbnailIndex = new List<int>();

        float padding = 16.0f;
        float thumbnailSize = 85.0f;
        float panelWidth;
        int columnCount;
        float cellSize;

        public override void OnInit()
        {
            matCount = RenderGraph.MeshSystem.GetAllMaterials().Count;

            Renderer3D.AddRenderPass(thumbnailRenderer);



            base.OnInit();
        }

        public override void OnGUIRender()
        {
            return;
            List<Material> materials = RenderGraph.MeshSystem.GetAllMaterials();

            if (matCount != materials.Count)
            {
                thumbnailRenderer.RemoveAllMaterialsFromQueue();
                materialThumbnailIndex.Clear();

                for (int i = 0; i < materials.Count; i++)
                {
                    materialThumbnailIndex.Add(thumbnailRenderer.AddMaterialToQueue(materials[i]));
                }

                matCount = materials.Count;
                thumbnailRenderer.ReRender();
            }

            //ImGui.PushStyleColor(ImGuiCol.Button, System.Numerics.Vector4.Zero);

            ImGui.Begin($"Material Browser");

            if (ImGui.Button("ReRender"))
            {
                thumbnailRenderer.ReRender();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1));

            ImGui.TextWrapped("Materials");
            ImGui.PopStyleColor();

            panelWidth = ImGui.GetContentRegionAvail().X;
            cellSize = thumbnailSize + padding + ImGui.GetStyle().FramePadding.X;
            columnCount = (int)(panelWidth / cellSize);


            if (columnCount < 1)
                columnCount = 1;

            ImGui.Columns(columnCount, "", false);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1.0f);
            for (int i = 0 ; i < materials.Count; i++)
            {
                ImGui.PushID("folder-" + i);

                ImDrawListPtr drawData = ImGui.GetWindowDrawList();


                ImGui.ImageButton((IntPtr)thumbnailRenderer.GetThumbnail(materialThumbnailIndex[i]), new System.Numerics.Vector2(thumbnailSize, thumbnailSize));

                ImGui.TextWrapped("Material " + i);
                ImGui.NextColumn();

                ImGui.PopID();
            }

            ImGui.Columns(1);

            ImGui.PopStyleVar();


            ImGui.End();
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
        }

        public override void OnRender()
        {
            base.OnRender();
        }

        public override void OnResize(int width, int height)
        {
            base.OnResize(width, height);
        }

    }
}
