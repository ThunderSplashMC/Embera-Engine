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
using Assimp;

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
            EmptyTextureIcon = (new Texture("Editor/Assets/texture-empty-icn.png").GetRendererID());
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

            ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1));
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
            ImGui.PopStyleColor(2);

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
                PresetObjectMenu();
                ImGui.EndMenu();
            }

            ImGui.PopStyleColor();

            ImGui.End();

            ImGui.PushStyleColor(ImGuiCol.Text, (new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1)));
            ImGui.Begin($"{FontAwesome.ForkAwesome.InfoCircle} Properties");


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
                        //ImGui.BeginGroup();

                        ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1));

                        //ImGui.BeginChild("component" + i);
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

                        //ImGui.EndChild();

                        //ImGui.EndGroup();
                        //ImGui.GetWindowDrawList().AddRect(new System.Numerics.Vector2(ImGui.GetItemRectMin().X - 10, ImGui.GetItemRectMin().Y), new System.Numerics.Vector2(ImGui.GetItemRectMax().X + (ImGui.GetContentRegionAvail().X), ImGui.GetItemRectMax().Y), UI.ToUIntA(new Vector4(0.1f, 0.1f, 0.1f, 0.5f)), 5f, ImDrawFlags.RoundCornersBottomRight | ImDrawFlags.RoundCornersBottomLeft, 2f);
                        ImGui.PopStyleColor();
                        ImGui.TreePop();

                    }
                    else
                    {
                        if (ImGui.BeginPopupContextItem())
                        {
                            if (ImGui.MenuItem("Remove Component"))
                            {
                                CurrentSelectedGameObject.RemoveComponent(component);
                            }
                        }
                    }
                    ImGui.PopID();
                }
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
                ImGui.EndPopup();
            }
            ImGui.PopStyleColor();

            ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 32));

            if (ImGui.BeginDragDropTarget())
            {
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && Editor.PROJECT_ASSEMBLY != null)
                {
                    Type[] types = Editor.PROJECT_ASSEMBLY.GetTypes();
                    DragFileItem item = Editor.DragDropService.GetDragFile();
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (types[i].Name == item.fileName.Substring(0, item.fileName.Length - 3))
                        {
                            if (types[i].BaseType == typeof(DevoidScript))
                            {
                                CurrentSelectedGameObject.AddComponent((Component)Activator.CreateInstance(types[i]));
                            }
                            
                        }
                    }
                }
                ImGui.EndDragDropTarget();
            }

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
            if (Editor.PROJECT_ASSEMBLY != null)
            {
                foreach (Type type in Editor.PROJECT_ASSEMBLY.GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(DevoidScript))))
                {
                    objects.Add((Component)Activator.CreateInstance(type));
                }
            }
            return objects;
        }

        void PresetObjectMenu()
        {
            if (ImGui.BeginMenu("Presets"))
            {
                if (ImGui.MenuItem("Panel"))
                {
                    GameObject gObject = Editor.EditorScene.NewGameObject("Panel");
                    UITransform transform = gObject.AddComponent<UITransform>();
                    gObject.AddComponent<UIImage>();

                    transform.Position = new Vector2(RenderGraph.ViewportWidth/2, RenderGraph.ViewportHeight/2);
                    transform.Size = new Vector2(256);
                    
                }
                ImGui.EndMenu();
            }
        }

    }
}
