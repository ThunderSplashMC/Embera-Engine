using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public class PathTracedRenderer
    {
        static List<DrawItem> DrawList = new List<DrawItem>();

        static VertexArray BakedVAO;


        public static void Render(List<DrawItem> drawList)
        {
            Begin(drawList);
            End();
        }

        public static void Begin(List<DrawItem> drawList)
        {
            if (DrawList.Count != drawList.Count)
            {
                DrawList = drawList;
                BakeMeshes();
            }
        }

        public static void BakeMeshes()
        {
            List<Vertex> TotalVertices = new List<Vertex>();

            for (int i = 0; i < DrawList.Count; i++) 
            {

                Vertex[] vertices = DrawList[i].mesh.GetVertices();

                for (int x = 0; x < vertices.Length; x++) 
                {
                    //Vertex vertex = new Vertex((new Vector4(vertices[x].Position, 1.0f) * Matrix4.CreateTranslation(DrawList[i].position)).Xyz, vertices[x].Normal, vertices[x].TexCoord);

                    TotalVertices.Add(vertices[x]);
                }
            }

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, TotalVertices.Count);
            vertexBuffer.SetData(TotalVertices.ToArray(), TotalVertices.Count);

            BakedVAO = new VertexArray(vertexBuffer);
        }

        public static void End()
        {
            RenderGraph.CompositeBuffer.Bind();

            RendererUtils.Clear();

            RendererUtils.BasicShader.Use();

            Renderer3D.UploadModelData(RendererUtils.BasicShader, Vector3.Zero, Vector3.Zero, Vector3.One);
            Renderer3D.UploadCameraData(RendererUtils.BasicShader);

            BakedVAO.Render();


            RenderGraph.CompositeBuffer.UnBind();
        }

    }
}
