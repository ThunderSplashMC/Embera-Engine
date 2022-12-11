namespace DevoidEngine.Engine.Core
{
    class Layer
    {
        public Application Application;

        public virtual void OnAttach() { }
        public virtual void OnDetach() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnResize(int width, int height) { }
        public virtual void OnRender() { }
        public virtual void OnLateRender() { }
        public virtual void GUIRender() { }
        public virtual void KeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs keyboardevent) { }

    }
}
