using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Assimp;
using BulletSharp.SoftBody;
using ImGuiNET;
using Microsoft.VisualBasic;
using OpenTK.Mathematics;
using YamlDotNet.Core.Tokens;
using static System.Net.Mime.MediaTypeNames;

namespace DevoidEngine.Elemental.EditorUtils
{
    class Node
    {
        public string name;

        public int x, y, w, h;
        public int clickprevx, clickprevy;
        public int nodetitleHeight = 35;
        public int nodetitlePadding = 25;
        public int defaultPropertyPaddingY = 5;

        public Vector2 clickPoint;

        public bool isDragging;

        public List<NodeProperty> properties = new List<NodeProperty>();
        public float totalPropertyPixelHeight = 0;
    }

    class NodeCollection
    {
        public string nodeCollectionId;
        public string CurrentNode;

        public Vector2 EditorPosition;
        public bool EditorShown;

        public Dictionary<string, Node> nodes;

        public List<NodeLink> links = new List<NodeLink>();
    }

    abstract class NodeProperty
    {
        public abstract NodePropertyType type { get; }
        public int id;
        public string propertyName;
        public float propertyPositionY;
        public float propertyOffsetPositionY;
        public Vector2 propertyPadding = new Vector2(0,0);
    }

    class NodeLink
    {
        public System.Numerics.Vector2 Outposition, InPosition;
        public Node nodeOut, nodeIn;
        public NodeProperty propertyOut, propertyIn;
    }

    class NodePropertyText : NodeProperty
    {
        public override NodePropertyType type => NodePropertyType.Text;
        public string value;
        public string previousValue;
    }

    class NodePropertyFloatInput : NodeProperty
    {
        public override NodePropertyType type => NodePropertyType.FloatInput;
        public float value;
        public float previousValue;
    }

    enum NodePropertyType
    {
        Text,
        FloatInput
    }

    class NodeManager
    {

        public static Dictionary<string, NodeCollection> NodeEditors = new Dictionary<string, NodeCollection>();
        private static string CurrentNodeCollection = null;

        public static void BeginNodeEditor(string id)
        {
            if (!NodeEditors.ContainsKey(id))
            {
                NodeEditors.Add(id, new NodeCollection()
                {
                    nodeCollectionId = id,
                    nodes = new Dictionary<string, Node>()
                });
            }
            ImGui.Begin(id, ImGuiWindowFlags.NoScrollbar);
            
            CurrentNodeCollection = id;
            NodeEditors[CurrentNodeCollection].EditorPosition = new Vector2(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y);
            NodeEditors[CurrentNodeCollection].EditorShown = true;
            DrawGridLines();
            DrawNodeLinks(new System.Numerics.Vector2(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y));
        }

        public static void EndNodeEditor()
        {
            ImGui.InvisibleButton("##" + "aa", ImGui.GetContentRegionAvail());
            ImGui.End();
            CurrentNodeCollection = null;
        }


