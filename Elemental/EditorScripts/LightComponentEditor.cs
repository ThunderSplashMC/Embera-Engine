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

        //public int Intensity = 1;
        //public LightType Lighttype = LightType.PointLight;
        //public Color4 color = new Color4(1f, 1f, 1f, 1f);
        //public Vector3 direction = new Vector3(0, -1, 0);
        //public float cutOff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
        //public float OuterCutSoftness;
        //public float OuterCutOff;

        public override bool OnInspectorGUI()
        {
            if (baseComp == null) { return false; }

            LightComponent light = (LightComponent)baseComp;

            UI.BeginPropertyGrid("lightComp");

            UI.BeginProperty("Intensity");
            UI.PropertyInt(ref light.Intensity);
            UI.EndProperty();

            UI.BeginProperty("Light Type");
            UI.DrawEnumField(light.GetType().GetField("Lighttype"), light);
            UI.EndProperty();

            UI.BeginProperty("Light Color");
            UI.PropertyColor4(ref light.color);
            UI.EndProperty();

            if (light.Lighttype == LightComponent.LightType.SpotLight)
            {
                UI.BeginProperty("Spot Direction");
                UI.PropertyVector3(ref light.direction);
                UI.EndProperty();

                UI.BeginProperty("Other Properties");
                UI.PropertyFloat(ref light.cutOff);
                UI.PropertyFloat(ref light.OuterCutOff);
                UI.PropertyFloat(ref light.OuterCutSoftness);
                UI.EndProperty();
            }

            UI.EndPropertyGrid();

            return true;
        }
    }
}
