﻿using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using ImGuiNET;

namespace Elemental.Editor.EditorUtils
{
    class EditorOutlinePass : RenderPass
    {
        public FrameBuffer frameBuffer;
        public FrameBuffer frameBuffer2;

        Shader outlineShader = new Shader("Editor/Assets/Shaders/outline/outline");
        Shader edgedetection = new Shader("Editor/Assets/Shaders/outline/edgedetection");

        public float outlineWidth = 3f;

        public int CurrentOutlinedObjectUUID = 0;

        public override void Initialize(int width, int height)
        {
            frameBuffer = new FrameBuffer(new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
                {
                     new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA32F, textureType = FrameBufferTextureType.Texture2D}
                },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height
                }
            });
            frameBuffer.Resize(width, height);

            frameBuffer2 = new FrameBuffer(new FrameBufferSpecification()
            {
                width = width,
                height = height,
                ColorAttachments = new ColorAttachment[]
    {
                     new ColorAttachment() {textureFormat = FrameBufferTextureFormat.RGBA32F, textureType = FrameBufferTextureType.Texture2D}
    },
                DepthAttachment = new DepthAttachment()
                {
                    width = width,
                    height = height,
                    stencilOnly = true
                }
            });
            frameBuffer2.Resize(width, height);


        }

        public override void DoRenderPass()
        {
            if (CurrentOutlinedObjectUUID == 0)
            {
                return;
            }

            frameBuffer.Bind();

            GL.ColorMask(false, false, false, false);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            List<DrawItem> drawlist = RenderGraph.MeshSystem.GetRenderDrawList();

            for (int i = 0; i < drawlist.Count; i++)
            {
                if ((int)drawlist[i].associateObject != CurrentOutlinedObjectUUID) continue;

                GL.StencilFunc(StencilFunction.Always, 1, 0x00);
                GL.StencilMask(0xFF);

                RendererUtils.BasicShader.Use();
                Renderer3D.UploadCameraData(RendererUtils.BasicShader);
                Renderer3D.UploadModelData(RendererUtils.BasicShader, drawlist[i].position, drawlist[i].rotation, drawlist[i].scale);

                drawlist[i].mesh.Draw();
            }

            GL.ColorMask(true, true, true, true);

            GL.Disable(EnableCap.StencilTest);
            frameBuffer.UnBind();

            frameBuffer2.Bind();

            GL.ClearColor(RenderGraph.Camera.ClearColor.X, RenderGraph.Camera.ClearColor.Y, RenderGraph.Camera.ClearColor.Z, 0);

            RendererUtils.Clear();

            RendererUtils.Cull(false);
            RendererUtils.DepthTest(false);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            for (int i = 0; i < drawlist.Count; i++)
            {
                if ((int)drawlist[i].associateObject != CurrentOutlinedObjectUUID) continue;


                edgedetection.Use();

                edgedetection.SetFloat("radius", outlineWidth);

                edgedetection.SetInt("frameBufferTexture", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, RenderGraph.CompositeBuffer.GetColorAttachment(0));


                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, frameBuffer.GetDepthAttachment());
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.DepthStencilTextureMode, (int)All.StencilIndex);
                edgedetection.SetInt("stencilTexture", 1);

                RendererUtils.QuadVAO.Render();
            }

            RendererUtils.Cull(true);
            RendererUtils.DepthTest(true);

            frameBuffer2.UnBind();

            RendererUtils.BlitFBToScreen(frameBuffer2, RenderGraph.CompositeBuffer, 0.5f, true);

        }

        public override void Resize(int width, int height)
        {
            frameBuffer.Resize(width, height);
            frameBuffer2.ResizeDepthStencil(width, height);
            frameBuffer2.Resize(width, height);
            base.Resize(width, height);
        }
    }
}
