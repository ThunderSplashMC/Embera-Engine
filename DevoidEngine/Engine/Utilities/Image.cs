using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Utilities
{
    class Image
    {
        public int Width, Height;
        public byte[] Pixels;

        public Image(string path)
        {
            LoadImage(path);
        }

        public Image()
        {

        }

        public void LoadImage(string path)
        {
            SixLabors.ImageSharp.Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(path);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(3 * image.Width * image.Height);
            Pixels = new byte[3 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }

        public void LoadImage(byte[] pixelsi)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(pixelsi);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[3 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);

                    }
                }
            });

            Pixels = pixels.ToArray();
        }

        public void LoadImageAlpha(string path, bool directPath = false)
        {
            SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
            this.Width = image.Width;
            this.Height = image.Height;
            List<byte> pixels = new List<byte>(4 * image.Width * image.Height);
            Pixels = new byte[4 * image.Width * image.Height];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);

                    for (int x = 0; x < image.Width; x++)
                    {
                        pixels.Add(row[x].R);
                        pixels.Add(row[x].G);
                        pixels.Add(row[x].B);
                        pixels.Add(row[x].A);
                    }
                }
            });

            Pixels = pixels.ToArray();
        }

        public void SaveImage(string path)
        {

        }
    }
}
