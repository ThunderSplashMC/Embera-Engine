using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Core
{
    class ComputeShader : IDisposable
    {
        

        public Dictionary<string, int> UniformPositions = new Dictionary<string, int>();

        int Handle;

        int OutputTexture;

        Vector3i WorkGroupSize;

        string Cpath;

        private bool disposedValue = false;

        public ComputeShader(string path, Vector3i WorkGroupSize)
        {
            Cpath = path;

            int computeShader;

            string ComputeShaderSource;

            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                ComputeShaderSource = reader.ReadToEnd();
            }

            computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, ComputeShaderSource);

            CompileSource(computeShader);

            this.WorkGroupSize = WorkGroupSize;
        }

        public void CreateOutputTexture(TextureFormat textureFormat)
        {
            OutputTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, OutputTexture);
            switch(textureFormat)
            {
                case TextureFormat.R32F:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, WorkGroupSize.X, WorkGroupSize.Y, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
                    SetupTextureParams();
                    GL.BindImageTexture(0, OutputTexture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32f);
                    break;
                case TextureFormat.RGBA32F:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, WorkGroupSize.X, WorkGroupSize.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                    SetupTextureParams();
                    GL.BindImageTexture(0, OutputTexture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                    break;
                default:
                    break;
            }   
        }

        void SetupTextureParams()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)MagMinFilterTypes.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)MagMinFilterTypes.Nearest);
        }

        public int GetOutputRendererID()
        {
            return OutputTexture;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, OutputTexture);
        }

        public void Dispatch(int workGroupX, int WorkGroupY, int workGroupZ)
        {
            GL.DispatchCompute(workGroupX, WorkGroupY, workGroupZ);
        }

        public void Wait()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
        }

        public void CompileSource(int handle)
        {
            // Compiling the shaders
            GL.CompileShader(handle);

            // Getting Shader logs and printing
            string infoLog = GL.GetShaderInfoLog(handle);
            if (infoLog != System.String.Empty) System.Console.WriteLine(infoLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, handle);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, handle);
            GL.DeleteShader(handle);
        }

        public void ReCompile()
        {

            int computeShader;

            string ComputeShaderSource;

            using (StreamReader reader = new StreamReader(Cpath, Encoding.UTF8))
            {
                ComputeShaderSource = reader.ReadToEnd();
            }

            computeShader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(computeShader, ComputeShaderSource);
            CompileSource(computeShader);
        }

        public void SetFloatArray(float[] values)
        {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, WorkGroupSize.X, WorkGroupSize.Y, 0, PixelFormat.Red, PixelType.Float, values);
        }

        public int GetAttribLocation(string AttribName)
        {
            return GL.GetAttribLocation(Handle, AttribName);
        }

        public int GetUniformLocation(string UniformName)
        {
            if (UniformPositions.ContainsKey(UniformName))
            {
                return UniformPositions[UniformName];
            } else
            {
                UniformPositions.Add(UniformName, GL.GetUniformLocation(Handle, UniformName));
            }
            return GL.GetUniformLocation(Handle, UniformName);
        }

        public bool UniformExists(string name)
        {
            if (GetUniformLocation(name) != -1)
                return true;
            return false;
         }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);
                disposedValue = true;
            }
        }

        ~ComputeShader()
        {
            //if (!GL.IsProgram(Handle)) { return; }
            //GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
