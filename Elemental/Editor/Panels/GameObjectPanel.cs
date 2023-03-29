using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using ImGuiNET;
using DevoidEngine.Engine.GUI;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Utilities;
using Elemental.Editor.EditorUtils;
using Elemental.Editor.EditorAttributes;
using DevoidEngine.Engine.Rendering;

namespace Elemental.Editor.Panels
{
    struct CustomEditorScriptItem
    {
        public Type Component;
        public EditorUI EditorUI;
    }

    class GameObjectPanel : Panel
    {
        public GameObject CurrentSelectedGameObject;
        int EmptyTextureIcon;

        List<CustomEditorScriptItem> CustomEditorScriptComponents;

        public override void OnInit()
        {
            EmptyTextureIcon = (new Texture("Editor/Assets/texture-empty-icn.png").GetTexture());
            GetCustomEditorComponentScript();
            

        }

        public void GetCustomEditorComponentScript()
        {
            Type[] types = Assembly.GetAssembly(typeof(EditorUI)).GetTypes();
            CustomEditorScriptComponents = new List<CustomEditorScriptItem>();

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                EditorCustomScript attr = (EditorCustomScript)type.GetCustomAttribute(typeof(EditorCustomScript));

                if (attr != null)
                {
                    CustomEditorScriptComponents.Add(new CustomEditorScriptItem()
                    {
                        Component = attr.type,
                        EditorUI = (EditorUI)Activator.CreateInstance(attr.type1)

                    });
                }
            }
        }

