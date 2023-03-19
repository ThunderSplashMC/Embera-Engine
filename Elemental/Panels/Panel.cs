using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Elemental.Panels
{
    abstract class Panel
    {
        public EditorLayer Editor;

        public virtual void OnInit() { }

        public virtual void OnGUIRender() { }

        public virtual void OnUpdate(float deltaTime) { }

        public virtual void OnRender() { }

        public virtual void OnResize(int width, int height) { }
        public virtual void OnKeyDown(KeyboardKeyEventArgs key) { }

    }
}
