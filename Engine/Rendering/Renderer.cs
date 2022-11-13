using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    class Renderer
    {

        public static void Init(int width, int height)
        {
            Renderer2D.Init(width, height);
            Renderer3D.Init(width, height);
        }

        public static void Render()
        {
            Renderer3D.Render();
            Renderer2D.Render();

        }

        public static void Resize(int width, int height)
        {
            RenderGraph.ViewportHeight = height;
            RenderGraph.ViewportWidth = width;
            Renderer3D.Resize(width, height);
            Renderer2D.Resize(width, height);
            RenderGraph.CompositePass.Resize(width, height);
        }
    }
}
