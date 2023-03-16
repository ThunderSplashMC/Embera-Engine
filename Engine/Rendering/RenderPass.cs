namespace DevoidEngine.Engine.Rendering
{
    class RenderPass
    {
        public virtual void Initialize(int width, int height) { }

        public virtual void DoRenderPass() { }

        public virtual void Resize(int width, int height) { }
    }
}
