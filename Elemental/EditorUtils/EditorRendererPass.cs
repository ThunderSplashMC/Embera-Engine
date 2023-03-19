using System;
using System.Collections.Generic;
using System.Linq;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace DevoidEngine.Elemental.EditorUtils
{
    class EditorRendererPass : RenderPass
    {
        public FrameBuffer frameBuffer;

        Shader viewportSelectShader = new Shader("Elemental/Assets/Shaders/viewportID");

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
            frameBuffer.Resize(width, height);
        }

        public override void DoRenderPass()
        {
            frameBuffer.Bind();

            //RenderGraph.CompositeBuffer.Bind();

            RendererUtils.Clear();

            RendererUtils.Cull(true);
            RendererUtils.DepthTest(true);

            List<DrawItem> drawlist = Renderer3D.GetRenderDrawList();

            for (int i = 0; i < drawlist.Count; i++)
            {
                if (drawlist[i].associateObject == null) { return; }
                viewportSelectShader.Use();
                Renderer3D.UploadCameraData(viewportSelectShader);
                Renderer3D.UploadModelData(viewportSelectShader, drawlist[i].position, drawlist[i].rotation, drawlist[i].scale);

                viewportSelectShader.SetInt("UUID", (int)drawlist[i].associateObject /*((GameObject)drawlist[i].associateObject).ID*/);

                //Console.WriteLine((float)((GameObject)drawlist[i].associateObject).ID);

                //RendererUtils.QuadVAO.Render();

                drawlist[i].mesh.Draw();
            }

            //RenderGraph.CompositeBuffer.UnBind();

            frameBuffer.UnBind();

            RendererUtils.Cull(false);
            RendererUtils.DepthTest(false);

            RendererUtils.BlitFBToScreen(frameBuffer, RenderGraph.CompositeBuffer);

            base.DoRenderPass();
        }

        public override void Resize(int width, int height)
        {
            frameBuffer.Resize(width, height);
        }

    }
}
