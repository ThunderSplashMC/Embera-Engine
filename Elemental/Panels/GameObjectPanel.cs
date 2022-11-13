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
using DevoidEngine.Elemental.EditorUtils;
using DevoidEngine.Elemental.EditorAttributes;

namespace DevoidEngine.Elemental.Panels
{
    struct CustomEditorScriptItem
    {
        public Type Component;
        public EditorUI EditorUI;
    }

    class GameObjectPanel : Panel
    {
        GameObject CurrentSelectedGameObject;
        int EmptyTextureIcon;

        List<CustomEditorScriptItem> CustomEditorScriptComponents;

        public override void OnInit()
        {
            EmptyTextureIcon = (new Texture("Elemental/Assets/texture-empty-icn.png").GetTexture());
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
            if (gameObject == CurrentSelectedGameObject)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1));
                ImGui.PushID("##object" + id);
                if (DevoidGUI.Button(FontAwesome.ForkAwesome.Cube + " " + gameObject.name, new Vector2(ImGui.GetWindowWidth(), 25f)))
                {
                    CurrentSelectedGameObject = gameObject;
                }
                ImGui.PopID();
                ImGui.PopStyleColor();
            } else
            {
                ImGui.PushID("##object" + id);
                if (DevoidGUI.Button(FontAwesome.ForkAwesome.Cube + " " + gameObject.name, new Vector2(ImGui.GetWindowWidth(), 25f)))
                {
                    CurrentSelectedGameObject = gameObject;
                }
                ImGui.PopID();
            }
        }


        int x = 999;

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

            for (int i = 0; i < gameObjects.Length; i++)
            {
                DrawObjectButton(gameObjects[i], i);
            }

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Delete GameObject"))
                {
                    Editor.EditorScene.RemoveGameObject(CurrentSelectedGameObject);
                }
                ImGui.EndMenu();
            }



            //ImGui.PopFont();
            ImGui.PopStyleVar(5);
            ImGui.PopStyleColor();

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Create GameObject"))
                {
                    Editor.EditorScene.NewGameObject();
                }
            }

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
            }

            if (ImGui.BeginPopupContextWindow())
            {
                List<Component> objs = GetAllComponents();
                for (int i = 0; i < objs.Count; i++)
                {
                    if (ImGui.MenuItem(objs[i].GetType().Name.ToString()) && CurrentSelectedGameObject != null)
                    {
                        CurrentSelectedGameObject.AddComponent(objs[i]);
                    }
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

    }
}
