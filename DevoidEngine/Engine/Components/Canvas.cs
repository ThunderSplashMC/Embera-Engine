using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    class Canvas : Component
    {
        struct CanvasItem
        {
            public Vector2 Position;
            public Vector2 Size;
            public Vector2 Rotation;
            public Texture texture;
        }

        public override string Type { get; } = nameof(Canvas);

        public Vector2 ScreenSize = new Vector2();

        List<CanvasItem> QueueList = new List<CanvasItem>();

        public override void OnStart()
        {
            ScreenSize = new Vector2(RenderGraph.ViewportWidth, RenderGraph.ViewportHeight);
        }

        public override void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < QueueList.Count; i++)
            {
                CanvasItem item = QueueList[i];
                Renderer2D.Submit(item.Position, item.Rotation, item.Size, Renderer2D.Quad, item.texture);
            }
            QueueList.Clear();
        }

        public void AddToQueue(Texture texture, Vector2 Position, Vector2 Rotation,Vector2 Size)
        {
            QueueList.Add(new CanvasItem()
            {
                Position = Position,
                Rotation = Rotation,
                Size = Size,
                texture = texture
            });
        }

    }
}
