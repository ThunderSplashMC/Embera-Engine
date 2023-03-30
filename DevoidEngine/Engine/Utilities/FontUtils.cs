using DevoidEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Utilities
{
    class FontSettings
    {
        public int FontSize = 16;

        public string FromFile = "D:/Programming/Devoid/DevoidEngine/Elemental/Assets/Fonts/OpenSans.ttf";
        public string FontName = "Open Sans";

        public string FontBitmapFilename = "test.png";
    }

    class Glyph
    {
        public int X, Y;
        public int W, H;
        public char character;
    }

    class DevoidFont
    {
        public List<Glyph> glyphs;
        public Texture LoadedTexture;

        public DevoidFont ()
        {
            glyphs = new List<Glyph> ();
        }
    }

    class FontUtils
    {

        public static DevoidFont GenerateBitmapFromFile(string filename, int fontSize, string fontName)
        {

            DevoidFont dFont = new DevoidFont();

            FontSettings Settings = new FontSettings();

            PrivateFontCollection collection = new PrivateFontCollection();

            collection.AddFontFile(filename);
            FontFamily fontFamily = new FontFamily(fontName, collection);


            Font font = new Font(fontFamily, fontSize);

            FontStyle fontStyle = font.Style;

            Bitmap bitmap = new Bitmap(2048, 2048);

            Graphics g = Graphics.FromImage(bitmap);

            Graphics g1 = Graphics.FromImage(new Bitmap(1, 1));

            //SizeF fontGlyphSize =;//;new SizeF(new System.Numerics.Vector2(100, 100));

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float totalWidth = 0;
            float totalHeight = 0;

            float prevH = 0;

            for (int p = 0; p < 16; p++)
            {
                for (int n = 0; n < 16; n++)
                {
                    char c = (char)(n + p * 16);

                    SizeF fontGlyphSize = g1.MeasureString(c.ToString(), font);

                    prevH = fontGlyphSize.Height;

                    g.DrawString(c.ToString(), font, Brushes.White, totalWidth, totalHeight);

                    dFont.glyphs.Add(new Glyph()
                    {
                        X = (int)(totalWidth),
                        Y = (int)(totalHeight),
                        W = (int)fontGlyphSize.Width,
                        H = (int)fontGlyphSize.Height,
                        character = c
                    }) ;
                    totalWidth += fontGlyphSize.Width;
                }
                totalHeight += prevH;
                totalWidth = 0;
            }

            bitmap.Save($"fontTex{fontName}.png");

            dFont.LoadedTexture = new Texture($"fontTex{fontName}.png");

            return dFont;
        }


    }
}
