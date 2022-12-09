using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Utilities
{
    class VertexBuffer : IDisposable
    {
        public static readonly int MaxVertices = 1000000;
        public static readonly int MinVertices = 1;

        public readonly int VertexBufferObject;
        public readonly VertexInfo VertexInfo;
        public readonly int VertexCount;
        public readonly bool IsStatic;


        private bool isdisposed = false;

        public VertexBuffer(VertexInfo vertexInfo, int vertexCount, bool isStatic = true)
        {
            this.isdisposed = false;
            //if (vertexCount > MaxVertices || vertexCount < MinVertices)
            //{
            //    throw new ArgumentException("Vertex Count exceeds " + MaxVertices + " or is lower than " + MinVertices);
            //}

            this.VertexInfo = vertexInfo;
            this.VertexCount = vertexCount;
            this.IsStatic = isStatic;
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            if (!isStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexInfo.SizeInBytes * vertexCount, IntPtr.Zero, hint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public VertexBuffer(VertexInfo vertexInfo, Vertex[] data, bool isStatic = true)
        {
            this.isdisposed = false;
            //if (vertexCount > MaxVertices || vertexCount < MinVertices)
            //{
            //    throw new ArgumentException("Vertex Count exceeds " + MaxVertices + " or is lower than " + MinVertices);
            //}

            this.VertexInfo = vertexInfo;
            this.VertexCount = data.Length;
            this.IsStatic = isStatic;
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            if (!isStatic)
            {
                hint = BufferUsageHint.StreamDraw;
            }

            VertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, VertexInfo.SizeInBytes * data.Length, IntPtr.Zero, hint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            SetData(data, data.Length);
        }

        public void SetData<T>(T[] data, int count) where T : struct
        {
            if (typeof(T) != VertexInfo.Type)
            {
                throw new ArgumentException();
            }

            if (data is null)
            {
                throw new ArgumentException();
            }

            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }

            if (count <= 0 || count > this.VertexCount || count > data.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, count * VertexInfo.SizeInBytes, data);
        }

        ~VertexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isdisposed == true)
            {
                return;
            }

            this.isdisposed = true;
            try
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DeleteBuffer(VertexBufferObject);
                GC.SuppressFinalize(this);
            } catch { }
        }
    }
}