        public void DrawObjectButton(GameObject gameObject, int id)
        {
            bool selected = gameObject == CurrentSelectedGameObject;
            if (selected) { ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1)); }
            ImGui.PushID("##object" + id);
            if (DevoidGUI.Button(FontAwesome.ForkAwesome.DotCircleO + " " + gameObject.name, new Vector2(ImGui.GetWindowWidth(), 20f)))
            {
                CurrentSelectedGameObject = gameObject;
            }
            ImGui.PopID();
            if (selected) { ImGui.PopStyleColor(); }
        }


        int x = 999;

        string ComponentSearch = "";

        public override void OnGUIRender()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0f, 0f));
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0.05f, 0.5f));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, System.Numerics.Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new System.Numerics.Vector2(0, 0.5f));
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.2f));
            ImGui.Begin($"{FontAwesome.ForkAwesome.Sitemap} Hierarchy");

            SceneRegistry sceneRegistry = Editor.EditorScene.GetSceneRegistry();
            GameObject[] gameObjects = sceneRegistry.GetAllGameObjects();

            if (ImGui.BeginTable("#game_obj_hier_panel", 2, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Status");
                ImGui.TableHeadersRow();
                ImGui.TableNextColumn();

                for (int i = 0; i < gameObjects.Length; i++)
                {
                    DrawObjectButton(gameObjects[i], i);
                    ImGui.TableNextColumn();
                    ImGui.Text(gameObjects[i].enabled.ToString());
                    ImGui.TableNextColumn();
                }

                ImGui.EndTable();
            }

            ImGui.PopStyleVar(5);
            ImGui.PopStyleColor();

            ImGui.PushStyleColor(ImGuiCol.Text, (new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1)));

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Delete GameObject"))
                {
                    Editor.EditorScene.RemoveGameObject(CurrentSelectedGameObject);
                }
                if (ImGui.MenuItem("Create GameObject"))
                {
                    Editor.EditorScene.NewGameObject();
                }
                ImGui.EndMenu();
            }

            ImGui.PopStyleColor();

            ImGui.End();

            ImGui.PushStyleColor(ImGuiCol.Text, (new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1)));
            ImGui.Begin($"{FontAwesome.ForkAwesome.InfoCircle} Properties");

            //ImGui.BeginTable("##hihi", 2);
            //ImGui.TableSetupColumn("Prop", 0);
            //ImGui.TableSetupColumn("Val", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableNextColumn();

            //ImGui.Text("Here is a button");

            //ImGui.TableSetColumnIndex(1);
            //ImGui.Button("Hello World");
            //ImGui.TableNextRow();
            //ImGui.TableSetColumnIndex(1);
            //ImGui.Button("Hello World");
            //ImGui.TableNextRow();
            //ImGui.TableSetColumnIndex(0);
            //ImGui.Dummy(new System.Numerics.Vector2(0,0));
            //ImGui.TableSetColumnIndex(1);
            //ImGui.Button("Hello World");


            //ImGui.TableNextRow();
            //ImGui.TableSetColumnIndex(0);

            //ImGui.Text("Here is a button");

            //ImGui.EndTable();


            if (CurrentSelectedGameObject != null)
            {
                UI.BeginPropertyGrid("obj_name_game");
                UI.BeginProperty("Name: ");
                UI.PropertyString(ref CurrentSelectedGameObject.name, false);
                UI.EndProperty();

                UI.BeginProperty("ID: ");
                UI.PropertyText(CurrentSelectedGameObject.ID.ToString());
                UI.EndProperty();

                UI.BeginProperty("Components: ");
                UI.PropertyText(CurrentSelectedGameObject.components.Count.ToString());
                UI.EndProperty();

                UI.EndPropertyGrid();

                Component[] components = CurrentSelectedGameObject.GetAllComponents();
                for (int i = 0; i < components.Length; i++)
                {
                    Component component = components[i];
                    ImGui.PushID("##component" + i);
                    if (ImGui.CollapsingHeader(component.GetType().Name))
                    {
                        ImGui.TreePush();
                        bool found = false;
                        for (int x = 0; x < CustomEditorScriptComponents.Count; x++)
                        {
                            if (CustomEditorScriptComponents[x].Component == component.GetType())
                            {
                                found = true;
                                CustomEditorScriptComponents[x].EditorUI.baseComp = component;
                                if (!CustomEditorScriptComponents[x].EditorUI.OnInspectorGUI()) 
                                {
                                    UI.BeginPropertyGrid("##" + i);
                                    UI.DrawComponentField(component.GetType().GetFields(), (object)component);
                                    DrawMaterialFields();
                                    UI.EndPropertyGrid();
                                };
                            }
                        }
                        if (!found)
                        {
                            UI.BeginPropertyGrid("##" + i);
                            UI.DrawComponentField(component.GetType().GetFields(), (object)component);
                            UI.EndPropertyGrid();
                        }

                        ImGui.TreePop();
                    }
                    ImGui.PopID();
                }

                DrawMaterialFields();
            } else
            {
                ImGui.Text("No GameObject Selected");
            }

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.BeginMenu("Add Component"))
                {
                    ImGui.InputText("##searchbar", ref ComponentSearch, 1024);

                    List<Component> objs = GetAllComponents();
                    for (int i = 0; i < objs.Count; i++) {
                        string ComponentName = objs[i].GetType().Name.ToString();
                        if (ComponentSearch == "")
                        {
                            if (ImGui.MenuItem(ComponentName) && CurrentSelectedGameObject != null)
                            {
                                CurrentSelectedGameObject.AddComponent(objs[i]);
                            }
                        }
                        else if (ComponentSearch.Length < ComponentName.Length && ComponentSearch == ComponentName.ToLower().Substring(0, ComponentSearch.Length))
                        {
                            if (ImGui.MenuItem(ComponentName) && CurrentSelectedGameObject != null)
                            {
                                CurrentSelectedGameObject.AddComponent(objs[i]);
                            }
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Delete GameObject"))
                {
                    Editor.EditorScene.RemoveGameObject(CurrentSelectedGameObject);
                    CurrentSelectedGameObject = null;
                }
            }
            ImGui.PopStyleColor();
            ImGui.End();
        }


        List<Component> Components = new List<Component>(); 

        public List<Component> GetAllComponents()
        {
            List<Component> objects = new List<Component>();
            foreach (Type type in Assembly.GetAssembly(typeof(Component)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Component))))
            {
                objects.Add((Component)Activator.CreateInstance(type));
            }
            return objects;
        }

        void DrawMaterialFields()
        {
            MeshHolder meshHolder = CurrentSelectedGameObject.GetComponent<MeshHolder>();
            if (meshHolder == null) { return; }

            for (int i = 0; i < meshHolder.Meshes.Count; i++)
            {
                if (ImGui.CollapsingHeader("Material " + i))
                {
                    ImGui.TreePush();
                    UI.BeginPropertyGrid("MAT_MSH" + i);

                    Vector3 albedo = meshHolder.Meshes[i].Material.GetVec3("material.albedo");
                    float metallic = meshHolder.Meshes[i].Material.GetFloat("material.metallic");
                    float roughness = meshHolder.Meshes[i].Material.GetFloat("material.roughness");
                    Vector3 emission = meshHolder.Meshes[i].Material.GetVec3("material.emission");
                    TextureAttribute[] TexAttributes = meshHolder.Meshes[i].Material.GetAllTexAttributes();

                    UI.BeginProperty("Albedo");
                    if (GetTextureAttribute(TexAttributes, "material.ALBEDO_TEX"))
                    {
                        UI.PropertyVector3(ref albedo);
                    }
                    UI.EndProperty();

                    UI.BeginProperty("Metallic");
                    UI.PropertyFloat(ref metallic, 0, 1, 0.02f);
                    UI.EndProperty();

                    UI.BeginProperty("Roughness");
                    if (GetTextureAttribute(TexAttributes, "material.ROUGHNESS_TEX"))
                    {
                        UI.PropertyFloat(ref roughness, 0, 1, 0.02f);
                    }
                    UI.EndProperty();

                    UI.BeginProperty("Emission");
                    if (GetTextureAttribute(TexAttributes, "material.EMISSION_TEX"))
                    {
                       UI.PropertyVector3(ref emission);
                    }
                    UI.EndProperty();

                    meshHolder.Meshes[i].Material.Set("material.albedo", albedo);
                    meshHolder.Meshes[i].Material.Set("material.metallic", metallic);
                    meshHolder.Meshes[i].Material.Set("material.roughness", roughness);
                    meshHolder.Meshes[i].Material.Set("material.emission", emission);

                    UI.EndPropertyGrid();
                    ImGui.TreePop();
                }
            }
        }

        bool GetTextureAttribute(TextureAttribute[] textureAttributes, string name)
        {
            for (int x = 0; x < textureAttributes.Length; x++)
            {
                if (textureAttributes[x].AttrName == name)
                {
                    UI.PropertyTexture((IntPtr)(textureAttributes[x].Tex.GetTexture()));
                    UI.BeginProperty("Filter Type");

                    FieldInfo field = textureAttributes[x].Tex.GetType().GetField("FilterType");

                    int prev = (int)field.GetValue(textureAttributes[x].Tex);

                    UI.DrawEnumField(field, textureAttributes[x].Tex);

                    int newv = (int)field.GetValue(textureAttributes[x].Tex);

                    if (newv != prev) 
                    {
                        textureAttributes[x].Tex.ChangeFilterType((FilterTypes)newv);
                    }

                    UI.EndProperty();
                    return false;
                }
            }
            return true;
        }

    }
}
