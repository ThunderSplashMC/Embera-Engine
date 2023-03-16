using System;
using System.Collections.Generic;
using DevoidEngine.Elemental.EditorUtils;
using DevoidEngine.Elemental;
using DevoidEngine.Elemental.EditorAttributes;

namespace DevoidEngine.Sandbox
{
    [EditorCustomScript(typeof(CustomBlockSpawner), typeof(BlockSpawnerEditorClass))]
    class BlockSpawnerEditorClass : EditorUI
    {
        public override bool OnInspectorGUI()
        {
            if (baseComp == null) { return false; }

            CustomBlockSpawner cbs = (CustomBlockSpawner)baseComp;

            if (UI.DrawButton("Generate"))
            {
                cbs.Generate();
            }



            return true;
        }

    }
}
