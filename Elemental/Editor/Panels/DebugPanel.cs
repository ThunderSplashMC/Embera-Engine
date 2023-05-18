using System;
using System.Collections.Generic;
using Elemental.Editor.EditorUtils;

namespace Elemental.Editor.Panels
{
    class DebugPanel : Panel
    {
        public override void OnInit()
        {
            base.OnInit();
        }


        public override void OnGUIRender()
        {
            NodeManager.BeginNodeEditor("##DEBUG");

            NodeManager.BeginNode("##BeginNode", "Debug #1", new OpenTK.Mathematics.Vector2(30, 30), new OpenTK.Mathematics.Vector2(256, 340));

            NodeManager.PropertyText("Hello World", "This is the value", new OpenTK.Mathematics.Vector2(0));

            NodeManager.EndNode();

            NodeManager.EndNodeEditor();
        }
    }
}
