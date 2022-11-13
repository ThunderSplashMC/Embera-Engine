using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;

namespace DevoidEngine.Elemental
{
    abstract class EditorUI
    {
        public Component baseComp;

        public virtual void OnRenderGUI()
        {

        }

        public virtual bool OnInspectorGUI() { return false; }

    }
}
