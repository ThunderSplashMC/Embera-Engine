using System;
using System.Collections.Generic;

using DevoidEngine.Engine.Core;
using OpenTK.Graphics.OpenGL;
using DevoidEngine.Engine.Rendering;

using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Utilities
{
    class Skybox
    {

        public Cubemap Cubemap;
        private VertexArray SkyboxVAO;
        private Shader SkyboxShader;

        public Skybox()
        {

        }

        public void Init()
        {
            Vertex[] vertices = VERTEX_DEFAULTS.GetCubeVertex();
            VertexBuffer SkyboxVBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, true);
            SkyboxVBO.SetData(vertices, vertices.Length);
            SkyboxVAO = new VertexArray(SkyboxVBO);

            SkyboxShader = new Shader("Engine/EngineContent/shaders/skybox");
        }

        public void SetSkyboxCubemap(Cubemap cubemap)
        {
            this.Cubemap = cubemap;
        }

        public Cubemap GetSkyboxCubemap() 
        {
            return Cubemap;
        }

        public void Render(Camera camera)
        {
            if (Cubemap == null) { return; }

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            SkyboxShader.Use();
            SkyboxShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.CreateTranslation(camera.position));
            SkyboxShader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());
            SkyboxShader.SetMatrix4("W_VIEW_MATRIX", camera.GetViewMatrix());
            SkyboxShader.SetInt("skybox", 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, Cubemap.CubeMapTex);
            SkyboxVAO.Render();
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }
    }
}
