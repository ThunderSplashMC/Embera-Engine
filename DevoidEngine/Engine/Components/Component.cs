using System;
using DevoidEngine.Engine.Rendering;

using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Components
{
    public abstract class Component
    {
        public abstract string Type { get; }
        public Component()
        {

        }
        
        public GameObject gameObject;
        public virtual void OnAwake() { }
        public virtual void OnStart() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnRender() { }
        public virtual void OnGUIRender() { }

    }
}

