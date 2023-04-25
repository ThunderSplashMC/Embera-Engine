using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Editor.EditorUtils
{
    internal class GizmoPass : RenderPass
    {
        public override void LightPassEarly()
        {
            //Guizmo3D.DrawGrid();
        }

    }
}
