using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Engine.Imgui.imnodes
{
    enum ImNodesCol_
    {
        ImNodesCol_NodeBackground = 0,
        ImNodesCol_NodeBackgroundHovered,
        ImNodesCol_NodeBackgroundSelected,
        ImNodesCol_NodeOutline,
        ImNodesCol_TitleBar,
        ImNodesCol_TitleBarHovered,
        ImNodesCol_TitleBarSelected,
        ImNodesCol_Link,
        ImNodesCol_LinkHovered,
        ImNodesCol_LinkSelected,
        ImNodesCol_Pin,
        ImNodesCol_PinHovered,
        ImNodesCol_BoxSelector,
        ImNodesCol_BoxSelectorOutline,
        ImNodesCol_GridBackground,
        ImNodesCol_GridLine,
        ImNodesCol_GridLinePrimary,
        ImNodesCol_MiniMapBackground,
        ImNodesCol_MiniMapBackgroundHovered,
        ImNodesCol_MiniMapOutline,
        ImNodesCol_MiniMapOutlineHovered,
        ImNodesCol_MiniMapNodeBackground,
        ImNodesCol_MiniMapNodeBackgroundHovered,
        ImNodesCol_MiniMapNodeBackgroundSelected,
        ImNodesCol_MiniMapNodeOutline,
        ImNodesCol_MiniMapLink,
        ImNodesCol_MiniMapLinkSelected,
        ImNodesCol_MiniMapCanvas,
        ImNodesCol_MiniMapCanvasOutline,
        ImNodesCol_COUNT
    };

    enum ImNodesStyleVar_
    {
        ImNodesStyleVar_GridSpacing = 0,
        ImNodesStyleVar_NodeCornerRounding,
        ImNodesStyleVar_NodePadding,
        ImNodesStyleVar_NodeBorderThickness,
        ImNodesStyleVar_LinkThickness,
        ImNodesStyleVar_LinkLineSegmentsPerLength,
        ImNodesStyleVar_LinkHoverDistance,
        ImNodesStyleVar_PinCircleRadius,
        ImNodesStyleVar_PinQuadSideLength,
        ImNodesStyleVar_PinTriangleSideLength,
        ImNodesStyleVar_PinLineThickness,
        ImNodesStyleVar_PinHoverRadius,
        ImNodesStyleVar_PinOffset,
        ImNodesStyleVar_MiniMapPadding,
        ImNodesStyleVar_MiniMapOffset,
        ImNodesStyleVar_COUNT
    };

    enum ImNodesStyleFlags_
    {
        ImNodesStyleFlags_None = 0,
        ImNodesStyleFlags_NodeOutline = 1 << 0,
        ImNodesStyleFlags_GridLines = 1 << 2,
        ImNodesStyleFlags_GridLinesPrimary = 1 << 3,
        ImNodesStyleFlags_GridSnapping = 1 << 4
    };

    enum ImNodesPinShape_
    {
        ImNodesPinShape_Circle,
        ImNodesPinShape_CircleFilled,
        ImNodesPinShape_Triangle,
        ImNodesPinShape_TriangleFilled,
        ImNodesPinShape_Quad,
        ImNodesPinShape_QuadFilled
    };

    // This enum controls the way the attribute pins behave.
    enum ImNodesAttributeFlags_
    {
        ImNodesAttributeFlags_None = 0,
        // Allow detaching a link by left-clicking and dragging the link at a pin it is connected to.
        // NOTE: the user has to actually delete the link for this to work. A deleted link can be
        // detected by calling IsLinkDestroyed() after EndNodeEditor().
        ImNodesAttributeFlags_EnableLinkDetachWithDragClick = 1 << 0,
        // Visual snapping of an in progress link will trigger IsLink Created/Destroyed events. Allows
        // for previewing the creation of a link while dragging it across attributes. See here for demo:
        // https://github.com/Nelarius/imnodes/issues/41#issuecomment-647132113 NOTE: the user has to
        // actually delete the link for this to work. A deleted link can be detected by calling
        // IsLinkDestroyed() after EndNodeEditor().
        ImNodesAttributeFlags_EnableLinkCreationOnSnap = 1 << 1
    };

    struct ImRect
    {
        public Vector2 Min, Max;

        public ImRect(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public void Add(Vector2 p)
        {
            if (Min.X > p.X) {
                Min.X = p.X;
            }
            if (Min.Y > p.Y) {
                Min.Y = p.Y;
            }
            if (Max.X < p.X) {
                Max.X = p.X;
            }
            if (Max.Y < p.Y) {
                Max.Y = p.Y;
            }
        }
        public void Add(ImRect r) 
        { 
            if (Min.X > r.Min.X) {
                Min.X = r.Min.X;
            }
            if (Min.Y > r.Min.Y) 
            {
                Min.Y = r.Min.Y;
            }
            if (Max.X < r.Max.X) 
            {
                Max.X = r.Max.X;
            }
            if (Max.Y < r.Max.Y) 
            {
                Max.Y = r.Max.Y;
            }
        }

        public void Expand(float amount)          { Min.X -= amount;   Min.Y -= amount;   Max.X += amount;   Max.Y += amount; }
        public void Expand(Vector2 amount) { Min.X -= amount.X; Min.Y -= amount.Y; Max.X += amount.X; Max.Y += amount.Y; }

}

    struct ImObjectPool<T>
    {
        public List<T> Pool;
        public List<bool> InUse;
        public List<int> FreeList;
        //public ImGuiStorage IdMap;

        public void Create()
        {
            Pool = new List<T>();
            InUse = new List<bool>();
            FreeList = new List<int>();
        }

    };

    struct ImNodeData
    {
        int Id;
        Vector2 Origin; // The node origin is in editor space
        ImRect TitleBarContentRect;
        ImRect Rect;

        struct ColorStyle_
        {
            UInt32 Background, BackgroundHovered, BackgroundSelected, Outline, Titlebar, TitlebarHovered, TitlebarSelected;
        };

        struct LayoutStyle_
        {
            float CornerRounding;
            Vector2 Padding;
            float BorderThickness;
        };

        ColorStyle_ ColorStyle;
        LayoutStyle_ LayoutStyle;

        List<int> PinIndices;
        bool Draggable;

        public ImNodeData(int node_id)
        {
            Id = node_id;
            Origin = new Vector2(0.0f, 0.0f);
            TitleBarContentRect = new ImRect();
            Draggable = true;
            Rect = new ImRect(Vector2.Zero, Vector2.Zero);
            ColorStyle = new ColorStyle_();
            LayoutStyle = new LayoutStyle_();
            PinIndices = new List<int>();
        }
    };

    struct ImPinData
    {
        int Id;
        int ParentNodeIdx;
        ImRect AttributeRect;
        int Type;
        int Shape;
        Vector2 Pos; // screen-space coordinates
        int Flags;

        struct ColorStyle_
        {
            UInt32 Background, Hovered;
        };
        ColorStyle_ ColorStyle;

        public ImPinData(int pin_id)
        {
            Id = pin_id;
            ParentNodeIdx = 0;
            AttributeRect = new ImRect();
            Shape = 0;
            Pos = Vector2.Zero;
            Flags = 0;
            ColorStyle = new ColorStyle_();
            Type = 0;
        }
    }

    struct ImLinkData
    {
        int Id;
        int StartPinIdx, EndPinIdx;

        struct ColorStyle_ { UInt32 Base, Hovered, Selected; }
        ColorStyle_ ColorStyle;

        public ImLinkData(int link_id)
        {
            Id = link_id;
            StartPinIdx = 0;
            EndPinIdx = 0;
            ColorStyle = new ColorStyle_();
        }
    }

struct GImNodes
    {
        public ImNodesStyle Style;
    }

    struct ImNodesStyle
    {
        public float GridSpacing;

        public float NodeCornerRounding;
        public Vector2 NodePadding;
        public float NodeBorderThickness;

        public float LinkThickness;
        public float LinkLineSegmentsPerLength;
        public float LinkHoverDistance;

        // The following variables control the look and behavior of the pins. The default size of each
        // pin shape is balanced to occupy approximately the same surface area on the screen.

        // The circle radius used when the pin shape is either ImNodesPinShape_Circle or
        // ImNodesPinShape_CircleFilled.
        public float PinCircleRadius;
        // The quad side length used when the shape is either ImNodesPinShape_Quad or
        // ImNodesPinShape_QuadFilled.
        public float PinQuadSideLength;
        // The equilateral triangle side length used when the pin shape is either
        // ImNodesPinShape_Triangle or ImNodesPinShape_TriangleFilled.
        public float PinTriangleSideLength;
        // The thickness of the line used when the pin shape is not filled.
        public float PinLineThickness;
        // The radius from the pin's center position inside of which it is detected as being hovered
        // over.
        public float PinHoverRadius;
        // Offsets the pins' positions from the edge of the node to the outside of the node.
        public float PinOffset;

        // Mini-map padding size between mini-map edge and mini-map content.
        public Vector2 MiniMapPadding;
        // Mini-map offset from the screen side.
        public Vector2 MiniMapOffset;

        // By default, ImNodesStyleFlags_NodeOutline and ImNodesStyleFlags_Gridlines are enabled.
        public int Flags;

        // Set these mid-frame using Push/PopColorStyle. You can index this color array with with a
        // ImNodesCol value.
        public uint[] Colors;
    }


    enum ImNodesMiniMapLocation_
    {
        ImNodesMiniMapLocation_BottomLeft,
        ImNodesMiniMapLocation_BottomRight,
        ImNodesMiniMapLocation_TopLeft,
        ImNodesMiniMapLocation_TopRight,
    };

    struct ImNodesEditorContext
    {
        ImObjectPool<ImNodeData> Nodes;
        ImObjectPool<ImPinData> Pins;
        ImObjectPool<ImLinkData> Links;

        List<int> NodeDepthOrder;

        // ui related fields
        Vector2 Panning;
        Vector2 AutoPanningDelta;
        // Minimum and maximum extents of all content in grid space. Valid after final
        // ImNodes::EndNode() call.
        ImRect GridContentBounds;

        List<int> SelectedNodeIndices;
        List<int> SelectedLinkIndices;

        // Relative origins of selected nodes for snapping of dragged nodes
        List<Vector2> SelectedNodeOffsets;
        // Offset of the primary node origin relative to the mouse cursor.
        Vector2 PrimaryNodeOffset;

        ImClickInteractionState ClickInteraction;

        // Mini-map state set by MiniMap()

        bool MiniMapEnabled;
        //ImNodesMiniMapLocation MiniMapLocation;
        float MiniMapSizeFraction;
        //ImNodesMiniMapNodeHoveringCallback MiniMapNodeHoveringCallback;
        //ImNodesMiniMapNodeHoveringCallbackUserData MiniMapNodeHoveringCallbackUserData;

        // Mini-map state set during EndNodeEditor() call

        ImRect MiniMapRectScreenSpace;
        ImRect MiniMapContentScreenSpace;
        float MiniMapScaling;

        public ImNodesEditorContext(int x = 0)
        {
            Nodes = new ImObjectPool<ImNodeData>();
            Pins = new ImObjectPool<ImPinData>();
            Links = new ImObjectPool<ImLinkData>();
            Panning = new Vector2(0f, 0f);
            SelectedNodeIndices = new List<int>();
            SelectedLinkIndices = new List<int>();
            SelectedNodeOffsets = new List<Vector2>();
            PrimaryNodeOffset = new Vector2(0f, 0f);
            ClickInteraction = new ImClickInteractionState();
            MiniMapEnabled = true;
            MiniMapSizeFraction = 0f;
            MiniMapScaling = 0f;
            MiniMapRectScreenSpace = new ImRect();
            MiniMapContentScreenSpace = new ImRect();
            NodeDepthOrder = new List<int>();
            AutoPanningDelta = new Vector2();
            GridContentBounds = new ImRect();

        }
    };

    struct ImNodesContext
    {
        //ImNodesEditorContext* DefaultEditorCtx;
        //ImNodesEditorContext* EditorCtx;

        //// Canvas draw list and helper state
        //ImDrawList* CanvasDrawList;
        //ImGuiStorage NodeIdxToSubmissionIdx;
        //ImVector<int> NodeIdxSubmissionOrder;
        //ImVector<int> NodeIndicesOverlappingWithMouse;
        //ImVector<int> OccludedPinIndices;

        //// Canvas extents
        //ImVec2 CanvasOriginScreenSpace;
        //ImRect CanvasRectScreenSpace;

        //// Debug helpers
        //ImNodesScope CurrentScope;

        //// Configuration state
        //ImNodesIO Io;
        //ImNodesStyle Style;
        //ImVector<ImNodesColElement> ColorModifierStack;
        //ImVector<ImNodesStyleVarElement> StyleModifierStack;
        //ImGuiTextBuffer TextBuffer;

        //int CurrentAttributeFlags;
        //ImVector<int> AttributeFlagStack;

        //// UI element state
        //int CurrentNodeIdx;
        //int CurrentPinIdx;
        //int CurrentAttributeId;

        //ImOptionalIndex HoveredNodeIdx;
        //ImOptionalIndex HoveredLinkIdx;
        //ImOptionalIndex HoveredPinIdx;

        //ImOptionalIndex DeletedLinkIdx;
        //ImOptionalIndex SnapLinkIdx;

        //// Event helper state
        //// TODO: this should be a part of a state machine, and not a member of the global struct.
        //// Unclear what parts of the code this relates to.
        //int ImNodesUIState;

        //int ActiveAttributeId;
        //bool ActiveAttribute;

        //// ImGui::IO cache

        //ImVec2 MousePos;

        //bool LeftMouseClicked;
        //bool LeftMouseReleased;
        //bool AltMouseClicked;
        //bool LeftMouseDragging;
        //bool AltMouseDragging;
        //float AltMouseScrollDelta;
        //bool MultipleSelectModifier;
    };

    struct ImOptionalIndex
    {
        int _Index;

        public ImOptionalIndex(int value)
        {
            _Index = value;
        }

        // Observers

        bool HasValue() { return _Index != 0; }

    int Value()
    {
        return _Index;
    }

        // Modifiers

        static ImOptionalIndex operator= (int value)
    {
        _Index = value;
        return this;
    }

    //void Reset() { _Index = INVALID_INDEX; }

    //static bool operator== (ref ImOptionalIndex rhs) { return _Index == rhs._Index; }

    //static bool operator== (int rhs) { return _Index == rhs; }

    //static bool operator != (ref ImOptionalIndex rhs) { return _Index != rhs._Index; }

    //static bool operator != (int rhs) { return _Index != rhs; }

    //int INVALID_INDEX = -1;
};

struct ImClickInteractionState
    {
        int Type;

        struct LinkCreation_
        {
            int StartPinIdx;
            int EndPinIdx;
            int Type;
        }   
        LinkCreation_ LinkCreation;

        struct BoxSelector_
        {
            ImRect Rect; // Coordinates in grid space
        };

        BoxSelector_ BoxSelector;

        public ImClickInteractionState(int t = 0) {

            Type = t;
            LinkCreation = new LinkCreation_();
            BoxSelector = new BoxSelector_();
        }
    };
}
