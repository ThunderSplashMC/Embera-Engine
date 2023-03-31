using System;
using System.Collections.Generic;

namespace DevoidEngine.Engine.Core
{
    class ShaderLibrary
    {

        static Dictionary<string, Shader> ShaderCache = new Dictionary<string, Shader>();

        public static void AddShader(string name, Shader shader)
        {
            Console.WriteLine(name);
            if (ShaderCache.ContainsKey(name))
            {
                ShaderCache[name] = shader;
            } else
            {
                ShaderCache.Add(name, shader);
            }
        }

        public static Shader GetShader(string name)
        {
            Shader shader;
            ShaderCache.TryGetValue(name, out shader);
            return shader;
        }

        public static void RemoveShader(string name)
        {
            ShaderCache.Remove(name);
        }
    }
}
