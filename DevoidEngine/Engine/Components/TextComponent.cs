using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Diagnostics;
using DevoidEngine.Engine.Utilities;
using OpenTK.Mathematics;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    
    public class TextComponent : Component
    {
        public override string Type { get; } = nameof(TextComponent);

        public string Content = "Text!";
        public Color4 OverlayColor = new Color4(0,0,0,0);
        public float CharSpacingOffset = 0.0f;

        private string prevContent;
        private float charSpacing;

        private Mesh mesh;
        private List<Vertex> vertices = new List<Vertex>();
        private DevoidFont fontLoaded;
        private Material fontMaterial;

        public enum TextFilterType
        {
            Linear,
            Closest
        }

        public enum TextRenderType
        {
            _3D,
            _2D
        }

        public TextRenderType renderType = TextRenderType._2D;
        public TextFilterType textFilterType = TextFilterType.Linear;

        private TextFilterType prevType;

        public override void OnStart()
        {
            fontLoaded = FontUtils.GenerateBitmapFromFile("Editor/Assets/Fonts/OpenSans.ttf", 16, "Open Sans");
            fontMaterial = new Material(new Shader("Engine/EngineContent/shaders/font-shader"));
            fontMaterial.SetTexture("u_Texture", fontLoaded.LoadedTexture);
        }

        public override void OnUpdate(float deltaTime)
        {

            if (fontLoaded == null)
            {
                OnStart();
            }
            if (prevType != textFilterType)
            {
                fontLoaded.LoadedTexture.ChangeFilterType(textFilterType == TextFilterType.Linear ? FilterTypes.Linear : FilterTypes.Nearest);

                prevType = textFilterType;
            }


            if (Content != prevContent && Content != "")
            {
                prevContent = Content;
                ReconstructMesh();
            }
            if (charSpacing != CharSpacingOffset)
            {
                charSpacing = CharSpacingOffset;
                ReconstructMesh();
            }
            mesh.Material.Set("v_Color", new Vector4(OverlayColor.R, OverlayColor.G, OverlayColor.B, 0.5f));
            if (Content != "")
            {
                if (renderType == TextRenderType._2D)
                {
                    mesh.Material.Set("Do_3D", 0);
                    Renderer2D.Submit(gameObject.transform.position.Xy, gameObject.transform.rotation.Xy, gameObject.transform.scale.Xy, mesh, null, mesh.Material);
                } else
                {
                    mesh.Material.Set("Do_3D", 1);
                    Renderer3D.Submit(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.scale, mesh);
                }
            }
        }

        void ReconstructMesh()
        {
            float totalLength = 0;
            float totalHeight = 0;
            mesh?.Dispose();
            vertices.Clear();

            for (int y = 0; y < Content.Length; y++)
            {

                for (int i = 0; i < fontLoaded.glyphs.Count; i++)
                {
                    char character = Content[y];

                    Glyph glyph = fontLoaded.glyphs[i];

                    if (character == glyph.character)
                    {
                        float u = (float)glyph.X / (float)fontLoaded.LoadedTexture.GetSize().X;
                        float v = (float)glyph.Y / (float)fontLoaded.LoadedTexture.GetSize().Y;

                        float u_step = (float)glyph.W / (float)fontLoaded.LoadedTexture.GetSize().X;
                        float v_step = (float)glyph.H / (float)fontLoaded.LoadedTexture.GetSize().Y;

                        float glyphWidth = (float)glyph.W * 10;
                        float glyphHeight = (float)glyph.H * 10;

                        if (Content[y] == "\n".ToCharArray()[0])
                        {
                            totalHeight += glyphHeight;
                            totalLength = 0;
                            continue;
                        }


                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u + u_step, v))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, v))
                         );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight + glyphHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, v + v_step))
                        );

                        //

                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight + glyphHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u + u_step, v + v_step))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u + u_step, v))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight + glyphHeight, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, v + v_step))
                        );

                        totalLength += glyphWidth + charSpacing;

                    }
                }


            }

            mesh = new Mesh();

            mesh.SetVertices(vertices.ToArray());

            mesh.SetMaterial(fontMaterial);

        }

    }
}
