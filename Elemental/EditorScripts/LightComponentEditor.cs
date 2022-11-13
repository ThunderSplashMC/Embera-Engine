using System;
using System.Collections.Generic;
using DevoidEngine.Elemental.EditorAttributes;
using DevoidEngine.Engine.Components;
using ImGuiNET;
using DevoidEngine.Elemental.EditorUtils;

namespace DevoidEngine.Elemental.EditorScripts
{
    [EditorCustomScript(typeof(LightComponent), typeof(LightComponentEditor))]
    class LightComponentEditor : EditorUI
    {
        public bool x = false;

        public override bool OnInspectorGUI()
        {
            if (baseComp == null) { return false; }

            //LightComponent light = (LightComponent)baseComp;

            //UI.BeginPropertyGrid("lightComp");

            //UI.BeginProperty("Joe :)");

            //UI.PropertyTexture((IntPtr)(light.joe.GetTexture()));

            //UI.EndProperty();

            //UI.EndPropertyGrid();

            return false;
        }
    }
}