        public static void BeginNode(string id, string nodeTitle, Vector2 Startposition, Vector2 Size)
        {
            if (!NodeEditors[CurrentNodeCollection].nodes.ContainsKey(id))
            {
                NodeEditors[CurrentNodeCollection].nodes[id] = (
                    new Node()
                    {
                        name = nodeTitle,
                        x = (int)Startposition.X,
                        y = (int)Startposition.Y,
                        h = (int)Size.X,
                        w = (int)Size.Y,
                    }
                );
            }

            Vector2 WindowPosition = NodeEditors[CurrentNodeCollection].EditorPosition;
            Node node = NodeEditors[CurrentNodeCollection].nodes[id];
            NodeEditors[CurrentNodeCollection].CurrentNode = id;

            System.Numerics.Vector2 mousePos = ImGui.GetMousePos();
            if (mousePos.X > WindowPosition.X + node.x && mousePos.X < WindowPosition.X + node.w + node.x && mousePos.Y > WindowPosition.Y + node.y && mousePos.Y < WindowPosition.Y + node.y + node.nodetitleHeight)
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    node.clickPoint.X = ((node.x) + (mousePos.X - node.x));
                    node.clickPoint.Y = ((node.y) + (mousePos.Y - node.y));

                    node.clickprevx = node.x;
                    node.clickprevy = node.y;
                    node.isDragging = true;
                }
            }
            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && node.isDragging)
            {
                node.x = (int)mousePos.X - ((int)node.clickPoint.X - node.clickprevx);
                node.y = (int)mousePos.Y - ((int)node.clickPoint.Y - node.clickprevy);
            }


            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                node.isDragging = false;
            }
        }

        public static void PropertyText(string name, string value, Vector2 padding)
        {

            if (CheckPropertyExists(name) == -1)
            {
                NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties.Add(new NodePropertyText() { value = value, propertyPadding = padding, propertyName = name });
            }
        
        }

        public static void PropertyFloat(string name, ref float value, Vector2 padding)
        {
            int index = CheckPropertyExists(name);
            if (index == -1)
            {
                AddProperty(new NodePropertyFloatInput() { value = value, propertyPadding = padding, propertyName = name});
            } 
            else
            {
                NodePropertyFloatInput nodeProperty = (NodePropertyFloatInput)(NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties[index]);
                
                if (value != nodeProperty.previousValue)
                {
                    nodeProperty.value = value;
                    nodeProperty.previousValue = value;
                } else
                {
                    value = nodeProperty.value;
                }
            }
        }

        public static int CheckPropertyExists(string name)
        {
            for(int i = 0; i <  NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties.Count; i++)
            {
                if (NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties[i].propertyName == name)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void NextProperty(Node currentNode, NodeProperty currentProperty)
        {
            currentNode.totalPropertyPixelHeight += 30 + currentNode.defaultPropertyPaddingY + currentProperty.propertyPadding.Y;
        }

        public static void DrawProperty(NodePropertyText property, Node node, System.Numerics.Vector2 windowPos)
        {
            ImGui.GetWindowDrawList().AddText(new System.Numerics.Vector2(windowPos.X + node.x + 20, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + node.totalPropertyPixelHeight + property.propertyPadding.Y + node.defaultPropertyPaddingY), UI.ToUIntA(new Vector4(0.5f, 0.5f, 0.5f, 1)), property.propertyName);
            ImGui.GetWindowDrawList().AddText(new System.Numerics.Vector2(windowPos.X + node.x + ImGui.CalcTextSize(property.propertyName).X + 40 + property.propertyPadding.X, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + node.totalPropertyPixelHeight + property.propertyPadding.Y + node.defaultPropertyPaddingY), UI.ToUIntA(new Vector4(1, 1, 1, 1)), property.value);
            property.propertyPositionY = (ImGui.GetFrameHeight() / 2) + node.totalPropertyPixelHeight;
            node.totalPropertyPixelHeight += ImGui.CalcTextSize(property.value).Y + (int)property.propertyPadding.Y + node.defaultPropertyPaddingY;
            property.propertyOffsetPositionY = ImGui.CalcTextSize(property.value).Y / 2;
        }

        public static void DrawProperty(NodePropertyFloatInput property, Node node, System.Numerics.Vector2 windowPos)
        {
            ImGui.GetWindowDrawList().AddText(new System.Numerics.Vector2(windowPos.X + node.x + 20, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + node.totalPropertyPixelHeight + property.propertyPadding.Y + node.defaultPropertyPaddingY), UI.ToUIntA(new Vector4(0.5f, 0.5f, 0.5f, 1)), property.propertyName);
            ImGui.SetNextItemWidth(node.w - property.propertyPadding.X - 200);
            ImGui.SetCursorScreenPos(new System.Numerics.Vector2(node.x + ImGui.CalcTextSize(property.propertyName).X + 40 + property.propertyPadding.X + windowPos.X, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + node.totalPropertyPixelHeight + property.propertyPadding.Y + node.defaultPropertyPaddingY));
            ImGui.DragFloat("##property" + node.name + node.properties.Count, ref property.value);

            property.propertyPositionY = (ImGui.GetFrameHeight() / 2) + node.totalPropertyPixelHeight;
            NextProperty(node, property);
            property.propertyOffsetPositionY = ImGui.GetFrameHeight()/2;
        }

        public static void DrawCircle(NodeProperty property, Node node, System.Numerics.Vector2 windowPos)
        {
            System.Numerics.Vector2 pos = new System.Numerics.Vector2(windowPos.X + node.x, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + property.propertyPositionY + property.propertyPadding.Y);

            ImGui.GetWindowDrawList().AddCircleFilled(pos, 5, UI.ToUint(new Vector4i(255,255,255, 255)));

            HandleLinkCreation(pos, node, property, true);
        }

        static bool drawBez = false;
        static bool isLinking = false;
        static System.Numerics.Vector2 sPos = System.Numerics.Vector2.Zero;
        static System.Numerics.Vector2 ePos = System.Numerics.Vector2.Zero;
        static Node node1, node2;

        public static void HandleLinkCreation(System.Numerics.Vector2 currentPosition, Node node, NodeProperty property, bool input = false)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && ImGui.IsMouseHoveringRect(new System.Numerics.Vector2(currentPosition.X - 5, currentPosition.Y - 5), new System.Numerics.Vector2(currentPosition.X + 5, currentPosition.Y + 5)))
            {
                if (node1 == node && (sPos.X > 0 || sPos.X == currentPosition.X))
                {
                    node1 = null;
                    sPos = System.Numerics.Vector2.Zero;
                    return;
                }
                if (sPos != System.Numerics.Vector2.Zero)
                {
                    NodeEditors[CurrentNodeCollection].links.Add(new NodeLink()
                    {
                        Outposition = sPos,
                        InPosition = new System.Numerics.Vector2(input ? 0 : node.w, node.nodetitleHeight + node.nodetitlePadding + property.propertyPositionY + property.propertyPadding.Y),
                        nodeOut = node1,
                        nodeIn = node
                    }
                    );
                    sPos = System.Numerics.Vector2.Zero;
                    drawBez = false;
                }
                else
                {
                    sPos = new System.Numerics.Vector2(input ? 0 : node.w, node.nodetitleHeight + node.nodetitlePadding + property.propertyPositionY + property.propertyPadding.Y);
                    drawBez = !drawBez;
                    node1 = node;
                }
            }

            if (drawBez)
            {
                DrawBezier();
            }
        }

        public static void DrawCircleOut(NodeProperty property, Node node, System.Numerics.Vector2 windowPos)
        {
            System.Numerics.Vector2 pos = new System.Numerics.Vector2(windowPos.X + node.x + node.w, windowPos.Y + node.y + node.nodetitleHeight + node.nodetitlePadding + property.propertyPositionY + property.propertyPadding.Y);

            ImGui.GetWindowDrawList().AddCircleFilled(pos, 5, UI.ToUint(new Vector4i(255, 255, 255, 255)));

            HandleLinkCreation(pos, node, property);
        }

        public static void DrawBezier()
        {
            NodeCollection nodeE = NodeEditors[CurrentNodeCollection];
            Node node = NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode];
            if (node != node1) return;

            System.Numerics.Vector2 mpos = ImGui.GetMousePos();
            System.Numerics.Vector2 pos = new System.Numerics.Vector2(sPos.X + node.x + nodeE.EditorPosition.X, sPos.Y + node.y + nodeE.EditorPosition.Y);

            float controlDistance = Math.Abs(mpos.X - pos.X) / 2;
            System.Numerics.Vector2 controlP1 = pos + new System.Numerics.Vector2(controlDistance, 0);
            System.Numerics.Vector2 controlP2 = mpos - new System.Numerics.Vector2(controlDistance, 0);

            ImGui.GetWindowDrawList().AddBezierCubic(pos, controlP1, controlP2, mpos, UI.ToUIntA(new Vector4(1, 1, 1, 1)), 3f);

         }

        public static void DrawBezier(System.Numerics.Vector2 pos, System.Numerics.Vector2 mpos)
        {
            float controlDistance = Math.Abs(mpos.X - pos.X)/2;
            System.Numerics.Vector2 controlP1 = pos + new System.Numerics.Vector2(controlDistance, 0);
            System.Numerics.Vector2 controlP2 = mpos - new System.Numerics.Vector2(controlDistance, 0);

            ImGui.GetWindowDrawList().AddBezierCubic(pos, controlP1, controlP2, mpos, UI.ToUIntA(new Vector4(1, 1, 1, 1)), 3f);
        }

        public static void EndNode()
        {
            if (!NodeEditors[CurrentNodeCollection].EditorShown) return;
            Node node = NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode];
            System.Numerics.Vector2 WindowPosition = new System.Numerics.Vector2(NodeEditors[CurrentNodeCollection].EditorPosition.X, NodeEditors[CurrentNodeCollection].EditorPosition.Y);

            ImGui.GetWindowDrawList().AddRectFilled(new System.Numerics.Vector2(WindowPosition.X + node.x, WindowPosition.Y + node.y), new System.Numerics.Vector2(WindowPosition.X + node.x + node.w, WindowPosition.Y + node.y + node.h), UI.ToUIntA(new Vector4(0.1f, 0.1f, 0.1f, 0.8f)), 5);
            ImGui.GetWindowDrawList().AddRectFilled(new System.Numerics.Vector2(WindowPosition.X + node.x, WindowPosition.Y + node.y), new System.Numerics.Vector2(WindowPosition.X + node.x + node.w, WindowPosition.Y + node.y + node.nodetitleHeight), UI.ToUIntA(new Vector4(0.717f, 0.658f, 0.596f, 1)), 5, ImDrawFlags.RoundCornersTopRight | ImDrawFlags.RoundCornersTopLeft);

            ImGui.GetWindowDrawList().AddText(new System.Numerics.Vector2(WindowPosition.X + node.x + 10, WindowPosition.Y + node.y + (node.nodetitleHeight/2) - 7), UI.ToUIntA(new Vector4(0,0,0,1)), FontAwesome.ForkAwesome.Link + " " + node.name);

            for (int i = 0; i < node.properties.Count; i++)
            {
                NodeProperty prop = node.properties[i];

                switch (prop.type)
                {
                    case NodePropertyType.Text:
                        DrawProperty((NodePropertyText)prop, node, WindowPosition);
                        DrawCircle(prop, node, WindowPosition);
                        DrawCircleOut(prop, node, WindowPosition);
                        break;
                    case NodePropertyType.FloatInput:
                        DrawProperty((NodePropertyFloatInput)prop, node, WindowPosition);
                        DrawCircle(prop, node, WindowPosition);
                        DrawCircleOut(prop, node, WindowPosition);
                        break;
                    default:
                        break;
                }
            }

            node.totalPropertyPixelHeight = 0;

            NodeEditors[CurrentNodeCollection].CurrentNode = null;
        }

        public static void DrawNodeLinks(System.Numerics.Vector2 WindowPosition)
        {
            for (int i = 0; i < NodeEditors[CurrentNodeCollection].links.Count; i++)
            {
                NodeLink link = NodeEditors[CurrentNodeCollection].links[i];
                System.Numerics.Vector2 sPos = link.Outposition;
                System.Numerics.Vector2 ePos = link.InPosition;
                DrawBezier(new System.Numerics.Vector2(sPos.X + WindowPosition.X + link.nodeOut.x, sPos.Y + WindowPosition.Y + link.nodeOut.y), new System.Numerics.Vector2(ePos.X + WindowPosition.X + link.nodeIn.x, ePos.Y + WindowPosition.Y + link.nodeIn.y));
            }
        }

        public static void AddProperty(NodeProperty property)
        {
            property.id = NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties.Count;
            NodeEditors[CurrentNodeCollection].nodes[NodeEditors[CurrentNodeCollection].CurrentNode].properties.Add(property);
        }

        public static void DrawGridLines()
        {
            for (int i = 0; i < ImGui.GetWindowSize().X; i += 40)
            {
                ImGui.GetWindowDrawList().AddLine(new System.Numerics.Vector2(i + ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y), new System.Numerics.Vector2(i + ImGui.GetWindowPos().X, ImGui.GetWindowSize().Y + 20 + ImGui.GetWindowPos().Y), UI.ToUIntA(new Vector4(1,1,1,0.2f)));

            }
            for (int i = 0; i < ImGui.GetWindowSize().Y; i += 40)
            {
                ImGui.GetWindowDrawList().AddLine(new System.Numerics.Vector2(ImGui.GetWindowPos().X, i + ImGui.GetWindowPos().Y), new System.Numerics.Vector2(ImGui.GetWindowSize().X + 20 + ImGui.GetWindowPos().X, i + ImGui.GetWindowPos().Y), UI.ToUIntA(new Vector4(1, 1, 1, 0.2f)));
            }
        }

    }
}
