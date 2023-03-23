using System;
using System.Collections.Generic;
using ImGuiNET;

namespace DevoidEngine.Elemental.Panels
{
    class ConsolePanel : Panel
    {
        public List<string> History = new List<string>();
        public string HistoryO = "";
        public int messageCount;

        public override void OnGUIRender()
        {
            ImGui.Begin($"{FontAwesome.ForkAwesome.Terminal} Console Output");

            ImGui.TextWrapped(HistoryO);

            ImGui.End();

            base.OnGUIRender();
        }

        public void LOG(string msg)
        {
            if (messageCount > 10000)
            {
                messageCount = 0;
                HistoryO = "";
                HistoryO += "Message Log Overflow: Exceeds 10000\nMessages Cleared";
            }
            HistoryO = messageCount + ". " + msg + "\n" + HistoryO;
            messageCount += 1;
        }
    }
}
