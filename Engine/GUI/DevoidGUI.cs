using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenTK.Mathematics;

namespace DevoidEngine.Engine.GUI
{
    class DevoidGUI
    {
        

        public static void DrawVector3Control(string label, ref Vector3 vector, float resetVal = 0f, float stepVal = 1f)
        {
            System.Numerics.Vector2 framepadding = ImGui.GetStyle().FramePadding;
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0f);
            ImGui.PushID(label);
            ImGui.Columns(4);
            if (label != "")
            {
                ImGui.Text(label);
            }
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 0));
            
            ImGui.NextColumn();

            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.9f, 0.1f, 0f, 1));
            ImGui.PushItemWidth(-1f);
            if (ImGui.Button("X", new System.Numerics.Vector2(22 + framepadding.X, 22 + framepadding.Y))) {
                vector.X = resetVal;
            }
            ImGui.SameLine();
            ImGui.DragFloat("##" + label + "x", ref vector.X, stepVal);
            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.NextColumn();
            
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.14f, 0.06f, 0.78f, 1));
            ImGui.PushItemWidth(-1f);
            if (ImGui.Button("Y", new System.Numerics.Vector2(22 + framepadding.X, 22 + framepadding.Y)))
            {
                vector.Y = resetVal;
            }
            ImGui.PopStyleColor();
            ImGui.SameLine();
            ImGui.DragFloat("##" + label + "y", ref vector.Y, stepVal);
            ImGui.PopItemWidth();
            ImGui.NextColumn();

            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.15f, 0.78f, 0.06f, 1));
            ImGui.PushItemWidth(-1f);
            if (ImGui.Button("Z", new System.Numerics.Vector2(22 + framepadding.X, 22 + framepadding.Y)))
            {
                vector.Z = resetVal;
            }
            ImGui.SameLine();
            ImGui.DragFloat("##" + label + "z", ref vector.Z, stepVal);
            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.Columns(1);
            ImGui.PopStyleVar();
            ImGui.PopID();

            ImGui.PopStyleVar();

        }

        public static void DrawCheckboxField(string label, ref bool value)
        {
            ImGui.PushID(label);
            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.Checkbox("##" + label, ref value);
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawTexturePreview(string label, int texture, Vector2 size)
        {
            ImGui.PushID(label);

            if (texture == 0)
            {

            } else
            {
                ImGui.Image((IntPtr)texture, new System.Numerics.Vector2(size.X, size.Y));
            }

            ImGui.PopID();
        }

        public static void DrawTextField(string label, string text)
        {
            ImGui.PushID(label);
            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.Text(text);
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawField(string label, string fieldName)
        {
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
            ImGui.Button(fieldName, new System.Numerics.Vector2(-1f, 22 + ImGui.GetStyle().FramePadding.Y));
            ImGui.PopStyleVar(2);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawTexField(string label, int texture)
        {
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
            DrawTexturePreview("##" + label, texture, new Vector2(64, 64));
            ImGui.PopStyleVar(2);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static bool DrawButtonField(string label, string fieldName)
        {
            bool isClicked = false;

            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(2, 2));
            isClicked = ImGui.Button(fieldName, new System.Numerics.Vector2(-1f, 22f));
            ImGui.PopStyleVar();
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
            return isClicked;
        }

        public static void DrawEnumField(string label,ref int currentIndex, string[] values, int length)
        {
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
            ImGui.Combo("##combo" + label, ref currentIndex, values, length);
            ImGui.PopStyleVar(2);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawColorField(string label, ref Color4 value)
        {
            System.Numerics.Vector4 col = new System.Numerics.Vector4(value.R, value.G, value.B, value.A);
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.ColorEdit4("##" + label, ref col, ImGuiColorEditFlags.Float);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
            value.R = col.X;
            value.G = col.Y;
            value.B = col.Z;
            value.A = col.W;
        }

        public static void DrawColorVecField(string label, ref Vector3 value, bool rgbFields = true)
        {
            ImGuiColorEditFlags flags = ImGuiColorEditFlags.Float;
            if (!rgbFields)
            {
                flags |= ImGuiColorEditFlags.NoInputs;
            }

            System.Numerics.Vector4 col = new System.Numerics.Vector4(value.X, value.Y, value.Z, 1);
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.ColorEdit4("##" + label, ref col, flags);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
            value.X = col.X;
            value.Y = col.Y;
            value.Z = col.Z;
        }

        public static void DrawFloatField(string label, ref float value, float speed = 0.01f, float min = float.NegativeInfinity, float max = float.PositiveInfinity)
        {
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.DragFloat("##" + label, ref value, speed, min, max);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawIntField(string label, ref int value, float speed = 0.01f, int min = int.MinValue, int max = int.MaxValue)
        {
            ImGui.PushID(label);

            ImGui.Columns(2);
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.PushItemWidth(-1);
            ImGui.DragInt("##" + label, ref value, speed, min, max);
            ImGui.PopItemWidth();
            ImGui.Columns(1);
            ImGui.PopID();
        }

        public static void DrawColor4Control(string label, ref Vector4 color)
        {
            ImGui.Text(label);
            ImGui.SameLine();
            System.Numerics.Vector4 vector = new System.Numerics.Vector4(color.X, color.Y, color.Z, color.W);
            ImGui.ColorEdit4("##" + label, ref vector, ImGuiColorEditFlags.NoInputs);
            color = new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }
        public static void DrawColor3Control(string label, ref Vector3 color)
        {
            ImGui.Text(label);
            ImGui.SameLine();
            System.Numerics.Vector3 vector = new System.Numerics.Vector3(color.X, color.Y, color.Z);
            ImGui.ColorEdit3("##" + label, ref vector, ImGuiColorEditFlags.NoInputs);
            color = new Vector3(vector.X, vector.Y, vector.Z);
        }


        public static void Image(IntPtr texture, Vector2 size)
        {
            ImGui.Image(texture, new System.Numerics.Vector2(size.X, size.Y), new System.Numerics.Vector2(1,1), new System.Numerics.Vector2(0,0));
        }

        public static bool Button(string label, Vector2 size, ImGuiButtonFlags flags)
        {
            System.Numerics.Vector2 framepadding = ImGui.GetStyle().FramePadding;
            return ImGui.Button(label, new System.Numerics.Vector2(size.X + framepadding.X, size.Y + framepadding.Y));
        }


        public static bool Button(string label, Vector2 size)
        {
            System.Numerics.Vector2 framepadding = ImGui.GetStyle().FramePadding;
            return ImGui.Button(label, new System.Numerics.Vector2(size.X + framepadding.X, size.Y + framepadding.Y));
        }

        public static float GetWindowHeight()
        {
            return ImGui.GetWindowHeight();
        }

        public static float GetWindowWidth()
        {
            return ImGui.GetWindowWidth();
        }
    }
}
