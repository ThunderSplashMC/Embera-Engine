using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ImGuiNET;
using System.Reflection;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Elemental.EditorUtils
{
    public static class UI
    {
        public static bool Button(string label, Vector2 size)
        {
            return ImGui.Button(label, new System.Numerics.Vector2(size.X, size.Y));
        }

        static bool firstProperty_, propertyCurr_, firstField_;
        static int propertyCount;
        static string propertyLabel;

        public static void BeginPropertyGrid(string id)
        {
            propertyCount = 0;
            firstProperty_ = true;
            ImGui.BeginTable("##" + id, 2);
            ImGui.TableSetupColumn("Prop", 0);
            ImGui.TableSetupColumn("Val", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();
        }

        public static void EndPropertyGrid()
        {
            ImGui.EndTable();
        }

        public static void BeginProperty(string fieldname)
        {
            propertyCount += 1;
            propertyCurr_ = true;
            firstField_ = true;
            propertyLabel = fieldname;

            if (propertyCount > 1)
            {
                ImGui.TableNextRow();
            }

            ImGui.TableSetColumnIndex(0);

            ImGui.Separator();

            ImGui.TextWrapped(fieldname);


            
        }

        public static void EndProperty()
        {
            propertyCurr_ = false;
            firstField_ = true;
        }

        public static void NextField()
        {
            if (firstField_)
            {
                ImGui.TableSetColumnIndex(1);

                ImGui.Separator();
                firstField_ = false;
                return;
            }
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);

            ImGui.Separator();

        }

        public static void PropertyInt(ref int value)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputInt("##" + propertyCount + propertyLabel, ref value);
        }

        public static void PropertyVector3(ref Vector3 value, float speed = 0.2f, float min = float.MinValue, float max = float.MaxValue)
        {
            NextField();

            System.Numerics.Vector3 vec3 = new System.Numerics.Vector3(value.X, value.Y, value.Z);

            ImGui.SetNextItemWidth(-1);
            ImGui.DragFloat3("##" + propertyCount + propertyLabel, ref vec3, speed, min, max);

            value = new Vector3(vec3.X, vec3.Y, vec3.Z);
        }

        public static void PropertyColor4(ref Color4 value)
        {
            NextField();

            System.Numerics.Vector4 color4 = new System.Numerics.Vector4(value.R, value.G, value.B, value.A);

            ImGui.SetNextItemWidth(-1);
            ImGui.ColorEdit4("##" + propertyCount + propertyLabel, ref color4);
            value = new Color4(color4.X, color4.Y, color4.Z, color4.W);
        }

        public static void PropertyFloat(ref float value, float min = float.MinValue, float max = float.MaxValue, float speed = 0.2f)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            ImGui.DragFloat("##" + propertyCount + propertyLabel, ref value, speed, min, max);
        }

        public static void PropertyEnum(ref int currentItem, string[] items, int item_count)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            ImGui.Combo("##" + propertyCount + propertyLabel, ref currentItem, items, item_count);
        }

        public static void PropertyBool(ref bool value)
        {
            NextField();
            ImGui.SetNextItemWidth(-1);
            ImGui.Checkbox("##" + propertyCount + propertyLabel, ref value);
        }

        public static void PropertyTexture(IntPtr value)
        {
            NextField();
            ImGui.Image(value, new System.Numerics.Vector2(ImGui.GetColumnWidth() * 0.5f, ImGui.GetColumnWidth() * 0.5f));
        }

        public static void PropertyText(string value)
        {
            NextField();

            ImGui.SetNextItemWidth(-1);
            ImGui.Text(value);
        }

        public static void PropertyType(Type fieldType)
        {
            NextField();

            ImGui.SetNextItemWidth(-1);
            ImGui.Button(fieldType.Name);
        }

        public static void DrawComponentField(FieldInfo[] fieldInfo, object component)
        {

            for (int i = 0; i < fieldInfo.Length; i++)
            {

                FieldInfo field = fieldInfo[i];

                if (field.IsPrivate || field.DeclaringType == typeof(Component)) { continue; }

                BeginProperty(field.Name);

                if (field.FieldType == typeof(int))
                {
                    DrawIntField(field, component);
                }
                else if (field.FieldType == typeof(float))
                {

                    DrawFloatField(field, component);
                }
                else if (field.FieldType == typeof(Vector3))
                {

                    DrawVec3Field(field, component);
                }
                else if (field.FieldType == typeof(bool))
                {
                    DrawBoolField(field, component);
                }
                else if (field.FieldType == typeof(Color4))
                {
                    DrawColor4Field(field, component);
                }
                else if (field.FieldType.IsEnum)
                {
                    DrawEnumField(field, component);
                }
                else if (field.FieldType == typeof(Texture))
                {
                    DrawTextureField(field, component);
                }
                else
                {
                    DrawTypeField(field, component);
                }

                EndProperty();
            }
        }

        public static void DrawIntField(FieldInfo field, object component)
        {
            int val = (int)field.GetValue(component);
            PropertyInt(ref val);
            field.SetValue(component, val);
        }

        public static void DrawFloatField(FieldInfo field, object component)
        {
            float val = (float)field.GetValue(component);
            PropertyFloat(ref val);
            field.SetValue(component, val);
        }

        public static void DrawTextureField(FieldInfo field, object component)
        {
            Texture val = (Texture)field.GetValue(component);
            PropertyTexture((IntPtr)(val == null ? 0 : val.GetTexture()));
            field.SetValue(component, val);
        }

        public static void DrawTypeField(FieldInfo field, object component)
        {
            PropertyType(field.FieldType);
        }

        public static void DrawVec3Field(FieldInfo field, object component)
        {
            Vector3 val = (Vector3)field.GetValue(component);
            PropertyVector3(ref val);
            field.SetValue(component, val);
        }

        public static void DrawColor4Field(FieldInfo field, object component)
        {
            Color4 val = (Color4)field.GetValue(component);
            PropertyColor4(ref val);
            field.SetValue(component, val);
        }
        public static void DrawBoolField(FieldInfo field, object component)
        {
            bool val = (bool)field.GetValue(component);
            PropertyBool(ref val);
            field.SetValue(component, val);
        }

        public static void DrawEnumField(FieldInfo field, object component)
        {
            List<string> vs = new List<string>();
            foreach (var v in field.FieldType.GetEnumValues())
            {
                vs.Add(v.ToString());
            }

            int value = (int)field.GetValue(component);
            
            PropertyEnum(ref value, vs.ToArray(), vs.Count);
            field.SetValue(component, field.FieldType.GetEnumValues().GetValue(value));
        }

    }
}
