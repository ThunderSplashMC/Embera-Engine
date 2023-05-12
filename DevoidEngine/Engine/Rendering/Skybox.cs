using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    class Skybox
    {
        Cubemap IrradianceTexture;
        FrameBuffer IrradianceCapture;
        Shader IrradianceShader;

        Matrix4 CaptureProjection;
        Matrix4[] CaptureViews;

        public void Initialize(int width, int height)
        {
            CaptureProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1, 0.1f, 10.0f);

            CaptureViews = new Matrix4[]
            {
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitX, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitX, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ),
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, -Vector3.UnitY)
            };


            IrradianceCapture = new FrameBuffer(new FrameBufferSpecification()
            {
                width = 32,
                height = 32,
                ColorAttachments = new ColorAttachment[]
                {
                    new ColorAttachment()
                    {
                        textureFormat = FrameBufferTextureFormat.RGBA16F,
                        textureType = FrameBufferTextureType.Texture2D
                    }
                }
            });
        }

        public void GenerateIrradianceMap()
        {
            IrradianceTexture = new Cubemap();
            IrradianceTexture.Create(32, 32, Core.FilterTypes.Linear, Core.FilterTypes.Linear);

            IrradianceCapture.Bind();

            IrradianceShader.Use();

            IrradianceShader.SetMatrix4("W_PROJECTION_MATRIX", CaptureProjection);

            for (int i = 0; i < 6; i++)
            {
                IrradianceShader.SetMatrix4("W_VIEW_MATRIX", CaptureViews[i]);

                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, IrradianceTexture.GetRendererID(), 0);
                RendererUtils.CubeVAO.Render();
            }

            IrradianceCapture.UnBind();

        }

    }
}
