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
    class Shader : IDisposable
    {

        public static List<Shader> Shaders = new List<Shader>();

        int Handle;

        string vPath, fPath;

        private bool disposedValue = false;

        public Shader(string vertexPath, string fragmentPath)
        {

            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            vPath = vertexPath;
            fPath = fragmentPath;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compiling the shaders
            GL.CompileShader(VertexShader);
            GL.CompileShader(FragmentShader);

            // Getting Shader logs and printing
            string infoVertLog = GL.GetShaderInfoLog(VertexShader);
            string infoFragLog = GL.GetShaderInfoLog(FragmentShader);
            if (infoVertLog != System.String.Empty) System.Console.WriteLine(infoVertLog);
            if (infoFragLog != System.String.Empty) System.Console.WriteLine(infoFragLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
            Shaders.Add(this);
        }

        public Shader(string Path)
        {

            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(Path + ".vert", Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(Path + ".frag", Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            vPath = Path + ".vert";
            fPath = Path + ".frag";

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compiling the shaders
            GL.CompileShader(VertexShader);
            GL.CompileShader(FragmentShader);

            // Getting Shader logs and printing
            string infoVertLog = GL.GetShaderInfoLog(VertexShader);
            string infoFragLog = GL.GetShaderInfoLog(FragmentShader);
            if (infoVertLog != System.String.Empty) System.Console.WriteLine(infoVertLog);
            if (infoFragLog != System.String.Empty) System.Console.WriteLine(infoFragLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
            Shaders.Add(this);
        }

        public Shader(bool fromSource, string VertexShaderSource, string FragmentShaderSource)
        {

            int VertexShader;
            int FragmentShader;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compiling the shaders
            GL.CompileShader(VertexShader);
            GL.CompileShader(FragmentShader);

            // Getting Shader logs and printing
            string infoVertLog = GL.GetShaderInfoLog(VertexShader);
            string infoFragLog = GL.GetShaderInfoLog(FragmentShader);
            if (infoVertLog != System.String.Empty) System.Console.WriteLine(infoVertLog);
            if (infoFragLog != System.String.Empty) System.Console.WriteLine(infoFragLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
            Shaders.Add(this);
        }

        public bool ExistsInAssetDB(string path)
        {
            Shader shader = AssetDatabase.Get(System.IO.Path.GetFileNameWithoutExtension(path));
            if (shader != null)
            {
                return true;
            }
            return false;
        }


        public void SetInt(string name, int value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetIntArray(string name, int[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                int location = this.GetUniformLocation(name + "["+i+"]");
                GL.Uniform1(location, value[i]);
            }
        }

        public void SetFloat(string name, float value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = this.GetUniformLocation(name);
            GL.UniformMatrix4(location, true, ref value);
        }

        public void SetMatrix4(string name, Matrix4 value, bool transpose)
        {
            int location = this.GetUniformLocation(name);
            GL.UniformMatrix4(location, transpose, ref value);
        }

        public void SetVector4(string name, Vector4 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform4(location, value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform3(location, value);
        }

        public void SetVector2(string name, Vector2 value)
        {
            int location = this.GetUniformLocation(name);
            GL.Uniform2(location, value);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void ReCompile()
        {

            int VertexShader;
            int FragmentShader;

            string VertexShaderSource;
            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(vPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            using (StreamReader reader = new StreamReader(fPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            // Compiling the shaders
            GL.CompileShader(VertexShader);
            GL.CompileShader(FragmentShader);

            // Getting Shader logs and printing
            string infoVertLog = GL.GetShaderInfoLog(VertexShader);
            string infoFragLog = GL.GetShaderInfoLog(FragmentShader);
            if (infoVertLog != System.String.Empty) System.Console.WriteLine(infoVertLog);
            if (infoFragLog != System.String.Empty) System.Console.WriteLine(infoFragLog);

            // Creating Shader Program
            Handle = GL.CreateProgram();

            // Attaching Frag and Vert shader to program
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Discarding Useless Resources
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);
        }

        public int GetAttribLocation(string AttribName)
        {
            return GL.GetAttribLocation(Handle, AttribName);
        }

        public int GetUniformLocation(string UniformName)
        {
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

        ~Shader()
        {
            if (!GL.IsProgram(Handle)) { return; }
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
