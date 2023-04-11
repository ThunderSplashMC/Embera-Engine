using System;
using DevoidEngine.Engine.Rendering;

namespace Elemental.Editor.EditorUtils
{
    class EditorGizmoPass : RenderPass
    {

        public FrameBuffer frameBuffer;

        public override void Initialize(int width, int height)
        {
            frameBuffer = new FrameBuffer(new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                    {
                     new ColorAttachment() {textureFormat = FrameBufferTextureFormat.R32I, textureType = FrameBufferTextureType.Texture2D}
                    },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            });


        }

        public override void DoRenderPass()
        {



        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
        }

    }
}
