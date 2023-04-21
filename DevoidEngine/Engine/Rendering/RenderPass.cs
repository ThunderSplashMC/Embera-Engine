namespace DevoidEngine.Engine.Rendering
{
    public class RenderPass
    {
        public virtual void Initialize(int width, int height) { }

        public virtual void LightPassEarly() { }
        public virtual void DoRenderPass() { }

        public virtual void Resize(int width, int height) { }
    }
}
