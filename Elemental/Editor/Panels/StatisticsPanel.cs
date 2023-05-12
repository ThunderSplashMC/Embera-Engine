using DevoidEngine.Engine.Rendering;
using ImGuiNET;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using Elemental.Editor.EditorUtils;

namespace Elemental.Editor.Panels
{
    class StatisticsPanel : Panel
    {
        public override void OnInit()
        {
            base.OnInit();
        }

        float deltaTime = 0f;

        public override void OnUpdate(float deltaTime)
        {
            this.deltaTime = deltaTime;
        }

        public override void OnGUIRender()
        {
            ImGui.Begin($"{FontAwesome.ForkAwesome.InfoCircle} Performance");

            if (ImGui.CollapsingHeader("Engine Info"))
            {
                ImGui.TreePush();

                UI.BeginPropertyGrid("##ENGINE_PERF");

                UI.BeginProperty("FPS");
                UI.PropertyText((1 / deltaTime).ToString());
                UI.EndProperty();

                UI.BeginProperty("FrameTime");
                UI.PropertyText((deltaTime).ToString());
                UI.EndProperty();

                UI.BeginProperty("Total Mesh Count");
                UI.PropertyText((Mesh.TotalMeshCount).ToString());
                UI.EndProperty();

                UI.BeginProperty("DrawCalls");
                UI.PropertyText((RenderGraph.Renderer_3D_DrawCalls).ToString());
                UI.EndProperty();

                UI.BeginProperty("RenderPasses");
                UI.PropertyText((Renderer3D.GetPassCount()).ToString());
                UI.EndProperty();

                UI.BeginProperty("Render Device");
                UI.PropertyText(SystemInfo.GetDeviceInfo().RENDERER_NAME);
                UI.EndProperty();

                UI.BeginProperty("Graphics API Ver.");
                UI.PropertyText(SystemInfo.GetDeviceInfo().Major + "." + SystemInfo.GetDeviceInfo().Minor);
                UI.EndProperty();

                UI.EndPropertyGrid();

                ImGui.TreePop();
            }

            ImGui.End();
        }
    }
}
